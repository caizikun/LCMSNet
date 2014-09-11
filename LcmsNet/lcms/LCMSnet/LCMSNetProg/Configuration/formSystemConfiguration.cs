﻿//*********************************************************************************************************
// Written by Dave Clark, Brian LaMarche for the US Department of Energy 
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2010, Battelle Memorial Institute
// Created 07/23/2010
//
// Last modified 07/23/2010
//						08/20/2010 (DAC) - Modified to save config settings during program shutdown
//						08/31/2010 (DAC) - Changes resulting from move to LcmsNet namespace
//						09/01/2010 (DAC) - Modified to allow operator reload of column names
//*********************************************************************************************************
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using LcmsNetDataClasses;
using LcmsNetDataClasses.Configuration;
using LcmsNetDataClasses.Logging;

using LcmsNetSQLiteTools;
using LcmsNetDmsTools;

namespace LcmsNet.Configuration
{
	#region "Namespace delegates"
		internal delegate void DelegateUpdateStatus(object sender, enumColumnStatus previousStatus, enumColumnStatus newStatus);
	#endregion

    /// <summary>
    /// Displays application and cart configurations.
    /// </summary>
	public partial class formSystemConfiguration : Form
	{
		#region "Class variables"
        private const int CONST_TOTAL_COLUMNS = 4;
		/// <summary>
		/// List of available separation types
		/// </summary>
        private List<string> mobj_SeparationTypes;
        /// <summary>
        /// List of users
        /// </summary>
        private Dictionary<string, classUserInfo> mobj_Users;
		#endregion

        public event EventHandler ColumnNameChanged;
        bool m_isLoading;

		#region "Constructors"
		/// <summary>
		///  Default constructor for displaying column data.
		/// </summary>
		public formSystemConfiguration()
		{
            m_isLoading = true;
			InitializeComponent();
            m_isLoading = false;

			Initialize();
		}	
		#endregion

        #region "Properties"
        /// <summary>
        /// Sets the list of column names.
        /// </summary>
        public List<string> ColumnNames
        {
            set
            {
                mcontrol_columnOne.ColumnNames = value;
                mcontrol_columnTwo.ColumnNames = value;
                mcontrol_columnThree.ColumnNames = value;
                mcontrol_columnFour.ColumnNames = value;
            }
        }

        /// <summary>
        /// Separation types
        /// </summary>
        public List<string> SeparationTypes
        {
            set
            {
                mobj_SeparationTypes = value;
                mcombo_SepType.Items.Clear();
                foreach (string sepType in mobj_SeparationTypes)
                {
                    mcombo_SepType.Items.Add(sepType);
                }
            }
        }
        /// <summary>
        /// User list
        /// </summary>
        public List<classUserInfo> Users
        {
            set { LoadUserCombo(value); }
        }

        public string PdfPath
        {
            set
            {
                txtPdfPath.Text = value;
            }
        }

        #endregion

		#region "Methods"
		/// <summary>
		/// Initializes data for the controls.
		/// </summary>
		private void Initialize()
		{
            mtextbox_triggerLocation.Text   = classLCMSSettings.GetParameter("TriggerFileFolder");
            txtPdfPath.Text = classLCMSSettings.GetParameter("PdfPath");
			mcontrol_columnOne.ColumnData   = classCartConfiguration.Columns[0];
			mcontrol_columnTwo.ColumnData   = classCartConfiguration.Columns[1];
			mcontrol_columnThree.ColumnData = classCartConfiguration.Columns[2];
			mcontrol_columnFour.ColumnData  = classCartConfiguration.Columns[3];

            RegisterColumn(mcontrol_columnOne);
            RegisterColumn(mcontrol_columnTwo);
            RegisterColumn(mcontrol_columnThree);
            RegisterColumn(mcontrol_columnFour);

			// Cart name
			mlabel_Cart.Text = classLCMSSettings.GetParameter("CartName");

            LoadInstrumentInformation();
            LoadApplicationSettings();
            
            numericUpDown1.Value = Convert.ToDecimal(classCartConfiguration.MinimumVolume);

            //load time zones into combobox
            foreach (TimeZoneInfo tzi in TimeZoneInfo.GetSystemTimeZones())
            {
                comboTimeZone.Items.Add(tzi.Id);
            }

            comboTimeZone.SelectedIndex = comboTimeZone.FindStringExact(classLCMSSettings.GetParameter("TimeZone"));
		}
 
        void RegisterColumn(controlColumn column)
        {
            column.ColumnNamesChanged += new controlColumn.delegateColumnNamesChanged(mcontrol_columnOne_ColumnNamesChanged);
        }

