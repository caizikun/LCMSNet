﻿using System.ComponentModel;

namespace LcmsNet.Simulator
{
    partial class formSimulatorControlsAndCharts
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // formSimulatorControlsAndCharts
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(772, 450);
            this.Name = "formSimulatorControlsAndCharts";
            this.Text = "LcmsNet Simulator Controls and Charts";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formSimulatorControlsAndCharts_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

    }
}