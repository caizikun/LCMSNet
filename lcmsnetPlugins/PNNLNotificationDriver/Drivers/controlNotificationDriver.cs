﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LcmsNetDataClasses.Logging;
using LcmsNetDataClasses.Devices;

namespace FailureInjector.Drivers
{
    public partial class controlNotificationDriver : controlBaseDeviceControl, IDeviceControl
    {
        /// <summary>
        /// Notification driver object.
        /// </summary>
        private NotificationDriver mobj_driver;

        public controlNotificationDriver()
        {
            InitializeComponent();
        }

        private void mbutton_injectFailure_Click(object sender, EventArgs e)
        {
            mobj_driver.InjectFailure();
        }
        
        public void RegisterDevice(IDevice device)
        {
            mobj_driver = device as NotificationDriver;
            SetBaseDevice(mobj_driver);
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
                return mobj_driver;
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
