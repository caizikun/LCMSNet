﻿using System;
using LcmsNetDataClasses.Devices;

namespace FailureInjector.Drivers
{
    public partial class controlNotificationDriver : controlBaseDeviceControl, IDeviceControl
    {
        /// <summary>
        /// Notification driver object.
        /// </summary>
        private NotificationDriver m_driver;

        public controlNotificationDriver()
        {
            InitializeComponent();
        }

        private void mbutton_injectFailure_Click(object sender, EventArgs e)
        {
            m_driver.InjectFailure();
        }
        
        public void RegisterDevice(IDevice device)
        {
            m_driver = device as NotificationDriver;
            SetBaseDevice(m_driver);
        }
        #region IDeviceControl Members

        public event DelegateNameChanged  NameChanged;
        public event DelegateSaveRequired SaveRequired;

        public bool Running
        {
            get;
            set;
        }

        public IDevice Device
        {
            get
            {
                return m_driver;
            }
            set
            {
                RegisterDevice(value);
            }
        }

        public void ShowProps()
        {
            
        }
        #endregion
    }
}
