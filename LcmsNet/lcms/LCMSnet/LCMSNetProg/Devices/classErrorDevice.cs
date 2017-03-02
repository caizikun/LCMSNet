﻿using System;
using System.Collections.Generic;
using System.Threading;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Method;

namespace LcmsNet.Devices
{
    /// <summary>
    /// Error testing class.
    /// </summary>
    //[classDeviceMonitoring(enumDeviceMonitoringType.Message, "")]
    public class classErrorDevice : IDevice
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public classErrorDevice()
        {
            m_name = "ErrorDevice";
            m_version = "Version X";
            menum_status = enumDeviceStatus.NotInitialized;
        }

        /// <summary>
        /// Fired when the status changes.
        /// </summary>
        public event EventHandler<classDeviceStatusEventArgs> StatusUpdate;

        /// <summary>
        /// Fired when an error occurs in the device.
        /// </summary>
        public event EventHandler<classDeviceErrorEventArgs> Error;

        /// <summary>
        /// Fired when a property changes in the device.
        /// </summary>
        /// <remarks>This event is required by IDevice but this class does not use it</remarks>
        public event EventHandler DeviceSaveRequired
        {
            add { }
            remove { }
        }

        public void WritePerformanceData(string directoryPath, string name, object[] parameters)
        {
        }

        /// <summary>
        /// Returns the name of this device.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_name;
        }

        #region Members

        /// <summary>
        /// Name of this device.
        /// </summary>
        private string m_name;

        /// <summary>
        /// Version of this device.
        /// </summary>
        private string m_version;

        /// <summary>
        /// Status of the device currently.
        /// </summary>
        private enumDeviceStatus menum_status;

        #endregion

        #region IDevice Members

        /// <summary>
        /// Gets or sets the abort event for scheduling.
        /// </summary>
        public ManualResetEvent AbortEvent { get; set; }

        /// <summary>
        /// Gets or sets whether the device is emulated or not.
        /// </summary>
        public bool Emulation { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        public string Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        /// <summary>
        /// Gets or sets the status of this device.
        /// </summary>
        public enumDeviceStatus Status
        {
            get { return menum_status; }
            set
            {
                if (value != menum_status && StatusUpdate != null)
                    StatusUpdate(this, new classDeviceStatusEventArgs(value, "None", this));
                menum_status = value;
            }
        }

        public bool Initialize(ref string errorMessage)
        {
            return true;
        }

        public bool Shutdown()
        {
            return true;
        }

        public List<string> GetStatusNotificationList()
        {
            return new List<string>();
        }

        public List<string> GetErrorNotificationList()
        {
            var errors = new List<string>();
            errors.Add("Error!");
            return errors;
        }

        #endregion

        #region LC-Method Registered Methods

        /// <summary>
        /// Throws an exception.
        /// </summary>
        [classLCMethod("Throw Exception", 1, false, "", -1, false)]
        public bool ThrowException()
        {
            var dummyCode = false;
            if (!dummyCode)
                throw new Exception("This exception was thrown on purpose.");
            return false;
        }

        [classLCMethod("Return Error", 1, false, "", -1, false)]
        public bool ReturnError()
        {
            return false;
        }

        [classLCMethod("Event Error", 1, false, "", -1, false)]
        public void EventError()
        {
            Error?.Invoke(this,
                          new classDeviceErrorEventArgs("Error!", null, enumDeviceErrorStatus.ErrorSampleOnly, this, "Error!"));
        }

        /// <summary>
        /// This method times out, waiting indefinitely.
        /// </summary>
        /// <returns>False</returns>
        [classLCMethod("Timeout", 1, false, "", -1, false)]
        public bool Timeout()
        {
            Thread.Sleep(4000);
            return true;
        }

        /// <summary>
        /// This method times out, waiting indefinitely.
        /// </summary>
        /// <returns>False</returns>
        [classLCMethod("Wait Full Timeout", 1, false, "", -1, false)]
        public bool WaitUntilTimeout()
        {
            Thread.Sleep(1000);
            return true;
        }

        #endregion

        #region IDevice Data Provider Methods

        public void RegisterDataProvider(string key, DelegateDeviceHasData remoteMethod)
        {
        }

        public void UnRegisterDataProvider(string key, DelegateDeviceHasData remoteMethod)
        {
        }

        #endregion

        /*public Finch.Data.FinchComponentData GetData()
        {
            return null;
        }*/

        #region IDevice Members

        public enumDeviceErrorStatus ErrorType { get; set; }

        public enumDeviceType DeviceType => enumDeviceType.BuiltIn;

        #endregion
    }
}