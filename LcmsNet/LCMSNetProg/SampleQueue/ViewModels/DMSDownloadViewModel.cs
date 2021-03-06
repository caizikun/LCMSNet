﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using LcmsNetDataClasses;
using LcmsNetSQLiteTools;
using ReactiveUI;

namespace LcmsNet.SampleQueue.ViewModels
{
    /// <summary>
    /// Form for retrieval of run requests from DMS
    /// </summary>
    public class DMSDownloadViewModel : ReactiveObject
    {
        #region "Properties"

        /// <summary>
        /// Connection string for DMS SQL Server connection
        /// </summary>
        public string DMSConnStr { get; set; }

        #endregion

        #region "Class variables"

        private List<classSampleData> dmsRequestList;
        private string matchString;
        private string cartName = string.Empty;
        private string cartConfigName = string.Empty;

        private string windowTitle = string.Empty;
        private string requestName = string.Empty;
        private string requestIdStart = string.Empty;
        private string requestIdEnd = string.Empty;
        private string cart = string.Empty;
        private string wellplate = string.Empty;
        private string batchId = string.Empty;
        private string block = string.Empty;
        private bool unassignedRequestsOnly;
        private readonly ReactiveList<string> lcCartComboBoxOptions = new ReactiveList<string>();
        private readonly ReactiveList<string> lcCartConfigComboBoxOptions = new ReactiveList<string>();
        private DMSDownloadDataViewModel availableRequestData = new DMSDownloadDataViewModel();
        private bool loadingData;
        private string requestsFoundString = string.Empty;
        private DMSDownloadDataViewModel selectedRequestData = new DMSDownloadDataViewModel();

        public string WindowTitle
        {
            get { return windowTitle; }
            private set { this.RaiseAndSetIfChanged(ref windowTitle, value); }
        }

        public string RequestName
        {
            get { return requestName; }
            set { this.RaiseAndSetIfChanged(ref requestName, value); }
        }

        public string RequestIdStart
        {
            get { return requestIdStart; }
            set { this.RaiseAndSetIfChanged(ref requestIdStart, value); }
        }

        public string RequestIdEnd
        {
            get { return requestIdEnd; }
            set { this.RaiseAndSetIfChanged(ref requestIdEnd, value); }
        }

        public string Cart
        {
            get { return cart; }
            set { this.RaiseAndSetIfChanged(ref cart, value); }
        }

        public string Wellplate
        {
            get { return wellplate; }
            set { this.RaiseAndSetIfChanged(ref wellplate, value); }
        }

        public string BatchId
        {
            get { return batchId; }
            set { this.RaiseAndSetIfChanged(ref batchId, value); }
        }

        public string Block
        {
            get { return block; }
            set { this.RaiseAndSetIfChanged(ref block, value); }
        }

        public bool UnassignedRequestsOnly
        {
            get { return unassignedRequestsOnly; }
            set
            {
                this.RaiseAndSetIfChanged(ref unassignedRequestsOnly, value);
                this.RaisePropertyChanged(nameof(AssignedRequestsOnly));
            }
        }

        public bool AssignedRequestsOnly
        {
            get { return !unassignedRequestsOnly; }
        }

        public string CartName
        {
            get { return cartName; }
            set { this.RaiseAndSetIfChanged(ref cartName, value); }
        }

        public IReadOnlyReactiveList<string> LcCartComboBoxOptions => lcCartComboBoxOptions;

        public string CartConfigName
        {
            get { return cartConfigName; }
            set { this.RaiseAndSetIfChanged(ref cartConfigName, value); }
        }

        public IReadOnlyReactiveList<string> LcCartConfigComboBoxOptions => lcCartConfigComboBoxOptions;

        public DMSDownloadDataViewModel AvailableRequestData
        {
            get { return availableRequestData; }
            private set { this.RaiseAndSetIfChanged(ref availableRequestData, value); }
        }

        public bool LoadingData
        {
            get { return loadingData; }
            private set { this.RaiseAndSetIfChanged(ref loadingData, value); }
        }

        public string RequestsFoundString
        {
            get { return requestsFoundString; }
            private set { this.RaiseAndSetIfChanged(ref requestsFoundString, value); }
        }

