﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LcmsNet.Method.Forms;
using LcmsNet.SampleQueue.IO;
using LcmsNet.SampleQueue.Views;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Experiment;
using LcmsNetDataClasses.Logging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ReactiveUI;

namespace LcmsNet.SampleQueue.ViewModels
{
    public class SampleManagerViewModel : ReactiveObject
    {
        #region "Events"

        /// <summary>
        /// Fired when a sample run should be stopped.
        /// </summary>
        public event EventHandler Stop;

        #endregion

        #region  Members

        /// <summary>
        /// Default extension for the queue files.
        /// </summary>
        private const string CONST_DEFAULT_QUEUE_EXTENSION = ".que";

        /// <summary>
        /// Reference to the DMS View.
        /// </summary>
        private DMSDownloadViewModel dmsView;

        private string lastSavedFileName = "queue.que";

        /// <summary>
        /// Manages adding the samples to the queue.
        /// </summary>
        private classSampleQueue sampleQueue;

        private const int TIME_SYNCH_WAIT_TIME_MILLISECONDS = 2000;

        private SampleControlViewModel sampleControlViewModel;
        private ColumnManagerViewModel columnManagerViewModel;
        private MethodManagerViewModel methodManagerViewModel;
        private SampleDataManager sampleDataManager;

        public SampleControlViewModel SampleControlViewModel
        {
            get { return sampleControlViewModel; }
            private set { this.RaiseAndSetIfChanged(ref sampleControlViewModel, value); }
        }

        public ColumnManagerViewModel ColumnManagerViewModel
        {
            get { return columnManagerViewModel; }
            private set { this.RaiseAndSetIfChanged(ref columnManagerViewModel, value); }
        }

        public MethodManagerViewModel MethodManagerViewModel
        {
            get { return methodManagerViewModel; }
            private set { this.RaiseAndSetIfChanged(ref methodManagerViewModel, value); }
        }

        public SampleDataManager SampleDataManager
        {
            get { return sampleDataManager; }
            private set { this.RaiseAndSetIfChanged(ref sampleDataManager, value); }
        }

        private SynchronizationContext synchronizationContext;

        #endregion

        #region "Constructors"

