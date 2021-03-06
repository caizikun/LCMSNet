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

namespace ASUTGen.Devices.Filtration
{
    public partial class FilterChangerControl : UserControl, IDeviceControl
    {
        /// <summary>
        /// Notification driver object.
        /// </summary>
        private FilterChanger mobj_valve;

        public FilterChangerControl()
        {
            InitializeComponent();
        }

        public void RegisterDevice(IDevice device)
        {
            mobj_valve = device as FilterChanger;
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
                return mobj_valve;
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
