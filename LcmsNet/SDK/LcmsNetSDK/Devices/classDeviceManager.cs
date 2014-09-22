﻿/*********************************************************************************************************
 * Written by Brian LaMarche, Dave Clark, John Ryan for the US Department of Energy 
 * Pacific Northwest National Laboratory, Richland, WA
 * Copyright 2009, Battelle Memorial Institute
 * Created 06/19/2009
 * 
 *  Last modified 06/19/2009
 *      Created class and added static methods with static device manager object that registers itself with 
 *      the static method property.
 *      
 *  12-12-2009: BLL
 *      Added a method, FindDevice, to search for a device given its name (key) and it's type (found via GetType()).
 *  12-10-2009: BLL
 *      Created plug-ins, and loading of a new configuration pattern.
 * 
/*********************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using LcmsNetDataClasses.Logging;
using LcmsNetDataClasses.Devices.Pumps;
using LcmsNetSDK.Data;
using LcmsNetDataClasses;

namespace LcmsNetDataClasses.Devices
{         
    /// <summary>
    /// Device manager class for maintaining a list of all devices used by the application.
    /// </summary>
    public class classDeviceManager : IDeviceManager
    {
        private const string CONST_MOBILE_PHASE_NAME    = "mobilephase-";
        private const string CONST_MOBILE_PHASE_COMMENT = "mobilephase-comment-";

        #region Members
        /// <summary>
        /// A current list of devices the application is using.
        /// </summary>
        private List<IDevice> mlist_devices;
        /// <summary>
        /// Static Device Manager Reference.
        /// </summary>
        private static classDeviceManager mobj_deviceManager = null;
        /// <summary>
        /// Fired when a device is successfully added.
        /// </summary>
        public event DelegateDeviceUpdated DeviceAdded;
        /// <summary>
        /// Fired when a device is successfully removed.
        /// </summary>
        public event DelegateDeviceUpdated DeviceRemoved;
        /// <summary>
        /// Fired when a device has been renamed.
        /// </summary>
        public event DelegateDeviceUpdated DeviceRenamed;
        /// <summary>
        /// Defines where pump methods are to be stored.
        /// </summary>
        public const string CONST_PUMP_METHOD_PATH   = "PumpMethods";
        /// <summary>
        /// Path to the device plug-ins.
        /// </summary>
        public const string CONST_DEVICE_PLUGIN_PATH = "Plugins";
        /// <summary>
        /// Tag for the configuration file.
        /// </summary>
        private const string CONST_DEVICE_NAME_TAG = "DeviceName";
        /// <summary>
        /// Tag for the configuration file.
        /// </summary>
        private const string CONST_DEVICE_TYPE_TAG = "DeviceType";
        /// <summary>
        /// Tag for the configuration file.
        /// </summary>
        private const string CONST_DEVICE_TYPE_PATH = "PluginPath";
        /// <summary>
        /// Fired when status changes for the device manager.
        /// </summary>
        public event EventHandler<classDeviceManagerStatusArgs> InitialzingDevice;
        /// <summary>
        /// Fired when new plugins are loaded.
        /// </summary>
        public event EventHandler PluginsLoaded;
        /// <summary>
        /// Fired when all devices are initialized.
        /// </summary>
        public event EventHandler DevicesInitialized;
        /// <summary>
        /// A list of loaded plugin assemblies.
        /// </summary>
        private Dictionary<string, List<classDevicePluginInformation>> mdict_plugins;
        /// <summary>
        /// Flag to indicate whether plug-ins are already being loaded via a directory operation.
        /// </summary>
        private bool mbool_loadingPlugins;
        /// <summary>
        /// Flag tracking whether the devices are emulated or not.
        /// </summary>
        private bool mbool_emulateDevices;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.  Sets the static device manager object reference to this.
        /// </summary>
        private classDeviceManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            mdict_plugins        = new Dictionary<string, List<classDevicePluginInformation>>();
            AvailablePlugins     = new List<classDevicePluginInformation>();
            mlist_devices        = new List<IDevice>();
            //Manager              = this;
            
            DeviceManagerBridge bridge = new DeviceManagerBridge(this);

            mbool_loadingPlugins = false;
            mbool_emulateDevices = true;
            classLCMSSettings.SettingChanged += OnSettingChanged;
        }

        /// <summary>
        /// Found the assembly that was required.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string [] data      = args.Name.Split(',');
            string basicName    = data[0];
            string assemblyName = basicName + ".dll";
            string testPath     = System.IO.Path.Combine(CONST_DEVICE_PLUGIN_PATH, assemblyName);
            testPath            = System.IO.Path.GetFullPath(testPath);

            if (System.IO.File.Exists(testPath))
            {
                return Assembly.LoadFile(testPath);
            }
            return null;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the list of registered devices.
        /// </summary>
        public List<IDevice> Devices
        {
            get
            {
                return mlist_devices;
            }
        }
        /// <summary>
        /// Gets the list of available plug-ins.
        /// </summary>
        public List<classDevicePluginInformation> AvailablePlugins
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets or sets whether to emulate the devices or not.
        /// </summary>
        public bool Emulate
        {
            get
            {
                return mbool_emulateDevices;
            }
            set
            {
                mbool_emulateDevices = value;
                SetEmulationFlags();
            }
        }
        /// <summary>
        /// Gets the device count.
        /// </summary>
        public int DeviceCount
        {
            get
            {
                return mlist_devices.Count;
            }
        }
        /// <summary>
        /// Gets the number of initialized devices.
        /// </summary>
        public int InitializedDeviceCount
        {
            get
            {
                // Find out how many are initialized.
                int total = 0;
                foreach (IDevice device in mlist_devices)
                {
                    if (device.Status == enumDeviceStatus.Initialized || device.Status == enumDeviceStatus.InUseByMethod)
                    {
                        total++;
                    }
                }

                return total;
            }
        }
        #endregion

        #region Configuration Extraction and Loading 
        /// <summary>
        /// Loads a devcie 
        /// </summary>
        /// <param name="filePath">Path to load configuration from.</param>
        public void LoadPersistentConfiguration(classDeviceConfiguration configuration)
        {
            List<Exception> exceptionsToThrow = new List<Exception>();

            for (int i = 0; i < configuration.DeviceCount; i++)
            {
                string deviceName           = configuration[i];
                Dictionary<string, object> settings = configuration.GetDeviceSettings(deviceName);
                
                // We may have to load the type from another assembly, so look it up.
                string  path    = null;
                if (settings.ContainsKey(CONST_DEVICE_TYPE_PATH))
                {
                    path        = settings[CONST_DEVICE_TYPE_PATH] as string;
                }

                Type type       = null;
                if (Assembly.GetExecutingAssembly().Location != path && path != null && System.IO.File.Exists(path))
                {
                    // This wasn't working...
                    //type = Assembly.LoadFrom(path).GetType(settings[CONST_DEVICE_TYPE_TAG] as string);                    
                    string typeName = settings[CONST_DEVICE_TYPE_TAG] as string;
                    foreach (Type t in Assembly.LoadFrom(path).GetTypes())
                    {
                        if (t.FullName == typeName)  //TODO: BLL if(t.AssemblyQualifiedName == typeName)
                        {
                            type = t;
                            break;
                        }
                    }
                }
                else
                {
                    string testType = settings[CONST_DEVICE_TYPE_TAG] as string;    
                    type = Type.GetType(testType);
                }                
                string  name    = settings[CONST_DEVICE_NAME_TAG] as string;                
                if (!typeof(IDevice).IsAssignableFrom(type))
                {
                    exceptionsToThrow.Add(new InvalidCastException("The specified device type is invalid."));
                    continue;
                }

                // 1. Create an instance of the device.
                // 2. Name the device
                // 3. Extract settings and bind them to the device.

                IDevice device = null;

                try
                {
                    device = Activator.CreateInstance(type) as IDevice;
                    device.Name = CreateUniqueDeviceName(deviceName);
                }
                catch (Exception ex)
                {
                    exceptionsToThrow.Add(ex);
                    continue;
                }
                // Emulation needs to be set on devices before any other properties, otherwise it may not be seen properly on a check
                // for emulation mode at startup/initialization.
                device.Emulation = mbool_emulateDevices;                
                // Get all writeable properties.
                PropertyInfo[] properties = type.GetProperties();

                // Then map them so we dont have to do a N^2 search to set a value.
                Dictionary<string, PropertyInfo> propertyMap = new Dictionary<string, PropertyInfo>();
                foreach (PropertyInfo property in properties)
                {
                    object[] attributes = property.GetCustomAttributes(typeof(classPersistenceAttribute), true);
                    foreach (object o in attributes)
                    {
                        classPersistenceAttribute setting = o as classPersistenceAttribute;
                        if (setting != null)
                        {
                            propertyMap.Add(setting.SettingName, property);
                        }
                    }
                }

                // Then finally load the values into the property.
                foreach (string key in settings.Keys)
                {
                    if (propertyMap.ContainsKey(key) && propertyMap[key].CanWrite)
                    {
                        object data         = settings[key];                     
                        // Enumerations are a special breed.  
                        Type propertyType   = propertyMap[key].PropertyType;
                        if (propertyType.IsEnum)
                        {
                            data = Enum.Parse(propertyType, data.ToString());
                        }
                        else
                        {
                            data = Convert.ChangeType(data, propertyMap[key].PropertyType);
                        }
                        try
                        {
                            propertyMap[key].SetValue(device, data, BindingFlags.SetProperty, null, null, null);
                        }
                        catch (Exception ex)
                        {
                            exceptionsToThrow.Add(ex);
                        }
                    }
                }
               
                // Reconstruct any mobile phase data...
                IPump pump = device as IPump;                
                if (pump != null)
                {
                    Dictionary<int, MobilePhase> phases = new Dictionary<int, MobilePhase>();
                    pump.MobilePhases.Clear();

                    foreach (string key in settings.Keys)
                    {
                        bool isComment  = key.Contains(CONST_MOBILE_PHASE_COMMENT);
                        bool isName     = key.Contains(CONST_MOBILE_PHASE_NAME);                                 
                        int phaseId     = 0;
                        
                        if (isComment && int.TryParse(key.Replace(CONST_MOBILE_PHASE_COMMENT, ""), out phaseId))
                        {
                            if (!phases.ContainsKey(phaseId))
                            {
                                phases.Add(phaseId, new MobilePhase());
                            }
                            MobilePhase phase = phases[phaseId];
                            object value      = settings[key];
                            phase.Comment     = value.ToString();
                        }
                        else if (isName && int.TryParse(key.Replace(CONST_MOBILE_PHASE_NAME, ""), out phaseId))
                        {                            
                            if (!phases.ContainsKey(phaseId))
                            {
                                phases.Add(phaseId, new MobilePhase());
                            }
                            MobilePhase phase = phases[phaseId];
                            object value      = settings[key];
                            phase.Name        = value.ToString();
                        }                        
                    }

                    foreach (MobilePhase phase in phases.Values)
                    {
                        pump.MobilePhases.Add(phase);
                    }
                }

                // Then add the device.
                AddDevice(device);
            }
        }
        /// <summary>
        /// Saves all devices to the configuration 
        /// </summary>
        /// <param name="filePath"></param>
        public void ExtractToPersistConfiguration(ref classDeviceConfiguration configuration)
        {                        
            foreach(IDevice device in Devices)
            {                
                if ((device.DeviceType != enumDeviceType.Component) && (device.DeviceType != enumDeviceType.Fluidics))
            {
                    continue;
                }
                Type deviceType = device.GetType();
                PropertyInfo [] properties = deviceType.GetProperties();

                configuration.AddSetting(device.Name, CONST_DEVICE_TYPE_TAG,  deviceType.FullName); //TODO: BLL - .AssemblyQualifiedName);
                configuration.AddSetting(device.Name, CONST_DEVICE_TYPE_PATH, deviceType.Assembly.Location); // This should be made a relative path since we have a plugin directory. All plugins should be in that directory. Using a relative path means that if the program is installed to a different directory, the plugin will still be found properly while avoiding problems with multiple installs(Such as multiple branches on the same dev machine and swapping hardware configs. 
                configuration.AddSetting(device.Name, CONST_DEVICE_NAME_TAG,  device.Name);

                foreach(PropertyInfo property in properties)
                {
                    
                    if (!property.CanWrite)
                        continue;

                    object [] attributes =  property.GetCustomAttributes(
                                                            typeof(classPersistenceAttribute), 
                                                            true);
                    // Make sure the propety is tagged to be persisted.
                    if (attributes.Length < 1)
                        continue;

                    foreach (object attributeObject in attributes)
                    {
                        classPersistenceAttribute settingAttribute = 
                                    attributeObject as classPersistenceAttribute;

                        if (settingAttribute != null)
                        {
                            object data = property.GetValue(device, BindingFlags.GetProperty,
                                                            null,
                                                            null,
                                                            null);
                            configuration.AddSetting(device.Name, settingAttribute.SettingName, data);
                        }
                    }
                }
            }        
        }
        #endregion

        #region Device Naming, Usage, Add, Delete
        /// <summary>
        /// Searches the device manager for a device with the same name.
        /// </summary>
        /// <param name="deviceName">Name to search the device manager for.</param>
        /// <returns>True if device name is in use.  False if the device name is free</returns>
        public bool DeviceNameExists(string deviceName)
        {
            if (deviceName == null)
                return false;

            foreach (IDevice dev in mlist_devices)
            {
                if (dev.Name == deviceName)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Finds the devices that are named with the supplied name and type.
        /// </summary>
        /// <param name="deviceName">Name of the device of question.</param>
        /// <param name="deviceType">Type of the device of question.</param>
        /// <returns>IDevice object reference if it exists, null if it does not.</returns>
        public IDevice FindDevice(string deviceName, Type deviceType)
        {
            IDevice device = null;

            /// 
            /// Find the device in the list, might be better if we used a 
            /// dictionary instead.
            /// 
            /// Then see if the device type matches as well...
            /// 
            foreach (IDevice dev in mlist_devices)
            {
                string name = dev.Name;
                Type devType = dev.GetType();
                if (dev.Name == deviceName && devType.Equals(deviceType))
                {
                    device = dev;
                    break;
                }
            }
            return device;
        }
        /// <summary>
        /// Finds a device just by name.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public IDevice FindDevice(string deviceName)
        {
            IDevice device = null;

            /// 
            /// Find the device in the list, might be better if we used a 
            /// dictionary instead.
            /// 
            /// Then see if the device type matches as well...
            /// 
            foreach (IDevice dev in mlist_devices)
            {
                if (dev.Name == deviceName)
                {
                    device = dev;
                    break;
                }
            }
            return device;
        }
        /// <summary>
        /// Creates a unique device name from the basename provided.
        /// </summary>
        /// <param name="baseName"></param>
        /// <returns></returns>
        public string CreateUniqueDeviceName(string baseName)
        {
            string newName  = baseName;
            int deviceCount = 0;

            /// We expect that there are not an infinite number of device names
            while (DeviceNameExists(newName) == true)
            {
                newName = baseName + deviceCount.ToString();
                deviceCount++;
            }

            return newName;
        }

        /// <summary>
        /// Renames the device with the given basename after checking to see if that name is reserved.
        /// </summary>
        /// <param name="device">Device to rename</param>
        /// <param name="basename">Name to use for the device.</param>
        public void RenameDevice(IDevice device, string basename)
        {
            string oldName  = device.Name;

            /// 
            /// If this happens, then they are trying to name the device
            /// the same thing....
            /// 
            if (basename == oldName)
                return;

            string newName  = CreateUniqueDeviceName(basename);
            device.Name     = newName;

            if (DeviceRenamed != null)
                DeviceRenamed(this, device);
        }
        /// <summary>
        /// Adds the device to the device manager if the name is not a duplicate and the same object reference does not exist.
        /// </summary>
        /// <returns>True if the device was added.  False if device was not added.</returns>
        public bool AddDevice(IDevice device)
        {
            /// 
            /// No null devices allowed.
            /// 
            if (device == null)
                return false;

            /// 
            /// No duplicate references allowed.
            /// 
            if (mlist_devices.Contains(device) == true)
                return false;

            /// 
            /// No duplicate names allowed.
            /// 
            if (DeviceNameExists(device.Name))
                return false;

            device.Emulation = mbool_emulateDevices;

            mlist_devices.Add(device);

            if (DeviceAdded != null)
                DeviceAdded(this, device);

            return true;
        }
        /// <summary>
        /// Creates a new device based on the plug-in information.
        /// </summary>
        /// <param name="plugin">Device plug-in used to create a new device.</param>        
        /// <param name="initialize">Indicates whether to initialize the device if added succesfully </param>
        /// <returns>True if successful, False if it fails.</returns>
        public bool AddDevice(classDevicePluginInformation plugin, bool initialize)
        {
            IDevice device  = Activator.CreateInstance(plugin.DeviceType) as IDevice;
            device.Name     = CreateUniqueDeviceName(device.Name);
            bool added      = AddDevice(device);

            if (added && initialize)
            {
                InitializeDevice(device);                
            }
            return added;
        }
        /// <summary>
        /// Removes the device from the device manager.
        /// </summary>
        /// <param name="device">Device to remove.</param>
        /// <returns>True if device was removed successfully.  False if the device could not be removed at that time.</returns>
        public bool RemoveDevice(IDevice device)
        {
            /// 
            /// Make sure we have the reference
            /// 
            if (mlist_devices.Contains(device) == false)
                return false;

            device.Shutdown();

            mlist_devices.Remove(device);

            if (DeviceRemoved != null)
                DeviceRemoved(this, device);

            return true;
        }
        /// <summary>
        /// Updates devices with emulated flags 
        /// </summary>
        private void SetEmulationFlags()
        {
            foreach (IDevice device in mlist_devices)
            {
                device.Emulation = mbool_emulateDevices;
            }
        }
        #endregion

        #region Shutdown and Initialization
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ShutdownDevices()
        {
            return ShutdownDevices(false);
        }
        /// <summary>
        /// Calls the shutdown method for each device.
        /// </summary>
        /// <returns>True if shutdown successful.  False if shutdown failed.</returns>
        public bool ShutdownDevices(bool clearDevices)
        {
            bool worked = true;
            foreach (IDevice device in mlist_devices)
            {
                worked = (worked && device.Shutdown());
            }

            if (clearDevices)
            {                
                    List<IDevice> tempDevices = new List<IDevice>();
                    tempDevices.AddRange(mlist_devices);
                    foreach (IDevice device in tempDevices)
                    {
                        try
                        {
                            RemoveDevice(device);
                        }
                        catch
                        {
                            //TODO: CRAP!  what happens if the device cannot be removed. 
                        }
                    }
            }

            return worked;
        }
        /// <summary>
        /// Initializes the device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool InitializeDevice(IDevice device)
        {
            if (InitialzingDevice != null)
                InitialzingDevice(this, new classDeviceManagerStatusArgs("Initializing " + device.Name));
            bool initialized  = false;
            try
            {
                string errorMessage = "";
                initialized = device.Initialize(ref errorMessage);
                if (initialized == false)
                {
                    device.Status = enumDeviceStatus.Error;
                    // We wrap error details in the event args class so that it can also be used via a delegate
                    // And then shove it into an exception class.  this way we force the 
                    // caller to handle any exception, but allow them to propogate to any observers.
                    classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(
                                                                        errorMessage,
                                                                        null,
                                                                        enumDeviceErrorStatus.ErrorAffectsAllColumns,
                                                                        device);
                    throw new classDeviceInitializationException(args);
                }
                else
                {
                    device.Status = enumDeviceStatus.Initialized;
                }
            }
            catch (classDeviceInitializationException ex)
            {
                device.Status = enumDeviceStatus.Error;
                throw ex;
            }
            catch (Exception ex)
            {
                device.Status = enumDeviceStatus.Error;
                classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Error intializing device.",
                                                                            ex,
                                                                            enumDeviceErrorStatus.ErrorAffectsAllColumns,
                                                                            device);
                throw new classDeviceInitializationException(args);
            }

            return true;
        }
        /// <summary>
        /// Initializes all the devices if they have not been initialized already.
        /// </summary>
        /// <returns></returns>
        public List<classDeviceErrorEventArgs> InitializeDevices(bool reinitializeAlreadyInitialized)
        {
            List<classDeviceErrorEventArgs> devices = new List<classDeviceErrorEventArgs>();
            foreach (IDevice device in mlist_devices)
            {
                try
                {
                    bool alreadyInitialized = (device.Status == enumDeviceStatus.Initialized || device.Status == enumDeviceStatus.Initialized);
                    if (!reinitializeAlreadyInitialized && !alreadyInitialized)
                    {
                        InitializeDevice(device);
                    }
                    else
                    {
                        InitializeDevice(device);
                    }
                }
                catch (classDeviceInitializationException ex)
                {
                    LcmsNetDataClasses.Logging.classApplicationLogger.LogError(0, string.Format("{0} could not be initialized.  {1}",
                        device.Name, 
                        ex.ErrorDetails.Error), ex.ErrorDetails.Exception);
                    devices.Add(ex.ErrorDetails);
                }
            }

            if (DevicesInitialized != null)
            {
                DevicesInitialized(this, new EventArgs());
            }
            return devices;
        }
        /// <summary>
        /// Initializes all the devices if they have not been initialized already.
        /// </summary>
        /// <returns></returns>
        public List<classDeviceErrorEventArgs> InitializeDevices()
        {
            return InitializeDevices(true);
        }
        #endregion

        #region Plug-in Management
        /// <summary>
        /// Loads the satellite assemblies required for type checking.
        /// </summary>
        /// <param name="path"></param>
        public void LoadSatelliteAssemblies(string path)
        {
            try
            {
                string [] files = System.IO.Directory.GetFiles(path, "*.dll");
                foreach(string file in files)
                {
                    try
                    {
                        string fullPath = System.IO.Path.GetFullPath(file);
                        Assembly ass = Assembly.LoadFile(fullPath);
                    }
                    catch (BadImageFormatException)
                    {
                        classApplicationLogger.LogMessage(0, string.Format("The dll {0} is not a .net assembly.  Skipping.", path));
                    }
                    catch (Exception ex)
                    {
                        classApplicationLogger.LogError(0, "Could not load satellite assemblies", ex);
                    }               
                }
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, "Could not load satellite assemblies", ex);
            }
        }
        /// <summary>
        /// Loads supported device plugin types.
        /// </summary>
        /// <param name="assembly">Assembly to load device types from.</param>
        /// <param name="forceReload">Flag indicating whether to force a re-load the assemblies if they have already been loaded.</param>
        public void LoadPlugins(Assembly assembly, bool forceReload)
        {
            string assemblyPath = assembly.Location;
            if (mdict_plugins.ContainsKey(assemblyPath))
            {
                if (!forceReload)
                {
                    throw new Exception("The plug-in assembly has already been loaded.");
                }
                else
                {
                    // Remove the old plug-ins from the list.
                    foreach (classDevicePluginInformation plugin in mdict_plugins[assemblyPath])
                    {
                        AvailablePlugins.Remove(plugin);
                    }
                    // Remove the old plug-in link in the plug-in dictionary.
                    mdict_plugins.Remove(assemblyPath);
                }
            }

            // Map the assembly path to a list of available plug-ins and also update the list of available plug-ins
            List<classDevicePluginInformation> supportedPlugins = RetrieveSupportedDevicePluginTypes(assembly);
            AvailablePlugins.AddRange(supportedPlugins);
            mdict_plugins.Add(assemblyPath, supportedPlugins);
        }
        /// <summary>
        /// Retrieves plug-ins from the assembly at the path provided.
        /// </summary>
        /// <param name="assemblyPath">File path to assembly.</param>
        /// <param name="forceReload">Flag indicating whether to force a re-load the assemblies if they have already been loaded.</param>
        public void LoadPlugins(string assemblyPath, bool forceReload)
        {   if (mdict_plugins.ContainsKey(assemblyPath))
                     
            {
                if (!forceReload)
                {
                    throw new Exception("The plug-in assembly has already been loaded.");
                }
                else
                {
                    // Remove the old plug-ins from the list.
                    foreach (classDevicePluginInformation plugin in mdict_plugins[assemblyPath])
                    {
                        AvailablePlugins.Remove(plugin);
                    }
                    // Remove the old plug-in link in the plug-in dictionary.
                    mdict_plugins.Remove(assemblyPath);
                }
            }

            // Map the assembly path to a list of available plug-ins and also update the list of available plug-ins
            List<classDevicePluginInformation> supportedPlugins = RetrieveSupportedDevicePluginTypes(System.IO.Path.GetFullPath(assemblyPath));
            AvailablePlugins.AddRange(supportedPlugins);
            mdict_plugins.Add(assemblyPath, supportedPlugins);

            if (PluginsLoaded != null && !mbool_loadingPlugins)
            {
                PluginsLoaded(this, null);
            }
        }
        /// <summary>
        /// Loads supported device plugin types from a directory.
        /// </summary>
        /// <param name="directoryPath">Directory of assemblies to load.</param>
        /// <param name="filter">Assembly file filter.</param>
        /// <param name="forceReload">Flag indicating whether to force a re-load the assemblies if they have already been loaded.</param>
        public void LoadPlugins(string directoryPath, string filter, bool forceReload)
        {
            // Signal others we are the ones doing the loading and alerting.
            mbool_loadingPlugins = true;

            string[] files = System.IO.Directory.GetFiles(directoryPath, filter);
            foreach(string assemblyPath in files)
            {
                LoadPlugins(assemblyPath, forceReload);
            }
            mbool_loadingPlugins = false;

            if (PluginsLoaded != null)
            {
                PluginsLoaded(this, null);
            }
        }
        /// <summary>
        /// Loads supported device plugin types.
        /// </summary>
        /// <param name="assembly">Assembly to load device types from.</param>
        /// <returns>All types that support IDevice and have been attributed with a glyph and device control attribute.</returns>
        private List<classDevicePluginInformation> RetrieveSupportedDevicePluginTypes(Assembly assembly)
        {
            List<classDevicePluginInformation> supportedTypes = new List<classDevicePluginInformation>();

            Type[] types = assembly.GetExportedTypes();
            foreach (Type objectType in types)
            {
                // Map the controls
                if (typeof(IDevice).IsAssignableFrom(objectType))
                {
                    object[] attributes = objectType.GetCustomAttributes(typeof(classDeviceControlAttribute), true);
                    foreach (object attribute in attributes)
                    {
                        classDeviceControlAttribute control = attribute as classDeviceControlAttribute;
                        if (control != null)
                        {
                            //TODO: Brian changed this...kind of chris made him do it...but we are going to revisit all of the bad things that could happen
                            // if we left this thing uncommented...basically trying to transition from this crap anyway...
                            //if (control.GlyphType != null && control.ControlType != null)
                            {
                                classDevicePluginInformation pluginInfo = new classDevicePluginInformation(objectType, control);
                                supportedTypes.Add(pluginInfo);
                            }
                        }
                    }
                }
            }

            return supportedTypes;
        }
        /// <summary>
        /// Retrieves plug-ins from the assembly at the path provided.
        /// </summary>
        /// <param name="assemblyPath">File path to assembly.</param>
        /// <returns>All types that support IDevice and have been attributed with a glyph and device control attribute.</returns>
        private List<classDevicePluginInformation> RetrieveSupportedDevicePluginTypes(string assemblyPath)
        {
            List<classDevicePluginInformation> supportedTypes = new List<classDevicePluginInformation>();
            try
            {
                Assembly fileAssembly = Assembly.LoadFile(assemblyPath);                
                List<classDevicePluginInformation> subTypes = RetrieveSupportedDevicePluginTypes(fileAssembly);
                supportedTypes.AddRange(subTypes);
            }
            catch(Exception ex)
            {
                classApplicationLogger.LogError(0, "Could not load the plugin from dll: " + assemblyPath, ex);
                //TODO: throw exception ! let people know this failed.
            }
            return supportedTypes;
        }
        #endregion

        /*#region System Health
        /// <summary>
        /// Writes the system health to the provider.
        /// </summary>
        public void WriteSystemHealth(string path)
        {            
            // Retrieve the data from the aggregates
            List<FinchAggregateData> aggregates = new List<FinchAggregateData>();
            FinchAggregateData aggregate = new FinchAggregateData();
            aggregate.Components         = new List<FinchComponentData>();
            aggregate.Error              = null;
            aggregate.LastUpdate         = DateTime.Now;
            aggregate.Status             = "";
            aggregate.Name               = LcmsNetDataClasses.classLCMSSettings.GetParameter("CartName");

            foreach (IDevice device in mlist_devices)
            {
                FinchComponentData data = device.GetData();
                if (data != null)
                {
                    aggregate.Components.Add(data);
                }
            }

            // Then write the aggregate data to file.
            aggregates.Add(aggregate);
            try
            {
                //writer.WriteAggregates(aggregates, path);
                FinchRestHttpClass http = new FinchRestHttpClass();
                http.URL                = Properties.Settings.Default.FinchServerURL;
                http.WriteAggregates(aggregates, path);
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, "Could not create system health message data.", ex);
            }
        }
        #endregion*/

        #region Static Property
        /// <summary>
        /// Gets or sets the static device manager reference.
        /// </summary>
        public static classDeviceManager Manager
        {
            get
            {
                if (mobj_deviceManager == null)
                    mobj_deviceManager = new classDeviceManager();

                return mobj_deviceManager;
            }
            set
            {
            }
        }
        #endregion
    
        public void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            if(e.SettingName.Equals("EmulationEnabled"))
            {
               Emulate = Convert.ToBoolean(e.SettingValue);        
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class classDeviceInitializationException : Exception
    {
        public classDeviceErrorEventArgs ErrorDetails { get; private set; }

        public classDeviceInitializationException(classDeviceErrorEventArgs errorArgs)            
        {
            ErrorDetails = errorArgs;
        }
    }
}