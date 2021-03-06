﻿using System;
using System.Windows.Forms;
using LcmsNetDataClasses;

namespace LcmsNet
{
    public partial class ModelCheckReportViewer : UserControl
    {
        readonly IModelCheckController controller;

        public ModelCheckReportViewer()
        {
            InitializeComponent();
        }

        public ModelCheckReportViewer(IModelCheckController cntrlr)
        {
            InitializeComponent();
            controller = cntrlr;
            controller.ModelStatusChangeEvent += StatusChangeHandler;
        }

        private void StatusChangeHandler(object sender, ModelStatusChangeEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<ModelStatusChangeEventArgs>(StatusChangeHandler), sender, e);
            }
            else
            {
                foreach (var status in e.StatusList)
                {
                    var report = new ModelCheckReport(status);
                    panelMessages.Controls.Add(report);
                    report.Dock = DockStyle.Top;
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            panelMessages.Controls.Clear();
            ((Control) sender).Refresh();
                // this causes the control to redraw, which removes the autoscroll bars, if present.
        }
    }
}