        void mcontrol_columnOne_ColumnNamesChanged()
        {
            if (ColumnNameChanged != null)
                ColumnNameChanged(this, null);
        }
        private void LoadInstrumentInformation()
        {
            // Load combo box
            List<classInstrumentInfo> instList = classSQLiteTools.GetInstrumentList(false);

            if (instList == null)
            {
                classApplicationLogger.LogError(0, "Instrument list retrieval returned null.");                    
                return;
            }

            if (instList.Count < 1)
            {
                classApplicationLogger.LogError(0, "No instruments found.");                    
                return;
            }

            comboBoxAvailInstruments.Items.Clear();
            foreach (classInstrumentInfo instData in instList)
            {
                comboBoxAvailInstruments.Items.Add(instData.DMSName);
            }

            // Determine if presently specified instrument name is in list. If it is, display it.
            string currentName = classLCMSSettings.GetParameter("InstName");
            int indx = 0;
            bool found = false;
            foreach (string itemName in comboBoxAvailInstruments.Items)
            {
                if (itemName == currentName)
                {
                    found = true;
                    break;
                }
                indx++;
            }

            if (found)
            {
                comboBoxAvailInstruments.SelectedIndex = indx;
            }
            else comboBoxAvailInstruments.SelectedIndex = 0;


            // Determine if presently specified instrument name is in list. If it is, display it.
            currentName = classLCMSSettings.GetParameter("SeparationType");
            indx = 0;
            found = false;
            foreach (string itemName in mcombo_SepType.Items)
            {
                if (itemName == currentName)
                {
                    found = true;
                    break;
                }
                indx++;
            }

            if (found)
            {
                mcombo_SepType.SelectedIndex = indx;
            }
            else if (mcombo_SepType.Items.Count > 0)
            {
                mcombo_SepType.SelectedIndex = 0;
            }

            mbutton_accept.BackColor = System.Drawing.Color.FromName("ButtonHighlight");
        }
        /// <summary>
        /// Loads the application settings to the user interface.
        /// </summary>
        private void LoadApplicationSettings()
        {
            //mcheckBox_createTriggerFiles.Checked = Convert.ToBoolean(classLCMSSettings.GetParameter("CreateTriggerFiles"));
            //mcheckBox_copyTriggerFiles.Checked = Convert.ToBoolean(classLCMSSettings.GetParameter("CopyTriggerFiles"));
            //mcheckBox_createMethodFolders.Checked = Convert.ToBoolean(classLCMSSettings.GetParameter("CreateMethodFolders"));
            //mcheckBox_copyMethodFolders.Checked = Convert.ToBoolean(classLCMSSettings.GetParameter("CopyMethodFolders"));            
        }

        /// <summary>
        /// Loads the user combo box
        /// </summary>
        /// <param name="userList"></param>
        private void LoadUserCombo(List<classUserInfo> userList)
        {
            // Make a dictionary based on the input list, with the first entry = "(None)"
            if (mobj_Users == null)
            {
                mobj_Users = new Dictionary<string, classUserInfo>();
            }
            else mobj_Users.Clear();

            // Create dummy user. User is not recognized by DMS, so that trigger files will fail and let
            //		operator know that user name was not provided
            classUserInfo tmpUser = new classUserInfo();
            tmpUser.PayrollNum = "None";
            tmpUser.UserName = "(None)";
            mobj_Users.Add(tmpUser.PayrollNum, tmpUser);

            // Add users to dictionary from user list
            foreach (classUserInfo currUser in userList)
            {
                var data = string.Format("{0} - ({1})",     currUser.UserName ,        
                                                            currUser.PayrollNum);

                mobj_Users.Add(data, currUser);
            }

            // Now add user list to combo box
            mcombo_Operator.Items.Clear();
            foreach (KeyValuePair<string, classUserInfo> currKey in mobj_Users)
            {
                mcombo_Operator.Items.Add(string.Format("{0} - ({1})",
                                                        currKey.Value.UserName ,        
                                                        currKey.Value.PayrollNum));
            }

            // Set combo box to display first entry
            mcombo_Operator.SelectedIndex = 0;


            // Determine if presently specified operator name is in list. If it is, display it.
            string currentName = classLCMSSettings.GetParameter("Operator");
            int  indx  = 0;
            bool found = false;
            foreach (string itemName in mcombo_Operator.Items)
            {
                string[] array = itemName.Split(new char[] { '-' });
                if (array[0].Trim() == currentName)
                {
                    found = true;
                    break;
                }
                indx++;
            }

            if (found)
            {
                mcombo_Operator.SelectedIndex = indx;
            }
            else if (mcombo_Operator.Items.Count > 0)
            {
                mcombo_Operator.SelectedIndex = 0;
            }
            mbutton_acceptOperator.BackColor = System.Drawing.Color.FromName("ButtonHighlight");
        }	
		/// <summary>
		/// Sets the separation type
		/// </summary>
		/// <param name="separationType">New separation type</param>
		public void SetSeparationType(string separationType)
		{
			int indx = mcombo_SepType.FindString(separationType, -1);
			if (indx == -1)
			{
				if (mcombo_SepType.Items.Count > 0)
				 mcombo_SepType.SelectedIndex = 0;
			}
			else
			{
				mcombo_SepType.SelectedIndex = indx;
			}
		}	
		#endregion

