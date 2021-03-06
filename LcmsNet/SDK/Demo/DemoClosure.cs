﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using LcmsNetDataClasses.Devices;
using FluidicsSDK.Devices;
using LcmsNetDataClasses.Method;
using LcmsNetSDK;

namespace DemoPluginLibrary
{
     [classDeviceControlAttribute(typeof(DemoClosureAdvanceControl),
                                 "Demo Closure",
                                 "Demo")]

    public class DemoClosure: IDevice, IFluidicsClosure
    {
        public DemoClosure()
        {
            Name = "Demo Closure";
        }

        #region Methods
        public bool Initialize(ref string errorMessage)
        {
            Status = enumDeviceStatus.Initialized;
            ErrorType = enumDeviceErrorStatus.NoError;
            return true;
        }

        public bool Shutdown()
        {
            return true;
        }

        public void RegisterDataProvider(string key, DelegateDeviceHasData remoteMethod)
        {

        }

        public void UnRegisterDataProvider(string key, DelegateDeviceHasData remoteMethod)
        {

        }

        public void WritePerformanceData(string directoryPath, string methodName, object[] parameters)
        {

        }

        public List<string> GetStatusNotificationList()
        {
            return new List<string>();
        }

        public List<string> GetErrorNotificationList()
        {
            return new List<string>();
        }

        /// <summary>
        /// Triggers a pulse of the specified voltage, lasting the specified duration.
        /// This is intended for use on the analog output ports--if it is a digital
        /// port the specified voltage will be disregarded.
        /// </summary>
        /// <param name="pulseLengthSeconds">The length of the pulse in seconds</param>
        /// <param name="portName">The port to send the voltage on</param>
        /// <param name="voltage">The voltage to set</param>
        [classLCMethodAttribute("Trigger With Voltage", enumMethodOperationTime.Parameter, "", -1, false)]
        public bool Trigger(int pulseLengthSeconds, string portName, double voltage)
        {
            //interact with hardware here.
            return true;
        }
        #endregion

        #region Events
        public event EventHandler<classDeviceStatusEventArgs> StatusUpdate
        {
            add { }
            remove { }
        }

        public event EventHandler<classDeviceErrorEventArgs> Error
        {
            add { }
            remove { }
        }

        public event EventHandler DeviceSaveRequired
        {
            add { }
            remove { }
        }
        #endregion

        #region Properties
        public enumDeviceType DeviceType => enumDeviceType.Component;

        public enumDeviceErrorStatus ErrorType
        {
            get;
            set;
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { this.RaiseAndSetIfChanged(ref name, value); }
        }

        public string Version
        {
            get;
            set;
        }

        public enumDeviceStatus Status
        {
            get;
            set;
        }

        public System.Threading.ManualResetEvent AbortEvent
        {
            get;
            set;
        }

        public bool Emulation
        {
            get;
            set;
        }
        #endregion

        public string GetClosureType()
        {
            return "DemoClosure";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
