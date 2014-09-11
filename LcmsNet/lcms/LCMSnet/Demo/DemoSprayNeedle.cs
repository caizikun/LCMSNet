﻿/*********************************************************************************************************
 * Written by Brian LaMarche and Christopher Walters for U.S. Department of Energy
 * Pacific Northwest National Laboratory, Richland, WA
 * Copyright 2013 Battle Memorial Institute
 * Created 12/31/2013
 * 
 * Last Modified 12/31/2013 By Christopher Walters 
 ********************************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO.Ports;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Method;
using FluidicsSDK.Devices;
using System.Timers;
using FluidicsSDK.Base;

namespace DemoPluginLibrary
{
    [classDeviceControlAttribute(null,
                                 typeof(FluidicsSprayNeedle),
                                 "Test Spray Needle",
                                 "Test")]
    public class DemoSprayNeedle:IDevice
    {
           public DemoSprayNeedle()
           {
               Name = "Stupid Needle";
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

            #region Events
            public event EventHandler<classDeviceStatusEventArgs> StatusUpdate;

            public event EventHandler<classDeviceErrorEventArgs> Error;

            public event EventHandler DeviceSaveRequired;

            public event EventHandler<ValvePositionEventArgs<TwoPositionState>> PositionChanged;
            #endregion

            #region Properties
            public enumDeviceType DeviceType
            {
                get { return enumDeviceType.Component; }
            }

            public enumDeviceErrorStatus ErrorType
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
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
    }
}
