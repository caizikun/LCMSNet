﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LcmsNetDataClasses.Devices;

namespace DemoPluginLibrary
{
    public partial class DemoPALAdvancedControl : controlBaseDeviceControl, IDeviceControl
    {
        DemoPAL m_device;

        public DemoPALAdvancedControl()
        {
            InitializeComponent();
        }

        public void RegisterDevice(IDevice device)
        {
            Device = device;
        }

        public void UnRegisterDevice()
        {
            Device = null;
        }

        public IDevice Device
        {
            get
            {
                return m_device as IDevice;
            }
            set
            {
                m_device = value as DemoPAL;
                SetBaseDevice(value);
            }
        }

        private void btnRunMethod_Click(object sender, EventArgs e)
        {
            // use a defaulted sampledata object since there's no sample associated with a user clicking "run"
            m_device.RunMethod(Convert.ToDouble(numTimeout.Value), new LcmsNetDataClasses.classSampleData(), comboMethod.SelectedItem.ToString());
        }


    }
}