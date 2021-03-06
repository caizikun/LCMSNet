﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluidicsSDK;
using LcmsNet.Devices.ViewModels;
using LcmsNet.Devices.Views;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Logging;
using ReactiveUI;
using Microsoft.Win32;

namespace LcmsNet.Devices.Fluidics.ViewModels
{
    public class FluidicsDesignViewModel : ReactiveObject, IDisposable
    {
        public FluidicsDesignViewModel()
        {
            classDeviceManager.Manager.DeviceAdded += Manager_DeviceAdded;
            classDeviceManager.Manager.DeviceRemoved += Manager_DeviceRemoved;
            classDeviceManager.Manager.DeviceRenamed += Manager_DeviceRenamed;
            fluidicsMod = FluidicsModeratorWpf.Moderator;
            FluidicsControlVm = new FluidicsControlViewModel();
            RefreshEvent += FluidicsControlVm.RefreshHandler;
            Reporter = new ModelCheckReportsViewModel(fluidicsMod);
            AdvancedDeviceControlPanel = new AdvancedDeviceControlPanelViewModel();

            fluidicsMod.ScaleWorldView(1);
            fluidicsMod.ModelChanged += FluidicsModelChanged;

            SetupCommands();
            this.WhenAnyValue(x => x.DesignTabSelected).Where(x => x).Subscribe(x => fluidicsControlVm.UpdateImage());
        }

