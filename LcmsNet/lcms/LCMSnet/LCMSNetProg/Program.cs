﻿using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Generic;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Configuration;
using LcmsNetDataClasses.Devices;
using LcmsNetSDK;
using LcmsNetSQLiteTools;
using LcmsNet.Method;
using LcmsNetDataClasses.Logging;

namespace LcmsNet
{
    /// <summary>
    /// LCMS.net Program that controls LC-Cart hardware components.
    /// </summary>
    static class Program
    {
        #region "Constants"

        public const string SOFTWARE_COPYRIGHT = "Copyright Battelle Memorial Institute, 2017";

        public const string SOFTWARE_DEVELOPERS =
            "Brian LaMarche, Christopher Walters, " +
            "Gordon Anderson, David Clark, Derek Hopkins, " +
            "Matthew Monroe, Ron Moore, Danny Orton, John Ryan, Richard Smith";


        #endregion

        #region "Class variables"

        /// <summary>
        /// Reference to splash screen window.
        /// </summary>
        private static formSplashScreen m_splashScreen;

        #endregion

        #region Configuration Loading

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <returns>An object that holds the application settings.</returns>
        static void LoadSettings()
        {
            var propColl = Properties.Settings.Default.Properties;
            foreach (SettingsProperty currProperty in propColl)
            {
                var propertyName = currProperty.Name;
                var propertyValue = Properties.Settings.Default[propertyName].ToString();
                classLCMSSettings.SetParameter(propertyName, propertyValue);
            }

            // Add path to executable as a saved setting
            var fi = new FileInfo(Application.ExecutablePath);
            classLCMSSettings.SetParameter(classLCMSSettings.PARAM_APPLICATIONPATH, fi.DirectoryName);


            var emulation = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_EMULATIONENABLED);
            if (!string.IsNullOrWhiteSpace(emulation))
            {
                var isEmulated = Convert.ToBoolean(emulation);
                m_splashScreen.SetEmulatedLabelVisibility(classLCMSSettings.GetParameter(classLCMSSettings.PARAM_CARTNAME), isEmulated);
            }
            return;
        }

        #endregion

        public static void LogVersionNumbers()
        {
            var information = SystemInformationReporter.BuildApplicationInformation();
            classApplicationLogger.LogMessage(0, information);
        }

        public static void LogMachineInformation()
        {
            var systemInformation = SystemInformationReporter.BuildSystemInformation();
            classApplicationLogger.LogMessage(0, systemInformation);
        }