        public DMSDownloadDataViewModel SelectedRequestData
        {
            get { return selectedRequestData; }
            private set { this.RaiseAndSetIfChanged(ref selectedRequestData, value); }
        }

        #endregion

        #region "Event Handlers"

        /// <summary>
        /// Command for FIND button to load available request list from DMS
        /// </summary>
        public ReactiveCommand FindCommand { get; protected set; }

        /// <summary>
        /// Command for MoveDown button to move requests from Available Requests to Requests To Run list
        /// </summary>
        public ReactiveCommand MoveDownCommand { get; protected set; }

        /// <summary>
        /// Command for MoveUp button to move requests from Requests To Run to Available Requests list
        /// </summary>
        public ReactiveCommand MoveUpCommand { get; protected set; }

        /// <summary>
        /// Command for OK button to tell calling form that new DMS data is available
        /// </summary>
        public ReactiveCommand OkCommand { get; protected set; }

        /// <summary>
        /// Command to trigger list of carts in combo boxes to be updated
        /// </summary>
        public ReactiveCommand UpdateCartInfoCommand { get; protected set; }

        private void SetupCommands()
        {
            FindCommand = ReactiveCommand.Create(() => FindDmsRequests());
            MoveDownCommand = ReactiveCommand.Create(() => MoveRequestsToRunList());
            MoveUpCommand = ReactiveCommand.Create(() => RemoveRequestsFromRunList());
            OkCommand = ReactiveCommand.Create(() => UpdateDMSCartAssignment());
            UpdateCartInfoCommand = ReactiveCommand.Create(() => UpdateCartInfo());
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Update cart assignments in DMS
            //TODO: if (UpdateDMSCartAssignment())
            //TODO: {
            //TODO:     // Hide the form if update was successful
            //TODO:     Hide();
            //TODO:     DialogResult = DialogResult.OK;
            //TODO: }
        }

        /// <summary>
        /// Causes list of carts in combo boxes to be updated
        /// </summary>
        private void UpdateCartInfo()
        {
            UpdateCartList();
            UpdateCartConfigList();
        }

        #endregion

        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public DMSDownloadViewModel()
        {
            // Avoid exceptions caused from not being able to access program settings, when being run to provide design-time data context for the designer
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            // Initialize form controls
            InitFormControls();
            SetupCommands();
        }

        /// <summary>
        /// Loads form controls with initial data, where applicable
        /// </summary>
        private void InitFormControls()
        {
            // Form caption
            string dbInUse;
            try
            {
                if (LcmsNet.Configuration.clsDMSDataContainer.DBTools.DMSVersion.Contains("_T3"))
                {
                    dbInUse = " (Using Development Database)";
                }
                else
                {
                    dbInUse = " (Using Production Database)";
                }
            }
            catch (Exception ex)
            {
                dbInUse = " (Using ?? database: " + ex.Message + ")";
            }

            WindowTitle = "LcmsNet V" + Assembly.GetEntryAssembly().GetName().Version + dbInUse;

            // Load the LC cart lists
            UpdateCartList();
            UpdateCartConfigList();

            // Cart name
            CartName = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_CARTNAME);
            CartConfigName = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_CARTCONFIGNAME);