        private void ReleaseUnmanagedResources()
        {
            classDeviceManager.Manager.ShutdownDevices();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~FluidicsDesignViewModel()
        {
            ReleaseUnmanagedResources();
        }

        #region Members

        // fluidics moderator, controls access to fluidics managers and provides interface for working with them
        private readonly FluidicsModeratorWpf fluidicsMod;

        /// <summary>
        /// Filter for file dialogs.
        /// </summary>
        private const string CONST_HARDWARE_CONFIG_FILTER = "Config files (*.ini)|*.ini|All files (*.*)|*.*";

        /// <summary>
        /// Default configuration for hardware.
        /// </summary>
        private const string CONST_DEFAULT_CONFIG_FILEPATH = "HardwareConfig.ini";

        private bool devicesLocked;
        private bool designTabSelected = true;
        private FluidicsControlViewModel fluidicsControlVm;
        private ModelCheckReportsViewModel reporterVm;
        private AdvancedDeviceControlPanelViewModel advancedDeviceControlPanel;

        #endregion

        #region Properties

        public event EventHandler RefreshEvent;

        public bool DevicesLocked
        {
            get { return devicesLocked; }
            set
            {
                var oldValue = devicesLocked;
                this.RaiseAndSetIfChanged(ref devicesLocked, value);
                if (oldValue != devicesLocked)
                {
                    this.RaisePropertyChanged(nameof(DevicesUnlocked));
                    FluidicsControlVm.DevicesLocked = devicesLocked;
                    if (devicesLocked)
                    {
                        // Locked, we want the unlock button to be pressable to unlock.
                        classApplicationLogger.LogMessage(classApplicationLogger.CONST_STATUS_LEVEL_USER, "The designer has been locked.");
                    }
                    else
                    {
                        // Unlocked, we want the lock button to be lockable.
                        classApplicationLogger.LogMessage(classApplicationLogger.CONST_STATUS_LEVEL_USER, "The designer has been un-locked.");
                    }
                }
            }
        }

        public bool DevicesUnlocked { get { return !devicesLocked; } }

        public bool DesignTabSelected
        {
            get { return designTabSelected; }
            set { this.RaiseAndSetIfChanged(ref designTabSelected, value); }
        }

        public FluidicsControlViewModel FluidicsControlVm
        {
            get { return fluidicsControlVm; }
            private set { this.RaiseAndSetIfChanged(ref fluidicsControlVm, value); }
        }

        public ModelCheckReportsViewModel Reporter
        {
            get { return reporterVm; }
            private set { this.RaiseAndSetIfChanged(ref reporterVm, value); }
        }

        public AdvancedDeviceControlPanelViewModel AdvancedDeviceControlPanel
        {
            get { return advancedDeviceControlPanel; }
            private set { this.RaiseAndSetIfChanged(ref advancedDeviceControlPanel, value); }
        }

        #endregion

        #region Methods

        private void Manager_DeviceRenamed(object sender, IDevice device)
        {
            Refresh();
        }

        public void Refresh()
        {
            RefreshEvent?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// show error to user
        /// </summary>
        /// <param name="ex">Exception</param>
        private void ShowError(Exception ex)
        {
            classApplicationLogger.LogError(classApplicationLogger.CONST_STATUS_LEVEL_USER, ex.Message, ex);
        }

        /// <summary>
        /// remove selected connections and devices
        /// </summary>
        private void RemoveSelected()
        {
            var devicesToRemoveFromDeviceManager = fluidicsMod.RemoveSelectedDevices();
            foreach (var device in devicesToRemoveFromDeviceManager)
            {
                classDeviceManager.Manager.RemoveDevice(device);
            }
            fluidicsMod.RemoveSelectedConnections();
        }

        /// <summary>
        ///  save the current fluidics design as a bitmap
        /// </summary>
        /// <returns>the image of the current fluidics design</returns>
        public System.Drawing.Bitmap GetImage()
        {
            var r = fluidicsMod.GetBoundingBox();
            var bounds = new Rect(0, 0, r.Width + 150, r.Height + 150);
            var drawVisual = new DrawingVisual();
            var drawContext = drawVisual.RenderOpen();
            drawContext.PushClip(new RectangleGeometry(bounds));
            drawContext.DrawRectangle(Brushes.White, null, bounds);
            float scale = 1;
            fluidicsMod.Render(drawContext, 255, scale, Layer.Devices);
            fluidicsMod.Render(drawContext, 255, scale, Layer.Ports);
            fluidicsMod.Render(drawContext, 255, scale, Layer.Connections);
            drawContext.Pop();
            drawContext.Close();
            var rtb = new RenderTargetBitmap(800, 200, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawVisual);
            rtb.Freeze();
            return rtb.ToImage();
        }

        public List<Tuple<string, string, string>> ListDevicesAndStatus()
        {
            return fluidicsMod.ListDevicesAndStatus();
        }

        /// <summary>
        /// Saves the hardware configuration to the path.
        /// </summary>
        public void SaveConfiguration(string path = CONST_DEFAULT_CONFIG_FILEPATH)
        {
            var configuration = new classDeviceConfiguration
            {
                CartName = classLCMSSettings.GetParameter(classLCMSSettings.PARAM_CARTNAME)
            };

            classDeviceManager.Manager.ExtractToPersistConfiguration(ref configuration);
            var connectionIds = new List<long>();
            //For each device, extract the X,Y position for persistence.
            foreach (var device in fluidicsMod.GetDevices())
            {
                configuration.AddSetting(device.IDevice.Name, "dashboard-x", device.Loc.X);
                configuration.AddSetting(device.IDevice.Name, "dashboard-y", device.Loc.Y);
                configuration.AddSetting(device.IDevice.Name, "State", device.CurrentState);

                //for every port in a device, add any connection that is not internal to the device to the list of connections
                foreach (var port in device.Ports)
                {
                    foreach (var conn in port.Connections)
                    {
                        if (!connectionIds.Contains(conn.ID) && conn.InternalConnectionOf == null)
                        {
                            connectionIds.Add(conn.ID);
                            configuration.AddConnection(conn.ID.ToString(),
                                conn.P1.ID + ", " + conn.P2.ID + "," + conn.ConnectionStyle);
                        }
                    }
                }
            }

            var writer = new classINIDeviceConfigurationWriter();
            writer.WriteConfiguration(path, configuration);

            classApplicationLogger.LogMessage(0, string.Format("Saved device configuration to {0}.", path));
        }

        /// <summary>
        /// Loads the default hardware configuration.
        /// </summary>
        public void LoadConfiguration()
        {
            if (File.Exists(CONST_DEFAULT_CONFIG_FILEPATH))
            {
                LoadConfiguration(CONST_DEFAULT_CONFIG_FILEPATH);
            }
        }

        /// <summary>
        /// Loads the hardware configuration from file.
        /// </summary>
        /// <param name="path"></param>
        public void LoadConfiguration(string path)
        {
            fluidicsMod.BeginModelSuspension();
            var reader = new classINIDeviceConfigurationReader();
            var configuration = reader.ReadConfiguration(path);

            classDeviceManager.Manager.LoadPersistentConfiguration(configuration);

            foreach (var device in fluidicsMod.GetDevices())
            {
                try
                {
                    var settings = configuration.GetDeviceSettings(device.IDevice.Name);
                    if (settings.ContainsKey("dashboard-x") && settings.ContainsKey("dashboard-y"))
                    {
                        var x = Convert.ToInt32(settings["dashboard-x"]);
                        var y = Convert.ToInt32(settings["dashboard-y"]);
                        device.MoveBy(new Point(x, y));
                    }
                    if (settings.ContainsKey("State"))
                    {
                        var stateAsInt = Convert.ToInt32(settings["State"]);
                        device.ActivateState(stateAsInt);
                    }
                }
                catch (Exception ex)
                {
                    classApplicationLogger.LogError(classApplicationLogger.CONST_STATUS_LEVEL_DETAILED, "Could not load the position or state of the device.", ex);
                }
            }
            var connections = configuration.GetConnections();
            foreach (var connection in connections.Keys)
            {
                var delimiter = new[] { ",", "\n" };
                var properties = connections[connection].Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                if (properties.Length == 3)
                {
                    try
                    {
                        fluidicsMod.CreateConnection(properties[0].Trim(), properties[1].Trim(), properties[2].Trim());
                    }
                    catch (Exception)
                    {
                        classApplicationLogger.LogError(classApplicationLogger.CONST_STATUS_LEVEL_USER, "Unable to create connection between " + properties[0] + " " + properties[1]);
                    }
                }
                else
                {
                    classApplicationLogger.LogError(classApplicationLogger.CONST_STATUS_LEVEL_USER, "Unable to create connection, specified ports invalid or missing connection style." + " port1: " + properties[0] + " port2: " + properties[1] + " style: " + properties[2]);
                }
            }
            Trace.WriteLine("Configuration Loaded");
            fluidicsMod.EndModelSuspension(true);
        }

        /// <summary>
        /// Adds the currently selected device to the dashboard.
        /// </summary>
        private void AddDeviceToDeviceManager()
        {
            var controller = new DeviceAddController();
            var addWindowVm = new DeviceAddViewModel();
            addWindowVm.AddPluginInformation(controller.GetAvailablePlugins());
            var addWindow = new DeviceAddWindow()
            {
                DataContext = addWindowVm,
            };

            var result = addWindow.ShowDialog();

            if (!result.HasValue || !result.Value)
            {
                return;
            }

            var plugins = addWindowVm.GetSelectedPlugins();
            var failedDevices = controller.AddDevices(plugins, addWindowVm.InitializeOnAdd);

            if (failedDevices == null || failedDevices.Count <= 0)
                return;

            var displayVm = new FailedDevicesViewModel(failedDevices);
            var display = new FailedDevicesWindow()
            {
                DataContext = displayVm,
            };

            display.ShowDialog();
        }

        /// <summary>
        /// Loads the hardware configuration.
        /// </summary>
        private void LoadHardware()
        {
            var deviceCount = classDeviceManager.Manager.DeviceCount;
            if (deviceCount > 0)
            {
                var result = MessageBox.Show("Do you want to clear the existing device configuration?", "Clear Configuration", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = CONST_HARDWARE_CONFIG_FILTER;
            openFileDialog.FilterIndex = 0;
            var openResult = openFileDialog.ShowDialog();
            if (openResult.HasValue && openResult.Value)
            {
                // The device manager sends us an event when it removes the devices.
                // Since this is an event driven architecture, we don't have to worry about explicitly
                // clearing our glyphs.
                fluidicsMod.BeginModelSuspension();
                classDeviceManager.Manager.ShutdownDevices(true);
                fluidicsMod.EndModelSuspension(true);
                // Then we actually load the next data
                LoadConfiguration(openFileDialog.FileName);

                classApplicationLogger.LogMessage(0, "Device configuration loaded from " + openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Saves the hardware configuration to a new path.
        /// </summary>
        private void SaveHardwareAs()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = CONST_HARDWARE_CONFIG_FILTER;
            saveFileDialog.FilterIndex = 0;
            var saveResult = saveFileDialog.ShowDialog();
            if (saveResult.HasValue && saveResult.Value)
            {
                SaveConfiguration(saveFileDialog.FileName);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Initialize devices.
        /// </summary>
        private void InitializeDevices()
        {
            var initializedCount = classDeviceManager.Manager.InitializedDeviceCount;

            var reinitialize = false;
            if (initializedCount > 0)
            {
                var result =
                    MessageBox.Show("Some devices are initialized already.  Do you want to re-initialize those?",
                        "Initialization", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    reinitialize = true;
                }
            }

            var failedDevices = classDeviceManager.Manager.InitializeDevices(reinitialize);
            if (failedDevices != null && failedDevices.Count > 0)
            {
                var displayVm = new FailedDevicesViewModel(failedDevices);
                var display = new FailedDevicesWindow()
                {
                    DataContext = displayVm,
                };

                display.ShowDialog();
            }
        }

        /// <summary>
        /// event handler for when a device is added to the system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="device"></param>
        void Manager_DeviceAdded(object sender, IDevice device)
        {
            try
            {
                fluidicsMod.AddDevice(device);
            }
            //this should never, ever, happen
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        /// <summary>
        /// event handler for when a device is removed from the system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="device"></param>
        void Manager_DeviceRemoved(object sender, IDevice device)
        {
            try
            {
                fluidicsMod.RemoveDevice(device);
            }
            //shouldn't ever happen.
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        /// <summary>
        /// event handler for when a fluidics model changes.
        /// </summary>
        private void FluidicsModelChanged()
        {
            ChangeHandler(this, new EventArgs());
        }

        /// <summary>
        /// handle change in fluidics devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeHandler(object sender, EventArgs e)
        {
            //System.Diagnostics.Trace.WriteLine("changeHandler sender: " + sender.ToString());
            FluidicsControlVm.UpdateImage();
            FluidicsControlVm.Refresh();
            RefreshEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// When btnConnect is clicked, attempt to create a new connection between two ports and update the panel
        /// </summary>
        private void ConnectDevices()
        {
            try
            {
                fluidicsMod.CreateConnection();
                fluidicsMod.DeselectPorts();
            }
            catch (ArgumentException ex)
            {
                ShowError(ex);
            }
            catch (ApplicationException ex)
            {
                ShowError(ex);
            }
            // shouldn't ever get to here
            catch (Exception ex)
            {
                ShowError(ex);
            }
            //this.panelFluidicsDesign.Invalidate();
        }

        /// <summary>
        /// when btnRemove is clicked, attempt to remove selected connections
        /// </summary>
        private void RemoveDevice()
        {
            try
            {
                var areYouSure = MessageBox.Show("Are you sure you want to delete this device or connection?", "Delete Device", MessageBoxButton.YesNo);

                if (areYouSure == MessageBoxResult.Yes)
                {
                    RemoveSelected();
                }
            }
            //shouldn't ever get this
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> LoadCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; private set; }
        public ReactiveCommand<Unit, bool> LockUnlockCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> InitializeCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> AddCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> RemoveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; private set; }

        private void SetupCommands()
        {
            LoadCommand = ReactiveCommand.Create(() => this.LoadHardware(), this.WhenAnyValue(x => x.DevicesUnlocked));
            SaveCommand = ReactiveCommand.Create(() => this.SaveConfiguration(), this.WhenAnyValue(x => x.DevicesUnlocked));
            SaveAsCommand = ReactiveCommand.Create(() => this.SaveHardwareAs(), this.WhenAnyValue(x => x.DevicesUnlocked));
            LockUnlockCommand = ReactiveCommand.Create(() => DevicesLocked = !DevicesLocked);
            InitializeCommand = ReactiveCommand.Create(() => this.InitializeDevices(), this.WhenAnyValue(x => x.DevicesUnlocked));
            AddCommand = ReactiveCommand.Create(() => this.AddDeviceToDeviceManager(), this.WhenAnyValue(x => x.DevicesUnlocked));
            RemoveCommand = ReactiveCommand.Create(() => this.RemoveDevice(), this.WhenAnyValue(x => x.DevicesUnlocked));
            ConnectCommand = ReactiveCommand.Create(() => this.ConnectDevices(), this.WhenAnyValue(x => x.DevicesUnlocked));
        }

        #endregion
    }
}