        private static void classLCMSSettings_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            Properties.Settings.Default[e.SettingName] = e.SettingValue;
            Properties.Settings.Default.Save();
        }

        #region "Constants"

        /// <summary>
        /// Default logging levels
        /// </summary>
        private const int CONST_DEFAULT_ERROR_LOG_LEVEL = 5;

        private const int CONST_DEFAULT_MESSAGE_LOG_LEVEL = 5;

        #endregion

        #region Methods

        /// <summary>
        /// Kills any hanging PAL exe processes.
        /// </summary>
        static void KillExistingPalProcesses()
        {
            try
            {
                var processes = Process.GetProcessesByName("paldriv");
                foreach (var process in processes)
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not kill the PAL processes. " + ex.Message);
            }
        }

        /// <summary>
        /// Creates the path required for local operation.
        /// </summary>
        /// <param name="path">Local path to create.</param>
        static void CreatePath(string localPath)
        {
            var path = Path.Combine(Application.StartupPath, localPath);
            //
            // See if the logging directory exists
            //
            if (Directory.Exists(path) == false)
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (UnauthorizedAccessException ex)
                {
                    //
                    // Not much we can do here...
                    //
                    var errorMessage =
                        string.Format(
                            "LCMS could not create missing folder {0} required for operation.  Please run application with higher priveleges.  {1}",
                            localPath, ex.Message);
                    MessageBox.Show(errorMessage);
                    Application.Exit();
                }
            }
        }

        /// <summary>
        /// Updates the splash screen with the appropiate messages.
        /// </summary>
        /// <param name="messageLevel">Filter for displaying messages.</param>
        /// <param name="args">Messages and other arguments passed from the sender.</param>
        static void classApplicationLogger_Message(int messageLevel, classMessageLoggerArgs args)
        {
            if (messageLevel < 1)
            {
                m_splashScreen.Status = args.Message;
            }
        }

        /// <summary>
        /// Creates the configurations for the columns and systems
        /// </summary>
        static void InitializeSystemConfigurations()
        {
            //
            // Create system data with columns.
            //
            var columnOne = new classColumnData
            {
                ID = 0,
                Name = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_COLUMNNAME0),
                Status = enumColumnStatus.Idle,
                Color = Color.Tomato,
                First = true
            };

            var columnTwo = new classColumnData
            {
                ID = 1,
                Name = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_COLUMNNAME1),
                Status = enumColumnStatus.Idle,
                Color = Color.Lime
            };

            var columnThree = new classColumnData
            {
                ID = 2,
                Name = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_COLUMNNAME2),
                Status = enumColumnStatus.Idle,
                Color = Color.LightSteelBlue
            };

            var columnFour = new classColumnData
            {
                ID = 3,
                Name = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_COLUMNNAME3),
                Status = enumColumnStatus.Idle,
                Color = Color.LightSalmon
            };

            classCartConfiguration.Columns = new List<classColumnData> {
                columnOne, columnTwo, columnThree, columnFour};
        }

        #endregion

        #region Application entry point

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var mutexName = "Global\\" + Assembly.GetExecutingAssembly().GetName().Name;

                //
                // Before we do anything, let's initialize the file logging capability.
                //
                classApplicationLogger.Error +=
                    classFileLogging.LogError;
                classApplicationLogger.Message +=
                    classFileLogging.LogMessage;

                //
                // Now lets initialize logging to SQLite database.
                //
                classApplicationLogger.Error += classDbLogger.LogError;
                classApplicationLogger.Message +=
                    classDbLogger.LogMessage;


                // Note that we used icons from here for the gears on the main form window.
                //     http://labs.chemist2dio.com/free-vector-gears.php/
                //
                //
                // Use a mutex to ensure a single copy of program is running. If we can create a new mutex then
                //      no instance of the application is running. Otherwise, we exit.
                // Code adapated from K. Scott Allen's OdeToCode.com at
                //      http://odetocode.com/Blogs/scott/archive/2004/08/20/401.aspx
                //
                using (var mutex = new Mutex(false, mutexName))
                {
                    if (!mutex.WaitOne(0, false))
                    {
                        MessageBox.Show("A copy of LcmsNet is already running.");
                        classApplicationLogger.LogError(0, "A copy of LcmsNet is already running.", null, null);
                        return;
                    }

                    KillExistingPalProcesses();
                    GC.KeepAlive(mutex);

                    //
                    // Synch to the logger so we can display any messages coming back from the
                    // rest of the program and interface.
                    //
                    classApplicationLogger.Message +=
                        classApplicationLogger_Message;

                    //
                    // Show the splash screen
                    //
                    m_splashScreen = new formSplashScreen
                    {
                        SoftwareCopyright = SOFTWARE_COPYRIGHT,
                        SoftwareDevelopers = SOFTWARE_DEVELOPERS
                    };

                    var splashLoadTime = DateTime.UtcNow;

                    m_splashScreen.Show();
                    Application.DoEvents();

                    LogVersionNumbers();
                    LogMachineInformation();
                    classApplicationLogger.LogMessage(0, string.Format("[Log]"));

                    //
                    // Display the splash screen mesasge first! make the log folder,
                    // after that we will route messages through the logger instead of
                    // through this static call.  That way we know the log folder has been
                    // created.
                    //
                    //classApplicationLogger.LogMessage(-1, "Creating pertinent folders");
                    Application.DoEvents();

                    //
                    // Make sure we can log/error report locally before we do anything!
                    //
                    try
                    {
                        CreatePath(classLCMethodFactory.CONST_LC_METHOD_FOLDER);
                        CreatePath(classDeviceManager.CONST_PUMP_METHOD_PATH);
                        CreatePath(classDeviceManager.CONST_DEVICE_PLUGIN_PATH);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        classApplicationLogger.LogError(0, "Could not create necessary directories.", ex);
                        MessageBox.Show(
                            "The application does not have the required privileges to run.  Please run as administrator.");
                        return;
                    }
                    //
                    // Load settings
                    //
                    classApplicationLogger.LogMessage(-1, "Loading settings");
                    Application.DoEvents();
                    LoadSettings();
                    
                    // Now that settings have been loaded, set the event handler so that when any of these settings change, we save them to disk.
                    classLCMSSettings.SettingChanged += classLCMSSettings_SettingChanged;
                    
                    //classApplicationLogger.LogMessage(-1, "Creating Initial System Configurations");
                    InitializeSystemConfigurations();

                    //
                    // Create a device manager.
                    //
                    //classApplicationLogger.LogMessage(-1, "Creating the Device Manager");
                    Application.DoEvents();
                    var deviceManager = classDeviceManager.Manager;
                    deviceManager.Emulate = Convert.ToBoolean(classLCMSSettings.GetParameter(classLCMSSettings.PARAM_EMULATIONENABLED));
                    deviceManager.AddDevice(new classTimerDevice());
                    deviceManager.AddDevice(new Devices.classBlockDevice());
                    deviceManager.AddDevice(new Devices.classLogDevice());
                    deviceManager.AddDevice(new Devices.classApplicationDevice());

                    //
                    // Load the device plug-ins.
                    //
                    classApplicationLogger.LogMessage(-1, "Loading necessary device plug-ins.");
                    var areDevicesLoaded = true;
                    try
                    {
                        classDeviceManager.Manager.LoadPlugins(Assembly.GetExecutingAssembly(), true);
                    }
                    catch
                    {
                        areDevicesLoaded = false;
                        classApplicationLogger.LogError(0, "Could not load internal device plug-ins.");
                    }
                    classApplicationLogger.LogMessage(-1, "Loading external device plug-ins.");
                    try
                    {
                        classDeviceManager.Manager.LoadSatelliteAssemblies(classDeviceManager.CONST_DEVICE_PLUGIN_PATH);
                        classDeviceManager.Manager.LoadPlugins(classDeviceManager.CONST_DEVICE_PLUGIN_PATH, "*.dll",
                            true);
                    }
                    catch
                    {
                        areDevicesLoaded = false;
                        classApplicationLogger.LogError(0, "Could not load external device plug-ins.");
                    }
                    //TODO: Whoops! not all of the device plug-in loading should kill this effort.  If one set loaded,
                    // that may be ok.
                    if (!areDevicesLoaded)
                    {
                        classApplicationLogger.LogError(-1, "Failed to load some of the device plug-ins.");
                        return;
                    }
                    classApplicationLogger.LogMessage(-1, "Device plug-ins are loaded.");
                    Application.DoEvents();

#if DEBUG
                    //
                    // Create a device we can use for testing errors with.
                    //
                    IDevice dev = new Devices.classErrorDevice();
                    deviceManager.AddDevice(dev);
#endif

                    //
                    // Create the method manager
                    //
                    //classApplicationLogger.LogMessage(-1, "Creating the Method Manager");
                    Application.DoEvents();


                    //
                    // Set the logging levels
                    //
                    var logLevelErrors = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_LOGGINGERRORLEVEL);
                    if (!string.IsNullOrWhiteSpace(logLevelErrors))
                        classApplicationLogger.ErrorLevel = int.Parse(logLevelErrors);
                    else 
                        classApplicationLogger.ErrorLevel = CONST_DEFAULT_ERROR_LOG_LEVEL;

                    var logLevelMessages = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_LOGGINGMSGLEVEL);
                    if (!string.IsNullOrWhiteSpace(logLevelMessages))
                        classApplicationLogger.MessageLevel = int.Parse(logLevelMessages);
                    else 
                        classApplicationLogger.MessageLevel = CONST_DEFAULT_MESSAGE_LOG_LEVEL;
                    
                    CreateSQLCache();
                    classApplicationLogger.LogMessage(-1, "Loading DMS data");
                    Application.DoEvents();

                    try
                    {
                        // This step loads DMS tools from folder C:\Users\<Username>\AppData\Roaming\LCMSNet\dmsExtensions
                        // The folder will typically have file LcmsNetDmsTools.dll
                        classDMSToolsManager.Instance.SelectedTool.LoadCacheFromDMS(false);
                    }
                    catch (Exception ex)
                    {
                        var extensionsFolder =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LCMSNet",
                                "dmsExtensions");
                        classApplicationLogger.LogError(classApplicationLogger.CONST_STATUS_LEVEL_CRITICAL,
                            @"Unable to load cache from DMS.  If inside PNNL, assure folder " + extensionsFolder +
                            " exists and has the required DLLs (including LcmsNetDmsTools.dll): " + ex.Message);
                    }

                    //
                    // Check to see if any trigger files need to be copied to the transfer server, and copy if necessary
                    //
                    if (bool.Parse(classLCMSSettings.GetParameter(classLCMSSettings.PARAM_COPYTRIGGERFILES)))
                    {
                        if (LcmsNetDataClasses.Data.classTriggerFileTools.CheckLocalTriggerFiles())
                        {
                            classApplicationLogger.LogMessage(-1, "Copying trigger files to DMS");
                            LcmsNetDataClasses.Data.classTriggerFileTools.MoveLocalTriggerFiles();
                        }
                    }

                    //
                    // Check to see if any method folders need to be copied to the transfer server, and copy if necessary
                    //
                    if (bool.Parse(classLCMSSettings.GetParameter(classLCMSSettings.PARAM_COPYMETHODFOLDERS)))
                    {
                        if (SampleQueue.IO.classMethodFileTools.CheckLocalMethodFolders())
                        {
                            classApplicationLogger.LogMessage(-1, "Copying method folders to DMS");
                            SampleQueue.IO.classMethodFileTools.MoveLocalMethodFiles();
                        }
                    }

                    //
                    // Load the main application and run
                    //
                    classApplicationLogger.LogMessage(-1, "Loading main form");
                    Application.DoEvents();
                    var main = new formMDImain();

                    // Assure that the splash screen has been visible for at least 3 seconds
                    while (DateTime.UtcNow.Subtract(splashLoadTime).TotalMilliseconds < 3000)
                    {
                        Thread.Sleep(250);
                    }

                    m_splashScreen.Hide();

                    classApplicationLogger.Message -= classApplicationLogger_Message;

                    try
                    {
                        Application.Run(main);
                    }
                    catch (Exception ex)
                    {
                        classApplicationLogger.LogError(classApplicationLogger.CONST_STATUS_LEVEL_CRITICAL,
                            "Program Failed!", ex);
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                //classApplicationLogger.LogError(classApplicationLogger.CONST_STATUS_LEVEL_CRITICAL, ex.Message);
                classApplicationLogger.LogError(classApplicationLogger.CONST_STATUS_LEVEL_CRITICAL,
                    "Shutting down due to unhandled error: " + ex.Message + "; " + ex.StackTrace);

                MessageBox.Show("Shutting down due to unhandled error: " + ex.Message +
                                " \nFor additional information, see the log files at " +
                                classDbLogger.LogFolderPath + "\\");
            }
            finally
            {
                //
                // Just to make sure...let's kill the PAL at the end of the program as well.
                //
                KillExistingPalProcesses();
                classFileLogging.LogMessage(0,
                    new classMessageLoggerArgs("-----------------shutdown complete----------------------"));
            }
        }

        /// <summary>
        /// Create the SQL cache file, if necessary (SQLiteTools does all the heavy lifting.)
        /// </summary>
        private static void CreateSQLCache()
        {
            classSQLiteTools.SaveSingleColumnListToCache(new List<string>() {"default"}, enumTableTypes.CartList);
            classSQLiteTools.SaveSingleColumnListToCache(new List<string>() {"default"},
                enumTableTypes.SeparationTypeList);
            classSQLiteTools.SaveSingleColumnListToCache(new List<string>() {"default"}, enumTableTypes.DatasetTypeList);
            classSQLiteTools.SaveInstListToCache(new List<classInstrumentInfo>());
            classSQLiteTools.SaveUserListToCache(new List<classUserInfo>());
            classSQLiteTools.SaveSingleColumnListToCache(new List<string>() {"0", "1", "2", "3", "4"},
                enumTableTypes.ColumnList);
            classSQLiteTools.SaveExperimentListToCache(new List<classExperimentData>() {new classExperimentData()});
            classSQLiteTools.SaveProposalUsers(new List<LcmsNetDataClasses.Data.classProposalUser>(),
                new List<LcmsNetDataClasses.Data.classUserIDPIDCrossReferenceEntry>(),
                new Dictionary<string, List<LcmsNetDataClasses.Data.classUserIDPIDCrossReferenceEntry>>());
        }

        #endregion
    }
}