﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LcmsNet.Devices.Pumps
{
    public partial class formAgilentPumpPurge : Form
    {
        /// <summary>
        /// Pump to purge.
        /// </summary>
        private classPumpAgilent mobj_pump;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pump"></param>
        public formAgilentPumpPurge(classPumpAgilent pump)
        {
            InitializeComponent();

            mobj_pump = pump;
            pump.DeviceSaveRequired += new EventHandler(pump_DeviceSaveRequired);
            Text = "Purge Pumps " + mobj_pump.Name;
        }

        void pump_DeviceSaveRequired(object sender, EventArgs e)
        {
            Text = "Purge Pumps " + mobj_pump.Name;
        }

        private void mbutton_purgeB2_Click(object sender, EventArgs e)
        {            
            double flow  = Convert.ToDouble(mnum_flowB2.Value);
            double mins  = Convert.ToDouble(mnum_timeB2.Value);
            mobj_pump.PurgePump(0, enumPurgePumpChannel.B2, flow, mins);
        }

        private void mbutton_purgeB1_Click(object sender, EventArgs e)
        {
            double flow = Convert.ToDouble(mnum_flowB1.Value);
            double mins = Convert.ToDouble(mnum_timeB1.Value);
            mobj_pump.PurgePump(0, enumPurgePumpChannel.B1, flow, mins);
        }

        private void mbutton_purgeA2_Click(object sender, EventArgs e)
        {
            double flow = Convert.ToDouble(mnum_flowA2.Value);
            double mins = Convert.ToDouble(mnum_timeA2.Value);
            mobj_pump.PurgePump(0, enumPurgePumpChannel.A2, flow, mins);
        }

        private void mbutton_purgeA1_Click(object sender, EventArgs e)
        {
            double flow = Convert.ToDouble(mnum_flowA1.Value);
            double mins = Convert.ToDouble(mnum_timeA1.Value);
            mobj_pump.PurgePump(0, enumPurgePumpChannel.A1, flow, mins);
        }

        private void mbutton_abortPurges_Click(object sender, EventArgs e)
        {
            mobj_pump.AbortPurges(0);
        }     
    }
}
