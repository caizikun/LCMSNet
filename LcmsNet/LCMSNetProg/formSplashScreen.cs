﻿//*********************************************************************************************************
// Written by Dave Clark, Brian LaMarche for the US Department of Energy
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2009, Battelle Memorial Institute
// Created 03/27/2009
//
// Updates
// - 11/20/2009 (DAC) - Changed Status property to use a delegate to avoid cross-thread problems
//*********************************************************************************************************

using System.Drawing;
using System.Windows.Forms;

namespace LcmsNet
{
    /// <summary>
    /// Startup splash screen
    /// </summary>
    public partial class formSplashScreen : Form
    {
        #region "Properties"

        public string Status
        {
            set
            {
//                labelStatus.Text = value;
                UpdateStatus(value);
            }
        }

        public string SoftwareCopyright
        {
            set { lblCopyright.Text = value; }
        }

        public string SoftwareDevelopers
        {
            set { lblDevelopers.Text = value; }
        }

        #endregion

        #region "Delegates"

        /// <summary>
        /// Delegate for updating status without cross-thread issues
        /// </summary>
        /// <param name="newStatus"></param>
        private delegate void delegateUpdateStatus(string newStatus);

        #endregion

        #region "Methods"

        public formSplashScreen()
        {
            InitializeComponent();
            mlabel_version.Text += Application.ProductVersion;
        }

        private void UpdateStatus(string newStatus)
        {
            if (labelStatus.InvokeRequired)
            {
                var d = new delegateUpdateStatus(UpdateStatus);
                labelStatus.Invoke(d, newStatus);
            }
            else
            {
                labelStatus.Text = newStatus;
                labelStatus.Refresh();
                Application.DoEvents();
            }
        }

        public void SetEmulatedLabelVisibility(string cartName, bool visible)
        {
            if (visible)
            {
                mlabel_emulated.ForeColor = Color.Red;
                mlabel_emulated.Text = cartName + "\n [EMULATED] ";
            }
            else
            {
                mlabel_emulated.Text = cartName;
            }
        }

        #endregion
    }
}