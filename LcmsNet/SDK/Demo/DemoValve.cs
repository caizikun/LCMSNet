﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Method;
using FluidicsSDK.Devices;
using FluidicsSDK.Base;
using LcmsNetSDK;

namespace DemoPluginLibrary
{
    [classDeviceControlAttribute(typeof(DemoValveAdvancedControl),
                                 "Demo Valve - Two Position",
                                 "Demo")]
    public class DemoValve : IDevice, IFourPortValve
    {
        #region Methods
        public DemoValve()
        {
            Name = "Demo Valve";
            Version = "infinity.";
            Position = 1;
            AbortEvent = new System.Threading.ManualResetEvent(false);
        }

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

        [classLCMethodAttribute("GetPosition", 1.0, "", -1, false)]
        public int GetPosition()
        {
            return Position;
        }

        [classLCMethodAttribute("SetPosition", 1.0, "", -1, false)]
        public void SetPosition(TwoPositionState position)
        {
            if ((int)position < 0 || (int)position > 1)
            {
                throw new Exception("The position is invalid.");
            }
            Position = (int)position;
            PositionChanged?.Invoke(this, new ValvePositionEventArgs<TwoPositionState>(position));
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

        public event EventHandler<ValvePositionEventArgs<TwoPositionState>> PositionChanged;

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

        public int Position { get; set; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