            if (CartName.ToLower() == classLCMSSettings.CONST_UNASSIGNED_CART_NAME)
            {
                // No cart name is assigned, user will need to select one
                CartName = "";
            }
        }

        /// <summary>
        /// Loads the LC cart dropdowns with data from cache
        /// </summary>
        private void UpdateCartList()
        {
            List<string> cartList;

            lcCartComboBoxOptions.Clear();

            // Get the list of carts from DMS
            try
            {
                cartList = classSQLiteTools.GetCartNameList();
            }
            catch (classDatabaseConnectionStringException ex)
            {
                // The SQLite connection string wasn't found
                var errMsg = ex.Message + " while getting LC cart listing.\r\n" +
                    "Please close LcmsNet program and correct the configuration file";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return;
            }
            catch (classDatabaseDataException ex)
            {
                // There was a problem getting the list of LC carts from the cache db
                var innerException = string.Empty;
                if (ex.InnerException != null)
                    innerException = ex.InnerException.Message;
                var errMsg = "Exception getting LC cart list from DMS: " + innerException + "\r\n" +
                    "As a workaround, you may manually type the cart name when needed.\r\n" +
                    "You may retry retrieving the cart list later, if desired.";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return;
            }

            if (cartList.Any())
            {
                using (lcCartComboBoxOptions.SuppressChangeNotifications())
                {
                    lcCartComboBoxOptions.AddRange(cartList);
                }
            }
        }

        /// <summary>
        /// Loads the LC cart config dropdown with data from cache
        /// </summary>
        private void UpdateCartConfigList()
        {
            List<string> cartConfigList;

            lcCartConfigComboBoxOptions.Clear();

            // Get the list of cart configuration names from DMS
            try
            {
                cartConfigList = classSQLiteTools.GetCartConfigNameList(false);
            }
            catch (classDatabaseConnectionStringException ex)
            {
                // The SQLite connection string wasn't found
                var errMsg = ex.Message + " while getting LC cart config name listing.\r\n" +
                    "Please close LcmsNet program and correct the configuration file";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return;
            }
            catch (classDatabaseDataException ex)
            {
                // There was a problem getting the list of LC carts from the cache db
                var innerException = string.Empty;
                if (ex.InnerException != null)
                    innerException = ex.InnerException.Message;
                var errMsg = "Exception getting LC cart config name list from DMS: " + innerException + "\r\n" +
                    "As a workaround, you may manually type the cart config name when needed.\r\n" +
                    "You may retry retrieving the cart list later, if desired.";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return;
            }

            if (cartConfigList.Any())
            {
                using (LcCartConfigComboBoxOptions.SuppressChangeNotifications())
                {
                    lcCartConfigComboBoxOptions.AddRange(cartConfigList);
                }
            }
        }
        /// <summary>
        /// Loads listViewAvailableRequests with all requests in DMS matching specified criteria
        /// </summary>
        private async void FindDmsRequests()
        {
            List<classSampleData> tempRequestList;

            // Fill an object with the data from the UI, then pass to DMSTools class to run the query
            var queryData = new classSampleQueryData {
                RequestName = this.RequestName
            };

            // If min request number input is not specified, set it to 0
            if (string.IsNullOrEmpty(RequestIdStart))
            {
                queryData.MinRequestNum = "0";
            }
            else
            {
                queryData.MinRequestNum = RequestIdStart;
            }

            //If max request number input is not specified, set it to 1,000,000,000
            if (string.IsNullOrEmpty(RequestIdEnd))
            {
                queryData.MaxRequestNum = "1000000000";
            }
            else
            {
                queryData.MaxRequestNum = RequestIdEnd;
            }

            // If "unassigned only" is checked, override cart selection combo box
            //string cartToFind;
            if (UnassignedRequestsOnly)
            {
                queryData.Cart = "unknown";
            }
            else
            {
                queryData.Cart = Cart;
            }

            queryData.BatchID = BatchId;
            queryData.Block = Block;
            queryData.Wellplate = Wellplate;

            // Blank listview and display wait message
            LoadingData = true;

            // Clear the available datasets listview
            AvailableRequestData.Data.Clear();

            // TODO: Async from here
            /*// Get a list of requests from DMS
            try
            {
                var dmsTools = LcmsNet.Configuration.clsDMSDataContainer.DBTools;
                tempRequestList = dmsTools.GetSamplesFromDMS(queryData);
            }
            catch (classDatabaseConnectionStringException ex)
            {
                // The DMS connection string wasn't found
                var errMsg = ex.Message + " while getting request listing\r\n";
                errMsg = errMsg + "Please close LcmsNet program and correct the configuration file";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButtons.OK);
                return;
            }
            catch (classDatabaseDataException ex)
            {
                var errMsg = ex.Message;
                if (ex.InnerException != null)
                    errMsg += ": " + ex.InnerException.Message;

                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButtons.OK);
                return;
            }
            finally
            {
                LoadingData = false;
            }
            */
            // TODO: Async to here...

            tempRequestList = await Task.Run(() => GetDMSData(queryData));

            if (tempRequestList == null)
            {
                return;
            }

            // Check to see if any items were found
            LoadingData = false;
            if (!tempRequestList.Any())
            {
                MessageBox.Show("No requests found in DMS");
                RequestsFoundString = "0 requests found";
                return;
            }
            RequestsFoundString = tempRequestList.Count + " requests found";

            // Add the requests to the listview
            if (dmsRequestList == null)
            {
                dmsRequestList = new List<classSampleData>();
            }

            var availReqList = new List<classSampleData>();
            foreach (var request in tempRequestList)
            {
                // Determine if already in list of requests
                matchString = request.DmsData.RequestName;
                var foundIndx = dmsRequestList.FindIndex(PredContainsRequestName);
                if (foundIndx == -1)
                {
                    // Request not found, so add to list of all requests
                    dmsRequestList.Add(request);
                    // Add request data to the listview
                    availReqList.Add(request);
                }
                else
                {
                    if (dmsRequestList[foundIndx].DmsData.SelectedToRun)
                    {
                        // Request was found and is already in Requests To Run list, so do nothing
                    }
                    else
                    {
                        availReqList.Add(request);
                    }
                }
            }

            using (AvailableRequestData.Data.SuppressChangeNotifications())
            {
                AvailableRequestData.Data.AddRange(availReqList);
            }

            // Hide the wait message and display the listview again
//              listviewAvailableRequests.EndUpdate();
            LoadingData = false;
//              classStatusTools.SendStatusMsg("Found " + listviewAvailableRequests.Items.Count.ToString() + " requests in DMS");

            // Check to see if any items have blocking enabled
            if (IsBlockingEnabled(tempRequestList))
            {
                var msg =
                    "You have downloaded samples that have blocking enabled. Please be sure you have downloaded the " +
                    "entire block of samples needed";
                MessageBox.Show(msg, "Blocked Samples Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private List<classSampleData> GetDMSData(classSampleQueryData queryData)
        {
            var tempRequestList = new List<classSampleData>();

            // Get a list of requests from DMS
            try
            {
                var dmsTools = LcmsNet.Configuration.clsDMSDataContainer.DBTools;
                tempRequestList = dmsTools.GetRequestedRunsFromDMS(queryData);
            }
            catch (classDatabaseConnectionStringException ex)
            {
                // The DMS connection string wasn't found
                var errMsg = ex.Message + " while getting request listing\r\n";
                errMsg = errMsg + "Please close LcmsNet program and correct the configuration file";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return null;
            }
            catch (classDatabaseDataException ex)
            {
                var errMsg = ex.Message;
                if (ex.InnerException != null)
                    errMsg += ": " + ex.InnerException.Message;

                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return null;
            }
            finally
            {
                LoadingData = false;
            }

            return tempRequestList;
        }

        /// <summary>
        /// Tests downloaded samples to determine if any have blocking enabled
        /// </summary>
        /// <param name="inputData">List containing downloaded samples</param>
        /// <returns>TRUE if any samples have blcoking enabled; otherwise FALSE</returns>
        private bool IsBlockingEnabled(List<classSampleData> inputData)
        {
            foreach (var testSample in inputData)
            {
                if (testSample.DmsData.Block > 0)
                {
                    // Blocking is enabled for this sample, no furhter test required
                    return true;
                }
            }
            // If we got to here, no samples have blocking enabled
            return false;
        }

        /// <summary>
        /// Moves items from the Available Requests list to the Requests To Run list
        /// </summary>
        private void MoveRequestsToRunList()
        {
            // Copy selected items and update main list of DMS items
            using (AvailableRequestData.Data.SuppressChangeNotifications())
            using (SelectedRequestData.Data.SuppressChangeNotifications())
            {
                foreach (var tempItem in AvailableRequestData.SelectedData)
                {
                    // Move the selected items
                    SelectedRequestData.Data.Add(tempItem);
                    // Update main list of DMS items
                    tempItem.DmsData.SelectedToRun = true;
                    // Remove the selected items from the Available Requests listview
                    AvailableRequestData.Data.Remove(tempItem);
                }
            }
        }

        /// <summary>
        /// Removes requests from the "Requests to run" list
        /// </summary>
        private void RemoveRequestsFromRunList()
        {
            using (AvailableRequestData.Data.SuppressChangeNotifications())
            using (SelectedRequestData.Data.SuppressChangeNotifications())
            {
                foreach (var tempItem in SelectedRequestData.SelectedData)
                {
                    // Update main list of DMS items
                    if (!dmsRequestList.Remove(tempItem))
                    {
                        var tempStr = "Request " + tempItem.DmsData.RequestName + " not found in requested run collection";
                        MessageBox.Show(tempStr);
                        return;
                    }
                    // Remove the selected items from the Requests To Run listview
                    SelectedRequestData.Data.Remove(tempItem);
                }
            }
        }

        /// <summary>
        /// Predicate function to find index of request matching specified request number
        /// Used for m_DmsRequestList FindIndex method
        /// </summary>
        /// <param name="request">classDMSData object passed in from FindIndex method</param>
        /// <returns>True if match is made; otherwise False</returns>
        private bool PredContainsRequestName(classSampleData request)
        {
            if (string.Equals(request.DmsData.RequestName, matchString, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clears all data on the DMS form
        /// </summary>
        public void ClearForm()
        {
            AvailableRequestData.Data.Clear();
            SelectedRequestData.Data.Clear();
            dmsRequestList.Clear();
        }

        /// <summary>
        /// Transfers the current list of classDMSData objects that have been selected
        /// for running to the calling program as a list of classSampleData objects
        /// </summary>
        /// <returns>List of classSampleData objects</returns>
        public List<classSampleData> GetNewSamplesDMSView()
        {
            var retList = new List<classSampleData>();

            foreach (var tempSampleData in SelectedRequestData.Data)
            {
                tempSampleData.DmsData.CartName = cartName;
                tempSampleData.DmsData.CartConfigName = cartConfigName;

                //                  classSampleData tempSampleData = CopyDMSDataObj(tempDMSData);
                retList.Add(tempSampleData);
            }
//              classStatusTools.SendStatusMsg("Adding " + retList.Count.ToString() + " samples from DMS");
            return retList;
        }

        /// <summary>
        /// Updates selected requests in DMS to show new cart assignment
        /// </summary>
        /// <returns></returns>
        bool UpdateDMSCartAssignment()
        {
            // Verify a cart is specified
            if (cartName.ToLower() == classLCMSSettings.CONST_UNASSIGNED_CART_NAME)
            {
                MessageBox.Show("Cart name must be specified", "CART NAME NOT SPECIFIED");
                return false;
            }

            // Update the cart assignments in DMS
            var reqIDs = "";

            // Build a string of request IDs for stored procedure call
            foreach (var tempDMSData in dmsRequestList)
            {
                if (tempDMSData.DmsData.SelectedToRun)
                {
                    if (reqIDs.Length != 0)
                    {
                        reqIDs = reqIDs + "," + tempDMSData.DmsData.RequestID;
                    }
                    else
                    {
                        reqIDs = tempDMSData.DmsData.RequestID.ToString();
                    }
                }
            }

            // Call the DMS stored procedure to update the cart assignments
            bool success;

            var dmsTools = LcmsNet.Configuration.clsDMSDataContainer.DBTools;
            try
            {
                success = dmsTools.UpdateDMSCartAssignment(reqIDs, cartName, cartConfigName, true);
            }
            catch (classDatabaseConnectionStringException ex)
            {
                // The DMS connection string wasn't found
                var errMsg = ex.Message + " while getting LC cart listing\r\n";
                errMsg = errMsg + "Please close LcmsNet program and correct the configuration file";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return false;
            }
            catch (classDatabaseDataException ex)
            {
                var errMsg = ex.Message;
                if (ex.InnerException != null)
                    errMsg += ": " + ex.InnerException.Message;

                errMsg = errMsg + "\r\n\r\n Requests in DMS may not show correct cart assignments";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return true;
            }
            catch (classDatabaseStoredProcException ex)
            {
                var errMsg = "Error " + ex.ReturnCode + " while executing stored procedure ";
                errMsg = errMsg + ex.ProcName + ": " + ex.ErrMessage;
                errMsg = errMsg + "\r\n\r\nRequests in DMS may not show correct cart assignments";
                MessageBox.Show(errMsg, "LcmsNet", MessageBoxButton.OK);
                return true;
            }

            if (!success)
            {
                MessageBox.Show(dmsTools.ErrMsg, "LcmsNet",
                    MessageBoxButton.OK);
                return false;
            }

            return true;
        }

        #endregion

    }
}