		#region Column events 
        ///// <summary>
        ///// Handles the event when the column status is changed.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="previousStatus"></param>
        ///// <param name="newStatus"></param>
        //void column_StatusChanged(object sender, enumColumnStatus previousStatus, enumColumnStatus newStatus)
        //{
        //    if (InvokeRequired == true)
        //    {
        //        BeginInvoke(new DelegateUpdateStatus(StatusChanged), new object[] { sender, previousStatus, newStatus });
        //    }
        //    else
        //    {
        //        StatusChanged(sender, previousStatus, newStatus);
        //    }
        //}				
		#endregion

		#region Form Events			
		private void mcombo_SepType_SelectedIndexChanged(object sender, EventArgs e)
			{
				classLCMSSettings.SetParameter("SeparationType",mcombo_SepType.Text);
                classSQLiteTools.SaveSelectedSeparationType(classLCMSSettings.GetParameter("SeparationType"));
			}	
		private void mbutton_Reload_Click(object sender, EventArgs e)
			{
				// Get a fresh list of columns from DMS and store it in the cache db
				classDBTools.GetColumnListFromDMS();

				// Get the new list of columns from the cache db
				List<string> columnList = classSQLiteTools.GetColumnList(true);

				// If a valid list was received, then update the display
				if (columnList == null)
				{
					// No new column list obtained
					classApplicationLogger.LogError(0, "Column name list null when refreshing list");
					MessageBox.Show("List not updated. Column name list from DMS is null");
					return;
				}
				if (columnList.Count < 1)
				{
					// No names found in list
					classApplicationLogger.LogError(0, "No column names found when refreshing list");
					MessageBox.Show("List not updated. No column names found.");
					return;
				}

				// Everything was OK, so update the list
				mcontrol_columnOne.ColumnNames      = columnList;
				mcontrol_columnTwo.ColumnNames      = columnList;
				mcontrol_columnThree.ColumnNames    = columnList;
				mcontrol_columnFour.ColumnNames     = columnList;

				classApplicationLogger.LogMessage(classApplicationLogger.CONST_STATUS_LEVEL_USER, "Column name lists updated");
			}	
		private void buttonAccept_Click(object sender, EventArgs e)
        {
            if (!m_isLoading)
                classLCMSSettings.SetParameter("InstName", comboBoxAvailInstruments.SelectedItem.ToString());
                mbutton_accept.BackColor = System.Drawing.Color.FromName("ButtonHighlight");
        }
        private void comboBoxAvailInstruments_SelectedIndexChanged(object sender, EventArgs e)
        {
            mbutton_accept.BackColor = System.Drawing.Color.Red;
        }
        private void mbutton_acceptOperator_Click(object sender, EventArgs e)
        {
            if (!m_isLoading)
            {
                var operatorName = mcombo_Operator.SelectedItem.ToString();
                if (mobj_Users.ContainsKey(operatorName))
                {
                    var instrumentOperator = mobj_Users[operatorName];
                    classLCMSSettings.SetParameter("Operator", instrumentOperator.UserName);
                }
                else
                {
                    classApplicationLogger.LogError(0, "Could not use the current user as the operator.  Was not present in the system.");
                }
            }
            mbutton_acceptOperator.BackColor = System.Drawing.Color.FromName("ButtonHighlight");
        }

        private void mcombo_Operator_SelectedIndexChanged(object sender, EventArgs e)
        {
            mbutton_acceptOperator.BackColor = System.Drawing.Color.Red;
        }  
        
        
        #endregion

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (!m_isLoading)
                classCartConfiguration.MinimumVolume = Convert.ToDouble(numericUpDown1.Value);
        }

        private void btnSetPdfPath_Click(object sender, EventArgs e)
        {
            string path = txtPdfPath.Text;
            if (Directory.Exists(path))
            {
                classLCMSSettings.SetParameter("PdfPath", path);
            }
            else
            {
                DialogResult = MessageBox.Show("That directory does not exist, create it?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                if (DialogResult == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Unable to create directory" + path, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
                else
                {
                    txtPdfPath.SelectAll(); 
                }
            }
        }
        
        private void comboTimeZone_SelectedValueChanged(object sender, EventArgs e)
        {
            classLCMSSettings.SetParameter("TimeZone", comboTimeZone.SelectedItem.ToString());
        }
       

    }	
}
