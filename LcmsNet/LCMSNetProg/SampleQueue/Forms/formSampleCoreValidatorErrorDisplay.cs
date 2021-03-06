﻿using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Experiment;

namespace LcmsNet.SampleQueue
{
    public partial class formSampleValidatorErrorDisplay : Form
    {
        public formSampleValidatorErrorDisplay(Dictionary<classSampleData, List<classSampleValidationError>> errors)
        {
            InitializeComponent();


            var i = 0;
            foreach (var sample in errors.Keys)
            {
                foreach (var error in errors[sample])
                {
                    i = i + 1;
                    var item = new ListViewItem();
                    item.Text = sample.DmsData.DatasetName;
                    item.SubItems.Add(error.Error);
                    if ((i % 2) == 0)
                        item.BackColor = Color.LightGray;
                    mlistview_errors.Items.Add(item);
                }
            }
        }
    }
}