        /// <summary>
        /// Default constructor that takes cart configuration data.
        /// </summary>
        /// <param name="queue">Sample queue to provide interface to.</param>
        public SampleManagerViewModel(classSampleQueue queue)
        {
            Initialize(queue);
            synchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Default constructor for design time use.
        /// </summary>
        [Obsolete("For WPF Design time use only.", true)]
        public SampleManagerViewModel()
        {
        }

        #endregion

        #region "methods"

        /// <summary>
        /// Initialization code.
        /// </summary>
        /// <param name="queue"></param>
        private void Initialize(classSampleQueue queue)
        {
            dmsView = new DMSDownloadViewModel();
            sampleQueue = queue;
            SetupCommands();

            if (sampleQueue != null)
            {
                sampleQueue.SamplesWaitingToRun += m_sampleQueue_SamplesWaitingToRun;
            }

            // Load up the data to the appropiate sub-controls.
            SampleDataManager = new SampleDataManager(sampleQueue);
            SampleControlViewModel = new SampleControlViewModel(dmsView, SampleDataManager);
            ColumnManagerViewModel = new ColumnManagerViewModel(dmsView, SampleDataManager);
            MethodManagerViewModel = new MethodManagerViewModel(dmsView, SampleDataManager);

            var palMethods = new List<string>();
            for (var i = 0; i < 6; i++)
            {
                SampleDataManager.AutoSamplerMethods.Add("method" + i);
                SampleDataManager.AutoSamplerTrays.Add("defaultTray0" + i);
            }

            // TODO: // This is the text that is appended to the application title bar
            TitleBarTextAddition = "Sample Queue - " + classLCMSSettings.GetParameter(classLCMSSettings.PARAM_CACHEFILENAME);
        }

        public void PreviewAvailable(object sender, SampleProgressPreviewArgs e)
        {
            if (e?.PreviewImage == null)
                return;

            try
            {
                SequencePreview = e.PreviewImage.ToBitmapImage();
            }
            catch (Exception)
            {
                // Ignore exceptions here
            }
            e.Dispose();
        }

        private BitmapImage sequencePreview;

        public BitmapImage SequencePreview
        {
            get { return sequencePreview; }
            private set { this.RaiseAndSetIfChanged(ref sequencePreview, value); }
        }

        private delegate void DelegateToggleButtons(classSampleQueueArgs args);

        private bool isRunButtonEnabled;
        private bool isStopButtonEnabled;
        private SolidColorBrush runButtonBackColor;
        private SolidColorBrush stopButtonBackColor;
        private string titleBarTextAddition = "";

        public bool IsRunButtonEnabled
        {
            get { return isRunButtonEnabled; }
            private set { this.RaiseAndSetIfChanged(ref isRunButtonEnabled, value); }
        }

        public bool IsStopButtonEnabled
        {
            get { return isStopButtonEnabled; }
            private set { this.RaiseAndSetIfChanged(ref isStopButtonEnabled, value); }
        }

        public SolidColorBrush RunButtonBackColor
        {
            get { return runButtonBackColor; }
            private set { this.RaiseAndSetIfChanged(ref runButtonBackColor, value); }
        }

        public SolidColorBrush StopButtonBackColor
        {
            get { return stopButtonBackColor; }
            private set { this.RaiseAndSetIfChanged(ref stopButtonBackColor, value); }
        }

        public string TitleBarTextAddition
        {
            get { return titleBarTextAddition; }
            private set { this.RaiseAndSetIfChanged(ref titleBarTextAddition, value); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="runButtonState"></param>
        /// <param name="stopButtonState"></param>
        void ToggleRunButton(bool runButtonState, bool stopButtonState)
        {
            IsRunButtonEnabled = runButtonState;
            if (runButtonState)
            {
                RunButtonBackColor = Brushes.LimeGreen;
            }
            else
            {
                RunButtonBackColor = Brushes.White;
            }
            IsStopButtonEnabled = stopButtonState;
            if (stopButtonState)
            {
                StopButtonBackColor = Brushes.Tomato;
            }
            else
            {
                StopButtonBackColor = Brushes.White;
            }
        }

        private void DetermineIfShouldSetButtons(classSampleQueueArgs data)
        {
            var runningCount = data.RunningSamplePosition;
            var totalSamples = data.RunningQueueTotal;

            if (runningCount > 0 && totalSamples > 0)
            {
                ToggleRunButton(false, true);
            }
            else if (runningCount <= 0 && totalSamples > 0)
            {
                ToggleRunButton(true, false);
            }
            else
            {
                ToggleRunButton(false, false);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void m_sampleQueue_SamplesWaitingToRun(object sender, classSampleQueueArgs data)
        {
            synchronizationContext.Post(d => DetermineIfShouldSetButtons(data), sender);
            //DetermineIfShouldSetButtons(data);
        }

        /// <summary>
        /// Supplies the list of PAL trays to the sample queue and associated objects.
        /// </summary>
        public void AutoSamplerTrayList(object sender, classAutoSampleEventArgs args)
        {
            var trays = args.TrayList;

            SampleDataManager.AutoSamplerTrays = trays;
        }

        /// <summary>
        /// Supplies a list of instrument methods to the sample queue and associated objects.
        /// </summary>
        public void InstrumentMethodList(object sender, classNetworkStartEventArgs args)
        {
            var methods = args.MethodList;

            SampleDataManager.InstrumentMethods = methods;
        }

        #endregion

        #region Exporting and Importing

        /// <summary>
        /// Handles exporting the queue to a csv file, xml file, or LCMS sample queue.
        /// </summary>
        private void ExportQueue(string name, ISampleQueueWriter writer)
        {
            if (writer == null)
            {
                classApplicationLogger.LogError(0, "The file type for exporting was not recognized.");
                return;
            }
            try
            {
                sampleQueue.SaveQueue(name, writer, true);
                classApplicationLogger.LogMessage(0, string.Format("The queue was exported to {0}.", name));
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, "Export Failed!  " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Exports the MRM Files
        /// </summary>
        public void ExportMRMFiles()
        {
            var folderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
            };

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var mrmFilePath = folderDialog.FileName;
                var mrmWriter = new classMRMFileExporter();
                sampleQueue.SaveQueue(mrmFilePath, mrmWriter, true);
            }
        }

        #endregion

        #region Form Event Handlers

        /// <summary>
        /// Imports the queue into LCMS.
        /// </summary>
        public void ImportQueue()
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "Load Queue",
                Filter = "LCMSNet Queue (*.que)|*.que|LCMS VB6 XML File (*.xml)|*.xml"
            };
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ISampleQueueReader reader = null;
                var extension = Path.GetExtension(fileDialog.FileName);

                switch (extension)
                {
                    case ".xml":
                        reader = new classQueueImportXML();
                        break;
                    case CONST_DEFAULT_QUEUE_EXTENSION:
                        reader = new classQueueImportSQLite();
                        break;
                }

                try
                {
                    sampleQueue.LoadQueue(fileDialog.FileName, reader);
                    classApplicationLogger.LogMessage(0, string.Format("The queue was successfully imported from {0}.", fileDialog.FileName));
                }
                catch (Exception ex)
                {
                    classApplicationLogger.LogError(0, string.Format("Could not load the queue {0}", fileDialog.FileName), ex);
                }
            }
        }

        /// <summary>
        /// Stops the sample run.
        /// </summary>
        private void StopQueue()
        {
            // This tells anyone else using the samples to STOP!
            // For the scheduler this would tell him to stop first
            // so that he can move the samples appropiately.
            Stop?.Invoke(this, new EventArgs());

            // Moves the samples from the running queue back onto the
            // waiting queue.
            sampleQueue.StopRunningQueue();
            ToggleRunButton(true, false);
        }

        /// <summary>
        /// Example of running a sequence for testing.
        /// </summary>
        private void RunQueue()
        {
            if (sampleQueue.IsRunning)
            {
                classApplicationLogger.LogMessage(0, "Samples are already running.");
                return;
            }
            var samples = sampleQueue.GetRunningQueue();

            // Remove any samples that have already been run, waiting to run, or had an error (== has run).
            samples.RemoveAll(
                delegate (classSampleData data) { return data.RunningStatus != enumSampleRunningStatus.WaitingToRun; });

            if (samples.Count < 1)
                return;

            // Make sure the samples pass the minimum QA/QC checks before running!
            // These checks include seeing if the sample has a valid method.
            // Seeing if the sample's method has all of the devices present in the method.
            // Later we will add to make sure none of the devices have an error that has
            // been thrown on them.
            var errors = new Dictionary<classSampleData, List<classSampleValidationError>>();

            foreach (var reference in classSampleValidatorManager.Instance.Validators)
            {
                var validator = reference.Value;
                foreach (var sample in samples)
                {
                    var error = validator.ValidateSamples(sample);
                    if (error.Count > 0)
                    {
                        errors.Add(sample, error);
                    }
                }

                // Of course if we have an error, we just want to display and alert the user.
                // But we don't let them continue, they must edit their queue and make it appropiate
                // before running.
                if (errors.Count > 0)
                {
                    var displayVm = new SampleValidatorErrorDisplayViewModel(errors);
                    var display = new SampleValidatorErrorDisplayWindow() { DataContext = displayVm };
                    // Apparently required to allow keyboard input in a WPF Window launched from a WinForms app?
                    System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(display);
                    display.ShowDialog();
                    return;
                }

                // Then we also want to check for running blocks on the wrong column.
                var badBlocks = validator.ValidateBlocks(samples);
                if (badBlocks.Count > 0)
                {
                    //TODO: Add a notification.
                    var displayVm = new SampleBadBlockDisplayViewModel(badBlocks);
                    var display = new SampleBadBlockDisplayWindow() { DataContext = displayVm };
                    // Apparently required to allow keyboard input in a WPF Window launched from a WinForms app?
                    System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(display);
                    var result = display.ShowDialog();
                    if (!result.HasValue || !result.Value)
                    {
                        return;
                    }
                }
            }

            // Then for trigger file checks, we want the sample data for DMS to be complete.
            // So here we validate all of the samples and show the user all samples before running.
            // This way they can validate if they need to all of this information.
            var validateSamples = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_VALIDATESAMPLESFORDMS, false) &&
                                  !(string.IsNullOrWhiteSpace(classLCMSSettings.GetParameter(classLCMSSettings.PARAM_DMSTOOL)));
            if (validateSamples)
            {
                var dmsDisplayVm = new SampleDMSValidatorDisplayViewModel(samples);
                var dmsDisplay = new SampleDMSValidatorDisplayWindow() { DataContext = dmsDisplayVm };
                // Apparently required to allow keyboard input in a WPF Window launched from a WinForms app?
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(dmsDisplay);

                var dmsResult = dmsDisplay.ShowDialog();

                // We don't care what the result is..
                if (!dmsResult.HasValue || !dmsResult.Value)
                    return;

                // If samples are not valid...then what?
                if (!dmsDisplayVm.AreSamplesValid)
                {
                    var result =
                        MessageBox.Show(
                            "Some samples do not contain all necessary DMS information.  This will affect automatic uploads.  Do you wish to continue?",
                            "DMS Sample Validation",
                            MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.No)
                        return;
                }
                sampleQueue.UpdateSamples(samples);
            }
            //SynchronizeSystemClock();
            sampleQueue.StartSamples();
            ToggleRunButton(false, true);
        }

        /// <summary>
        /// Caches the queue to the default.
        /// </summary>
        public void SaveQueue()
        {
            try
            {
                sampleQueue.CacheQueue(false);
                classApplicationLogger.LogMessage(0,
                    "Queue saved \"" + classLCMSSettings.GetParameter(classLCMSSettings.PARAM_CACHEFILENAME) + "\".");
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0,
                    "Could not save queue: " + classLCMSSettings.GetParameter(classLCMSSettings.PARAM_CACHEFILENAME) + "  " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Saves the sample queue to another file.
        /// </summary>
        public void SaveQueueAs()
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Save Queue As",
                FileName = lastSavedFileName.Replace(".xml", ".que").Replace(".csv", ".que"),
                Filter = "LCMSNet Queue (*.que)|*.que"
            };

            var result = saveDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                lastSavedFileName = saveDialog.FileName;
                sampleQueue.CacheQueue(lastSavedFileName);
                // TODO: // This is the text that is appended to the application title bar
                TitleBarTextAddition = "Sample Queue - " + saveDialog.FileName;
                classApplicationLogger.LogMessage(0,
                    "Queue saved to \"" + classLCMSSettings.GetParameter(classLCMSSettings.PARAM_CACHEFILENAME) +
                    "\" and is now the default queue.");
            }
        }

