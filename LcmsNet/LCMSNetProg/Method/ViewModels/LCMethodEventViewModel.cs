﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using LcmsNet.Method.Forms;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Method;
using ReactiveUI;

namespace LcmsNet.Method.ViewModels
{
    public delegate void DelegateLCMethodEventOptimize(object sender, bool optimize);

    public delegate void DelegateLCMethodEventLocked(object sender, bool enabled, classLCMethodData methodData);

    /// <summary>
    /// LC-Method User Interface for building an LC-Method.
    /// </summary>
    public class LCMethodEventViewModel : ReactiveObject
    {
        /// <summary>
        /// Default constructor for the event view model that takes no arguments
        /// Calling this constructor is only for the IDE designer.
        /// </summary>
        [Obsolete("For WPF Design time use only.", true)]
        public LCMethodEventViewModel()
        {
            SelectedDevice = null;
            EventNumber = "1";
            StoppedHere = false;
            Initialize();
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LCMethodEventViewModel(int eventNum)
        {
            SelectedDevice = null;
            EventNumber = eventNum.ToString();
            StoppedHere = false;
            Initialize();
        }

        /// <summary>
        /// Constructor for an unlocking event.
        /// </summary>
        /// <param name="device"></param>
        public LCMethodEventViewModel(classLCMethodData methodData, bool locked)
        {
            Initialize();

            SelectedDevice = methodData.Device;
            // Every device is a reference held in the device manager...except for the timer
            // object.  This object is created every time as a non-critical object because
            // it does not require simultaneous use by various threads.  However, we have to
            // make sure here the device is not a timer, because if it is, then we must
            // find the device name string "Timer" from the combo box so we can
            // get the right reference.
            if (!(methodData.Device is classTimerDevice))
            {
                SelectedDevice = methodData.Device;
            }
            else
            {
                foreach (var d in DevicesComboBoxOptions)
                {
                    if (d.Name.Equals("Timer"))
                    {
                        SelectedDevice = d;
                        break;
                    }
                }
            }

            DevicesComboBoxEnabled = (locked == false);
            EventUnlocked = (locked == false);

            OptimizeWith = methodData.OptimizeWith;

            if (locked)
            {
                // Create a dummy classLCMethodData
                var tempObj = new classLCMethodData(null, null, new classLCMethodAttribute("Unlock", 0.0, "", 0, false), null);
                methodsComboBoxOptions.Add(tempObj);
                SelectedLCMethod = tempObj;
            }
            else
            {
                LoadMethodInformation(SelectedDevice);
                // We have to do some reference trickery here, since method data is reconstructed
                // for every lcevent then we need to set the method data for this object
                // with the method we previously selected. This way we preserve the parameter values etc.
                var index = FindMethodIndex(methodData);
                methodsComboBoxOptions[index] = methodData;
                SelectedLCMethod = methodData;

                LoadMethodParameters(methodData);
            }

            isLockingEvent = locked;
            this.methodData = methodData;
        }

        /// <summary>
        /// Fired when the user wants to optimize with this event.
        /// </summary>
        public event DelegateLCMethodEventOptimize UseForOptimization;

        /// <summary>
        /// Fired when this event is to be locked.
        /// </summary>
        public event DelegateLCMethodEventLocked Lock;

        /// <summary>
        /// Fired when an event changes.
        /// </summary>
        public event EventHandler EventChanged;

        ~LCMethodEventViewModel()
        {
            methodData.BreakPointEvent -= BreakPointEvent_Handler;
            methodData.Simulated -= Simulated_Handler;
            methodData.SimulatingEvent -= Simulating_Handler;
        }

        /// <summary>
        /// Initializes the device mappings structures and registers events to listen for data from the device manager.
        /// </summary>
        private void Initialize()
        {
            // Add the devices to the method editor
            deviceMappings = new Dictionary<IDevice, List<classLCMethodData>>();
            RegisterDevices();
            Breakpoint = new BreakpointViewModel();

            // Handle user interface events to display context of method editors
            this.WhenAnyValue(x => x.SelectedDevice).Subscribe(x => this.SelectedDeviceChanged());
            this.WhenAnyValue(x => x.SelectedLCMethod).Subscribe(x => this.SelectedMethodChanged());
            this.WhenAnyValue(x => x.OptimizeWith).Subscribe(x => this.OptimizeForChanged());
            Breakpoint.BreakpointChanged += Breakpoint_Changed;

            // Register to listen for device additions or deletions.
            classDeviceManager.Manager.DeviceAdded += Manager_DeviceAdded;
            classDeviceManager.Manager.DeviceRemoved += Manager_DeviceRemoved;
            classDeviceManager.Manager.DeviceRenamed += Manager_DeviceRenamed;
        }

        void Breakpoint_Changed(object sender, BreakpointArgs e)
        {
            methodData.BreakPoint = e.IsSet;
        }

        private int FindMethodIndex(classLCMethodData method)
        {
            var i = 0;
            foreach (var data in MethodsComboBoxOptions)
            {
                if (data != null)
                {
                    if (data.MethodAttribute.Name.Equals(method.MethodAttribute.Name))
                        return i;
                }
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Internal class that provides methods to map types to boolean values for testing.
        /// </summary>
        internal static class classParameterTypeFactory
        {
            /// <summary>
            /// Returns true if the type is a double, short, long, int, uint, ushort, ulong, or float.
            /// </summary>
            /// <param name="t">Type to interrogate.</param>
            /// <returns>True if numeric, false if not or null.</returns>
            public static bool IsNumeric(Type t)
            {
                if (t == null)
                    return false;

                var isNumeric = false;
                isNumeric = isNumeric || (typeof(int) == t);
                isNumeric = isNumeric || (typeof(uint) == t);
                isNumeric = isNumeric || (typeof(ulong) == t);
                isNumeric = isNumeric || (typeof(long) == t);
                isNumeric = isNumeric || (typeof(short) == t);
                isNumeric = isNumeric || (typeof(ushort) == t);
                isNumeric = isNumeric || (typeof(double) == t);
                isNumeric = isNumeric || (typeof(float) == t);
                return isNumeric;
            }
        }

        #region Members

        /// <summary>
        /// Method data that has been selected to be displayed.
        /// </summary>
        private classLCMethodData methodData;

        /// <summary>
        /// List of device methods and parameters to use.
        /// </summary>
        private Dictionary<IDevice, List<classLCMethodData>> deviceMappings;

        /// <summary>
        /// Flag indicating if this event is a placeholder so that we know it's an unlocking event
        /// </summary>
        private readonly bool isLockingEvent;

        private BreakpointViewModel breakpoint;
        private string eventNumber = "1";
        private bool optimizeWith = false;
        private IDevice selectedDevice = null;
        private classLCMethodData selectedLCMethod = null;
        private readonly ReactiveList<IDevice> devicesComboBoxOptions = new ReactiveList<IDevice>();
        private readonly ReactiveList<classLCMethodData> methodsComboBoxOptions = new ReactiveList<classLCMethodData>();
        private readonly ReactiveList<EventParameterViewModel> eventParameterList = new ReactiveList<EventParameterViewModel>();
        private bool devicesComboBoxEnabled = true;
        private bool eventUnlocked = true;
        private bool isSelected = false;

        #endregion

        #region Properties

        public BreakpointViewModel Breakpoint
        {
            get { return breakpoint; }
            set { this.RaiseAndSetIfChanged(ref breakpoint, value); }
        }

        public string EventNumber
        {
            get { return eventNumber; }
            set { this.RaiseAndSetIfChanged(ref eventNumber, value); }
        }

        /// <summary>
        /// The selected device
        /// </summary>
        public IDevice SelectedDevice
        {
            get { return selectedDevice; }
            set { this.RaiseAndSetIfChanged(ref selectedDevice, value); }
        }

        /// <summary>
        /// The selected method
        /// </summary>
        public classLCMethodData SelectedLCMethod
        {
            get { return selectedLCMethod; }
            set { this.RaiseAndSetIfChanged(ref selectedLCMethod, value); }
        }

        public IReadOnlyReactiveList<IDevice> DevicesComboBoxOptions => devicesComboBoxOptions;
        public IReadOnlyReactiveList<classLCMethodData> MethodsComboBoxOptions => methodsComboBoxOptions;
        public IReadOnlyReactiveList<EventParameterViewModel> EventParameterList => eventParameterList;

        public bool DevicesComboBoxEnabled
        {
            get { return devicesComboBoxEnabled; }
            private set { this.RaiseAndSetIfChanged(ref devicesComboBoxEnabled, value); }
        }

        public bool EventUnlocked
        {
            get { return eventUnlocked; }
            private set { this.RaiseAndSetIfChanged(ref eventUnlocked, value); }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { this.RaiseAndSetIfChanged(ref isSelected, value); }
        }

        /// <summary>
        /// Gets the method selected to run by the user.
        /// </summary>
        public classLCMethodData SelectedMethod
        {
            get
            {
                if (methodData != null)
                {
                    // Link the method to a device
                    methodData.Device = SelectedDevice;
                    methodData.OptimizeWith = OptimizeWith;
                    methodData.BreakPoint = Breakpoint.IsSet;

                    // Make sure that we build the method so that the values are updated
                    // from the control used to interface them....
                    methodData.BuildMethod();
                }
                methodData.BreakPointEvent += BreakPointEvent_Handler;
                methodData.Simulated += Simulated_Handler;
                methodData.SimulatingEvent += Simulating_Handler;
                return methodData;
            }
        }

        public void SetBreakPoint(bool value)
        {
            Breakpoint.IsSet = value;
        }

        public SolidColorBrush EventBackColor
        {
            get
            {
                if (IsCurrent && !StoppedHere)
                {
                    return Brushes.Green;
                }
                if (StoppedHere)
                {
                    return Brushes.Yellow;
                }
                if (methodData.BreakPoint)
                {
                    return Brushes.Maroon;
                }
                return Brushes.White;
            }
        }

        /// <summary>
        /// Gets or sets whether to optimize the method alignment with this event.
        /// </summary>
        public bool OptimizeWith
        {
            get { return optimizeWith; }
            set { this.RaiseAndSetIfChanged(ref optimizeWith, value); }
        }

        /// <summary>
        /// Gets flag indicating whether this event editor is a placeholder for a locking event.
        /// </summary>
        public bool IsLockingEvent => isLockingEvent;

        private bool IsCurrent { get; set; }

        private bool StoppedHere { get; set; }

        #endregion

        #region Device Manager Event Listeners

        /// <summary>
        /// Handles when a device is renamed, it updates the internal device data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="device"></param>
        void Manager_DeviceRenamed(object sender, IDevice device)
        {
            if (device.DeviceType == enumDeviceType.Fluidics)
                return;

            if (devicesComboBoxOptions.Contains(device))
            {
                var index = devicesComboBoxOptions.IndexOf(device);
                devicesComboBoxOptions[index] = device;
            }
        }

        /// <summary>
        /// Updates the list of available devices.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="device"></param>
        void Manager_DeviceRemoved(object sender, IDevice device)
        {
            if (device.DeviceType == enumDeviceType.Fluidics)
                return;

            if (devicesComboBoxOptions.Contains(device))
                devicesComboBoxOptions.Remove(device);

            if (deviceMappings.ContainsKey(device))
                deviceMappings.Remove(device);

            if (devicesComboBoxOptions.Count < 1)
                DevicesComboBoxEnabled = false;
        }

        /// <summary>
        /// Updates the list of available devices.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="device"></param>
        void Manager_DeviceAdded(object sender, IDevice device)
        {
            // If this was the first device added for some odd reason, then make sure we enable the device.
            var isFirstDevice = false;

            if (device.DeviceType == enumDeviceType.Fluidics)
                return;


            if (devicesComboBoxOptions.Count < 1)
            {
                DevicesComboBoxEnabled = true;
                isFirstDevice = true;
            }

            if (devicesComboBoxOptions.Contains(device) == false)
                devicesComboBoxOptions.Add(device);

            if (deviceMappings.ContainsKey(device) == false)
            {
                var methodPairs = ReflectDevice(device);
                deviceMappings.Add(device, methodPairs);
            }

            // Make sure we select a device
            if (isFirstDevice)
            {
                if (devicesComboBoxOptions.Count > 0)
                {
                    SelectedDevice = devicesComboBoxOptions[0];
                }
                UpdateSelectedDevice();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays the given device method names and selected controls.
        /// </summary>
        /// <param name="device"></param>
        private void LoadMethodInformation(IDevice device)
        {
            // Make sure the device is not null
            if (device == null)
                return;

            // We know that timer devices are always re-created.  Meaning their references
            // are not persisted throughout a method or across methods.  Thus we need to test to see
            // if this is true, if so, then we need to just find the device in the
            // device manager and get the device method mappings that way.
            if (device is classTimerDevice)
            {
                // Find the timer device.
                foreach (var tempDevice in deviceMappings.Keys)
                {
                    if (tempDevice is classTimerDevice)
                    {
                        device = tempDevice;
                        break;
                    }
                }
            }
            // Add the method information into the combo-box as deemed by the device.
            var methods = deviceMappings[device];
            using (methodsComboBoxOptions.SuppressChangeNotifications())
            {
                // Clear out the combo-box
                methodsComboBoxOptions.Clear();
                methodsComboBoxOptions.AddRange(methods);
            }

            if (methodsComboBoxOptions.Count > 0)
            {
                SelectedLCMethod = methodsComboBoxOptions[0];
                UpdateSelectedMethod();
            }
        }

        /// <summary>
        /// Displays the given device method names and selected controls.
        /// </summary>
        /// <param name="method"></param>
        private void LoadMethodParameters(classLCMethodData method)
        {
            // Make sure the device is not null
            if (method == null)
                return;

            // Clear out the combo-box
            try
            {
                foreach (var vm in eventParameterList)
                {
                    vm.EventChanged -= param_EventChanged;
                }
            }
            catch
            {
            }
            using (eventParameterList.SuppressChangeNotifications())
            {
                eventParameterList.Clear();
            }
            //mpanel_parameters.ColumnStyles.Clear();

            // If the method requires sample input then we just ignore adding any controls.
            var parameters = method.Parameters;

            // This readjusts the number of parameters that are sample specific
            var count = parameters.Controls.Count * 2;
            var indexOfSampleData = method.MethodAttribute.SampleParameterIndex;

            // Update the style so we have the right spacing
            //var percent = 100.0F / Convert.ToSingle(count);
            //for (var i = 0; i < parameters.Controls.Count; i++)
            //{
            //    if (i != indexOfSampleData)
            //        mpanel_parameters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize, percent));
            //}

            // Add the controls to the mpanel_parameter controls
            for (var j = 0; j < parameters.Controls.Count; j++)
            {
                if (j != indexOfSampleData)
                {
                    // Get the name of the parameter
                    var name = parameters.Names[j];

                    // Get the control for the parameter
                    var control = parameters.Controls[j];

                    // Construct the description for the parameter
                    var vm = GetEventParameterFromControl(control);
                    vm.WinFormsParameter = (ILCEventParameter)control;
                    vm.ParameterLabel = name;

                    // Add the control itself
                    eventParameterList.Add(vm);

                    vm.ParameterValue = parameters.Values[j];
                    vm.EventChanged += param_EventChanged;
                }
            }
        }

        void param_EventChanged(object sender, EventArgs e)
        {
            OnEventChanged();
        }

        /// <summary>
        /// Updates the method and device user interface items.
        /// </summary>
        private void UpdateSelectedDevice()
        {
            // Update the user interface.
            if (SelectedDevice != null)
            {
                if (deviceMappings.ContainsKey(SelectedDevice) == false)
                {
                    ReflectDevice(SelectedDevice);
                }
                LoadMethodInformation(SelectedDevice);
            }
        }

        private void UpdateSelectedMethod()
        {
            // Update the user interface.
            var method = SelectedLCMethod;
            if (method != methodData)
            {
                if (methodData != null)
                {
                    methodData.BreakPointEvent -= BreakPointEvent_Handler;
                    methodData.Simulated -= Simulated_Handler;
                }
                methodData = method;
                if (method != null)
                {
                    methodData.BreakPointEvent += BreakPointEvent_Handler;
                    methodData.Simulated += Simulated_Handler;
                    if (SelectedDevice != null)
                    {
                        LoadMethodParameters(methodData);
                    }
                }
            }
        }

        /// <summary>
        /// Fires the event changed event.
        /// </summary>
        private void OnEventChanged()
        {
            EventChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Updates the label to display which number this event is in the list of events.
        /// </summary>
        /// <param name="eventNum">integer representing the place in the event list of this event</param>
        public void UpdateEventNum(int eventNum)
        {
            EventNumber = eventNum.ToString();
        }

        #endregion

        #region Registration and Reflection

        /// <summary>
        /// Registers the devices with the user interface from the device manager.
        /// </summary>
        private void RegisterDevices()
        {
            foreach (var device in classDeviceManager.Manager.Devices)
            {
                if (device.DeviceType == enumDeviceType.Fluidics)
                    continue;

                var methodPairs = ReflectDevice(device);
                deviceMappings.Add(device, methodPairs);
                devicesComboBoxOptions.Add(device);
            }

            if (devicesComboBoxOptions.Count > 0)
            {
                SelectedDevice = devicesComboBoxOptions[0];
                UpdateSelectedDevice();
            }
            else
            {
                DevicesComboBoxEnabled = false;
            }
        }

        /// <summary>
        /// Reflects the given device and puts the method and parameter information in the appropiate combo boxes.
        /// </summary>
        public List<classLCMethodData> ReflectDevice(IDevice device)
        {
            if (device == null)
                throw new NullReferenceException("Device cannot be null.");

            var type = device.GetType();

            // List of method editing pairs
            var methodPairs = new List<classLCMethodData>();

            // We are trying to enumerate all the methods for this device building their method-parameter pairs.
            foreach (var method in type.GetMethods())
            {
                var customAttributes = method.GetCustomAttributes(typeof(classLCMethodAttribute), true);
                foreach (var objAttribute in customAttributes)
                {
                    // If the method has a custom LC Method Attribute, then we want to look at the parameters used
                    var attr = objAttribute as classLCMethodAttribute;
                    if (attr != null)
                    {
                        // Grab the parameters used for this method
                        var info = method.GetParameters();
                        var parameters = new classLCMethodEventParameter();

                        // Here we are looking to see if the method has a parameter
                        // that requires a data provider.
                        if (info.Length > 0)
                        {
                            // Make sure that we have parameter data, and also make sure that
                            // the parameter we are going to use is a sample data object.
                            // Then for each parameter, see if we can add it to a control to display.
                            var i = 0;
                            foreach (var paramInfo in info)
                            {
                                Control control = null;
                                object value = null;

                                // If the method editor has to use sample data then
                                // we skip adding a control...but allow for
                                // other data to be loaded.
                                if (attr.RequiresSampleInput && i == attr.SampleParameterIndex)
                                {
                                    parameters.AddParameter(null, null, paramInfo.Name, attr.DataProvider);
                                }
                                else if (string.IsNullOrEmpty(attr.DataProvider) == false && i == attr.DataProviderIndex)
                                {
                                    // Figure out what index to adjust the data provider for.
                                    var combo = new controlParameterComboBox();

                                    // Register the event to automatically get new data when the data provider has new stuff.
                                    device.RegisterDataProvider(attr.DataProvider, combo.FillData);
                                    control = combo;
                                    // Set the data if we have it, otherwise, cross your fingers batman!
                                    if (combo.Items.Count > 0)
                                        value = combo.Items[0];
                                    parameters.AddParameter(value, control, paramInfo.Name, attr.DataProvider);
                                }
                                else
                                {
                                    // Get a control to display
                                    control = GetControlFromType(paramInfo.ParameterType);

                                    // We need to get a default value, so just ask the
                                    // type for it.
                                    if (paramInfo.ParameterType.IsEnum)
                                    {
                                        var aenums = Enum.GetValues(paramInfo.ParameterType);
                                        var enums = new object[aenums.Length];
                                        aenums.CopyTo(enums, 0);
                                        value = enums[0];
                                    }

                                    // If the control is not null, then we can add it to display.
                                    // If it is null, then it is of a type we know nothing about.
                                    // And well you're SOL.
                                    if (control != null)
                                    {
                                        parameters.AddParameter(value, control, paramInfo.Name, attr.DataProvider);
                                    }
                                }
                                i++;
                            }
                        }

                        // Construct the new method from what we found
                        // during the reflection phase and add it to the list of
                        // possible methods to call for this device.
                        var newMethod = new classLCMethodData(device, method, attr, parameters);
                        methodPairs.Add(newMethod);
                    }
                }
            }
            return methodPairs;
        }

        /// <summary>
        /// Given a parameter type, figure out what kind of control is associated with it.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Control GetControlFromType(Type t)
        {
            Control control = null;

            if (t.IsEnum)
            {
                // Grab the enumeration values for the parameter
                var aenums = Enum.GetValues(t);
                var enums = new object[aenums.Length];
                aenums.CopyTo(enums, 0);

                // Add the parameters to the combo box before we do anything.
                var box = new controlParameterComboBox();
                box.DropDownStyle = ComboBoxStyle.DropDownList;
                box.Items.AddRange(enums);
                box.SelectedIndex = 0;
                control = box;
            }
            else if (classParameterTypeFactory.IsNumeric(t))
            {
                control = new controlParameterNumericUpDown();
            }
            else if (typeof(string) == t)
            {
                control = new controlParameterTextBox();
            }

            return control;
        }

        /// <summary>
        /// Given a parameter type, figure out what kind of control is associated with it.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static EventParameterViewModel GetEventParameterFromControl(Control c)
        {
            EventParameterViewModel control = null;

            if (c is controlParameterComboBox cpcb)
            {
                // Add the parameters to the combo box before we do anything.
                control = new EventParameterViewModel(EventParameterViewModel.ParameterTypeEnum.Enum);
                using (control.ComboBoxOptions.SuppressChangeNotifications())
                {
                    // Grab the enumeration values for the parameter
                    control.ComboBoxOptions.AddRange(cpcb.Items.Cast<object>());
                }
                control.SelectedOption = cpcb.SelectedItem;
            }
            else if (c is controlParameterNumericUpDown cpnud)
            {
                control = new EventParameterViewModel(EventParameterViewModel.ParameterTypeEnum.Numeric);
                control.NumberValue = Convert.ToDouble(cpnud.ParameterValue);
                control.DecimalPlaces = cpnud.DecimalPlaces;
            }
            else if (c is controlParameterTextBox cptb)
            {
                control = new EventParameterViewModel(EventParameterViewModel.ParameterTypeEnum.Text);
                control.TextValue = cptb.ParameterValue.ToString();
            }

            return control;
        }

        /// <summary>
        /// Given a parameter type, figure out what kind of control is associated with it.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static EventParameterViewModel GetEventParametersFromType(Type t)
        {
            EventParameterViewModel control = null;

            if (t.IsEnum)
            {
                // Add the parameters to the combo box before we do anything.
                control = new EventParameterViewModel(EventParameterViewModel.ParameterTypeEnum.Enum);
                using (control.ComboBoxOptions.SuppressChangeNotifications())
                {
                    // Grab the enumeration values for the parameter
                    control.ComboBoxOptions.AddRange(Enum.GetValues(t).Cast<object>());
                }
            }
            else if (classParameterTypeFactory.IsNumeric(t))
            {
                control = new EventParameterViewModel(EventParameterViewModel.ParameterTypeEnum.Numeric);
            }
            else if (typeof(string) == t)
            {
                control = new EventParameterViewModel(EventParameterViewModel.ParameterTypeEnum.Text);
            }

            return control;
        }

        #endregion

        #region Event handlers

        private void BreakPointEvent_Handler(object sender, BreakEventArgs e)
        {
            StoppedHere = e.IsStopped;
            //TODO: What?: Refresh();
        }

        private void Simulating_Handler(object sender, EventArgs e)
        {
            IsCurrent = true;
            //TODO: What?: Refresh();
        }

        private void Simulated_Handler(object sender, EventArgs e)
        {
            IsCurrent = false;
            //TODO: What?: Refresh();
        }

        /// <summary>
        ///  when the device changes update the user interface.
        /// </summary>
        private void SelectedDeviceChanged()
        {
            UpdateSelectedDevice();
            OnEventChanged();
        }

        /// <summary>
        /// Handles when the user selects a new method for the current device.
        /// </summary>
        private void SelectedMethodChanged()
        {
            UpdateSelectedMethod();
            OnEventChanged();
        }

        /// <summary>
        /// Tells the other devices that this method should be used in optimization.
        /// </summary>
        private void OptimizeForChanged()
        {
            UseForOptimization?.Invoke(this, OptimizeWith);
            methodData.OptimizeWith = OptimizeWith;
            OnEventChanged();
        }

        #endregion
    }
}