        /// <summary>
        /// Exports the queue to LCMS Version XML
        /// </summary>
        public void ExportQueueToXML()
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Export Queue to XML for LCMS VB6",
                FileName = lastSavedFileName.Replace(".que", ".xml").Replace(".csv", ".xml"),
                Filter = "LCMS VB6 XML (*.xml)|*.xml"
            };

            var result = saveDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                lastSavedFileName = saveDialog.FileName;
                ISampleQueueWriter writer = new classQueueExportXML();
                ExportQueue(saveDialog.FileName, writer);
            }
        }

        /// <summary>
        /// Exports queue to CSV.
        /// </summary>
        public void ExportQueueToCsv()
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Export Queue to CSV",
                FileName = lastSavedFileName.Replace(".xml", ".csv").Replace(".que", ".csv"),
                Filter = "Comma Separated (*.csv)|*.csv"
            };

            var result = saveDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                lastSavedFileName = saveDialog.FileName;
                ISampleQueueWriter writer = new classQueueExportCSV();
                ExportQueue(saveDialog.FileName, writer);
            }
        }

        /// <summary>
        /// Exports the sample queue to Xcalibur
        /// </summary>
        public void ExportQueueToXcalibur()
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Export Queue to XCalibur",
                FileName = lastSavedFileName.Replace(".xml", ".csv").Replace(".que", ".csv"),
                Filter = "Comma Separated (*.csv)|*.csv"
            };

            var result = saveDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                lastSavedFileName = saveDialog.FileName;
                ISampleQueueWriter writer = new classQueueExportExcalCSV();
                ExportQueue(saveDialog.FileName, writer);
            }
        }

        #endregion

        public void RestoreUserUIState()
        {
            var timer = new System.Threading.Timer(FixScrollPosition, this, 50, System.Threading.Timeout.Infinite);
        }

        private void FixScrollPosition(object obj)
        {
            SampleControlViewModel.RestoreUserUIState();
        }

        public ReactiveCommand UndoCommand { get; private set; }
        public ReactiveCommand RedoCommand { get; private set; }
        public ReactiveCommand RunQueueCommand { get; private set; }
        public ReactiveCommand StopQueueCommand { get; private set; }

        private void SetupCommands()
        {
            UndoCommand = ReactiveCommand.Create(() =>
            {
                using (SampleControlViewModel.Samples.SuppressChangeNotifications())
                {
                    this.sampleQueue.Undo();
                }
            }, this.WhenAnyValue(x => x.SampleDataManager.CanUndo));
            RedoCommand = ReactiveCommand.Create(() =>
            {
                using (SampleControlViewModel.Samples.SuppressChangeNotifications())
                {
                    this.sampleQueue.Redo();
                }
            }, this.WhenAnyValue(x => x.SampleDataManager.CanRedo));
            RunQueueCommand = ReactiveCommand.Create(() => this.RunQueue(), this.WhenAnyValue(x => x.IsRunButtonEnabled));
            StopQueueCommand = ReactiveCommand.Create(() => this.StopQueue(), this.WhenAnyValue(x => x.IsStopButtonEnabled));
        }
    }
}
