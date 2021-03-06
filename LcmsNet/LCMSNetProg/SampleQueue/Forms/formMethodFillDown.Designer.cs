﻿using System.ComponentModel;
using System.Windows.Forms;

namespace LcmsNet.SampleQueue.Forms
{
    partial class formMethodFillDown
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMethodFillDown));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.comboLcMethodCol2 = new System.Windows.Forms.ComboBox();
            this.mbutton_fillDatasetType = new System.Windows.Forms.Button();
            this.mbutton_fillVolume = new System.Windows.Forms.Button();
            this.comboLcMethodCol3 = new System.Windows.Forms.ComboBox();
            this.comboLcMethodCol4 = new System.Windows.Forms.ComboBox();
            this.mbutton_fillInstrument = new System.Windows.Forms.Button();
            this.comboInstMethodCol1 = new System.Windows.Forms.ComboBox();
            this.mbutton_fillLCMethod = new System.Windows.Forms.Button();
            this.comboInstMethodCol2 = new System.Windows.Forms.ComboBox();
            this.comboInstMethodCol3 = new System.Windows.Forms.ComboBox();
            this.comboInstMethodCol4 = new System.Windows.Forms.ComboBox();
            this.upDownVolCol1 = new System.Windows.Forms.NumericUpDown();
            this.upDownVolCol2 = new System.Windows.Forms.NumericUpDown();
            this.upDownVolCol3 = new System.Windows.Forms.NumericUpDown();
            this.upDownVolCol4 = new System.Windows.Forms.NumericUpDown();
            this.comboLcMethodCol1 = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboDatasetTypeCol1 = new System.Windows.Forms.ComboBox();
            this.comboDatasetTypeCol2 = new System.Windows.Forms.ComboBox();
            this.comboDatasetTypeCol3 = new System.Windows.Forms.ComboBox();
            this.comboDatasetTypeCol4 = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.mbutton_cancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownVolCol1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownVolCol2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownVolCol3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownVolCol4)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.Controls.Add(this.label6, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboLcMethodCol2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.mbutton_fillDatasetType, 4, 5);
            this.tableLayoutPanel1.Controls.Add(this.mbutton_fillVolume, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.comboLcMethodCol3, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.comboLcMethodCol4, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.mbutton_fillInstrument, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.comboInstMethodCol1, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.mbutton_fillLCMethod, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.comboInstMethodCol2, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.comboInstMethodCol3, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.comboInstMethodCol4, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.upDownVolCol1, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.upDownVolCol2, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.upDownVolCol3, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.upDownVolCol4, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.comboLcMethodCol1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label10, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboDatasetTypeCol1, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboDatasetTypeCol2, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.comboDatasetTypeCol3, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.comboDatasetTypeCol4, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBox1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBox2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBox3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBox4, 0, 4);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 4);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1125, 235);
            this.tableLayoutPanel1.TabIndex = 100;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(81, 2);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(375, 31);
            this.label6.TabIndex = 27;
            this.label6.Text = "LC Method";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboLcMethodCol2
            // 
            this.comboLcMethodCol2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLcMethodCol2.FormattingEnabled = true;
            this.comboLcMethodCol2.Location = new System.Drawing.Point(81, 73);
            this.comboLcMethodCol2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboLcMethodCol2.MaxDropDownItems = 100;
            this.comboLcMethodCol2.Name = "comboLcMethodCol2";
            this.comboLcMethodCol2.Size = new System.Drawing.Size(371, 24);
            this.comboLcMethodCol2.TabIndex = 5;
            // 
            // mbutton_fillDatasetType
            // 
            this.mbutton_fillDatasetType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mbutton_fillDatasetType.Image = ((System.Drawing.Image)(resources.GetObject("mbutton_fillDatasetType.Image")));
            this.mbutton_fillDatasetType.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mbutton_fillDatasetType.Location = new System.Drawing.Point(873, 175);
            this.mbutton_fillDatasetType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mbutton_fillDatasetType.Name = "mbutton_fillDatasetType";
            this.mbutton_fillDatasetType.Size = new System.Drawing.Size(236, 54);
            this.mbutton_fillDatasetType.TabIndex = 26;
            this.mbutton_fillDatasetType.Text = "Apply";
            this.mbutton_fillDatasetType.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.mbutton_fillDatasetType.UseVisualStyleBackColor = true;
            this.mbutton_fillDatasetType.Click += new System.EventHandler(this.mbutton_fillDatasetType_Click);
            // 
            // mbutton_fillVolume
            // 
            this.mbutton_fillVolume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mbutton_fillVolume.Image = ((System.Drawing.Image)(resources.GetObject("mbutton_fillVolume.Image")));
            this.mbutton_fillVolume.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mbutton_fillVolume.Location = new System.Drawing.Point(775, 175);
            this.mbutton_fillVolume.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mbutton_fillVolume.Name = "mbutton_fillVolume";
            this.mbutton_fillVolume.Size = new System.Drawing.Size(88, 54);
            this.mbutton_fillVolume.TabIndex = 25;
            this.mbutton_fillVolume.Text = "Apply";
            this.mbutton_fillVolume.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.mbutton_fillVolume.UseVisualStyleBackColor = true;
            this.mbutton_fillVolume.Click += new System.EventHandler(this.mbutton_fillVolume_Click);
            // 
            // comboLcMethodCol3
            // 
            this.comboLcMethodCol3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLcMethodCol3.FormattingEnabled = true;
            this.comboLcMethodCol3.Location = new System.Drawing.Point(81, 107);
            this.comboLcMethodCol3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboLcMethodCol3.MaxDropDownItems = 100;
            this.comboLcMethodCol3.Name = "comboLcMethodCol3";
            this.comboLcMethodCol3.Size = new System.Drawing.Size(371, 24);
            this.comboLcMethodCol3.TabIndex = 6;
            // 
            // comboLcMethodCol4
            // 
            this.comboLcMethodCol4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLcMethodCol4.FormattingEnabled = true;
            this.comboLcMethodCol4.Location = new System.Drawing.Point(81, 141);
            this.comboLcMethodCol4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboLcMethodCol4.MaxDropDownItems = 100;
            this.comboLcMethodCol4.Name = "comboLcMethodCol4";
            this.comboLcMethodCol4.Size = new System.Drawing.Size(371, 24);
            this.comboLcMethodCol4.TabIndex = 7;
            // 
            // mbutton_fillInstrument
            // 
            this.mbutton_fillInstrument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mbutton_fillInstrument.Image = ((System.Drawing.Image)(resources.GetObject("mbutton_fillInstrument.Image")));
            this.mbutton_fillInstrument.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mbutton_fillInstrument.Location = new System.Drawing.Point(466, 175);
            this.mbutton_fillInstrument.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mbutton_fillInstrument.Name = "mbutton_fillInstrument";
            this.mbutton_fillInstrument.Size = new System.Drawing.Size(299, 54);
            this.mbutton_fillInstrument.TabIndex = 24;
            this.mbutton_fillInstrument.Text = "Apply";
            this.mbutton_fillInstrument.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.mbutton_fillInstrument.UseVisualStyleBackColor = true;
            this.mbutton_fillInstrument.Click += new System.EventHandler(this.mbutton_fillInstrument_Click);
            // 
            // comboInstMethodCol1
            // 
            this.comboInstMethodCol1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInstMethodCol1.FormattingEnabled = true;
            this.comboInstMethodCol1.Location = new System.Drawing.Point(466, 39);
            this.comboInstMethodCol1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboInstMethodCol1.MaxDropDownItems = 100;
            this.comboInstMethodCol1.Name = "comboInstMethodCol1";
            this.comboInstMethodCol1.Size = new System.Drawing.Size(297, 24);
            this.comboInstMethodCol1.TabIndex = 9;
            // 
            // mbutton_fillLCMethod
            // 
            this.mbutton_fillLCMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mbutton_fillLCMethod.Image = ((System.Drawing.Image)(resources.GetObject("mbutton_fillLCMethod.Image")));
            this.mbutton_fillLCMethod.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mbutton_fillLCMethod.Location = new System.Drawing.Point(81, 175);
            this.mbutton_fillLCMethod.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mbutton_fillLCMethod.Name = "mbutton_fillLCMethod";
            this.mbutton_fillLCMethod.Size = new System.Drawing.Size(375, 54);
            this.mbutton_fillLCMethod.TabIndex = 23;
            this.mbutton_fillLCMethod.Text = "Apply";
            this.mbutton_fillLCMethod.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.mbutton_fillLCMethod.UseVisualStyleBackColor = true;
            this.mbutton_fillLCMethod.Click += new System.EventHandler(this.mbutton_fillLCMethod_Click);
            // 
            // comboInstMethodCol2
            // 
            this.comboInstMethodCol2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInstMethodCol2.FormattingEnabled = true;
            this.comboInstMethodCol2.Location = new System.Drawing.Point(466, 73);
            this.comboInstMethodCol2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboInstMethodCol2.MaxDropDownItems = 100;
            this.comboInstMethodCol2.Name = "comboInstMethodCol2";
            this.comboInstMethodCol2.Size = new System.Drawing.Size(297, 24);
            this.comboInstMethodCol2.TabIndex = 10;
            // 
            // comboInstMethodCol3
            // 
            this.comboInstMethodCol3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInstMethodCol3.FormattingEnabled = true;
            this.comboInstMethodCol3.Location = new System.Drawing.Point(466, 107);
            this.comboInstMethodCol3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboInstMethodCol3.MaxDropDownItems = 100;
            this.comboInstMethodCol3.Name = "comboInstMethodCol3";
            this.comboInstMethodCol3.Size = new System.Drawing.Size(297, 24);
            this.comboInstMethodCol3.TabIndex = 11;
            // 
            // comboInstMethodCol4
            // 
            this.comboInstMethodCol4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInstMethodCol4.FormattingEnabled = true;
            this.comboInstMethodCol4.Location = new System.Drawing.Point(466, 141);
            this.comboInstMethodCol4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboInstMethodCol4.MaxDropDownItems = 100;
            this.comboInstMethodCol4.Name = "comboInstMethodCol4";
            this.comboInstMethodCol4.Size = new System.Drawing.Size(297, 24);
            this.comboInstMethodCol4.TabIndex = 12;
            // 
            // upDownVolCol1
            // 
            this.upDownVolCol1.DecimalPlaces = 1;
            this.upDownVolCol1.Location = new System.Drawing.Point(775, 39);
            this.upDownVolCol1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.upDownVolCol1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.upDownVolCol1.Name = "upDownVolCol1";
            this.upDownVolCol1.Size = new System.Drawing.Size(88, 22);
            this.upDownVolCol1.TabIndex = 14;
            this.upDownVolCol1.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // upDownVolCol2
            // 
            this.upDownVolCol2.DecimalPlaces = 1;
            this.upDownVolCol2.Location = new System.Drawing.Point(775, 73);
            this.upDownVolCol2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.upDownVolCol2.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.upDownVolCol2.Name = "upDownVolCol2";
            this.upDownVolCol2.Size = new System.Drawing.Size(88, 22);
            this.upDownVolCol2.TabIndex = 15;
            this.upDownVolCol2.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // upDownVolCol3
            // 
            this.upDownVolCol3.DecimalPlaces = 1;
            this.upDownVolCol3.Location = new System.Drawing.Point(775, 107);
            this.upDownVolCol3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.upDownVolCol3.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.upDownVolCol3.Name = "upDownVolCol3";
            this.upDownVolCol3.Size = new System.Drawing.Size(88, 22);
            this.upDownVolCol3.TabIndex = 16;
            this.upDownVolCol3.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // upDownVolCol4
            // 
            this.upDownVolCol4.DecimalPlaces = 1;
            this.upDownVolCol4.Location = new System.Drawing.Point(775, 141);
            this.upDownVolCol4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.upDownVolCol4.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.upDownVolCol4.Name = "upDownVolCol4";
            this.upDownVolCol4.Size = new System.Drawing.Size(88, 22);
            this.upDownVolCol4.TabIndex = 17;
            this.upDownVolCol4.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // comboLcMethodCol1
            // 
            this.comboLcMethodCol1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLcMethodCol1.FormattingEnabled = true;
            this.comboLcMethodCol1.Location = new System.Drawing.Point(81, 39);
            this.comboLcMethodCol1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboLcMethodCol1.MaxDropDownItems = 100;
            this.comboLcMethodCol1.Name = "comboLcMethodCol1";
            this.comboLcMethodCol1.Size = new System.Drawing.Size(371, 24);
            this.comboLcMethodCol1.TabIndex = 4;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(466, 2);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(299, 31);
            this.label9.TabIndex = 28;
            this.label9.Text = "Instrument Acquisition Method";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(775, 2);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 31);
            this.label10.TabIndex = 29;
            this.label10.Text = "Volume";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(873, 2);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(252, 31);
            this.label1.TabIndex = 30;
            this.label1.Text = "DatesetType";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboDatasetTypeCol1
            // 
            this.comboDatasetTypeCol1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDatasetTypeCol1.FormattingEnabled = true;
            this.comboDatasetTypeCol1.Location = new System.Drawing.Point(873, 39);
            this.comboDatasetTypeCol1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboDatasetTypeCol1.Name = "comboDatasetTypeCol1";
            this.comboDatasetTypeCol1.Size = new System.Drawing.Size(235, 24);
            this.comboDatasetTypeCol1.TabIndex = 19;
            // 
            // comboDatasetTypeCol2
            // 
            this.comboDatasetTypeCol2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDatasetTypeCol2.FormattingEnabled = true;
            this.comboDatasetTypeCol2.Location = new System.Drawing.Point(873, 73);
            this.comboDatasetTypeCol2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboDatasetTypeCol2.Name = "comboDatasetTypeCol2";
            this.comboDatasetTypeCol2.Size = new System.Drawing.Size(235, 24);
            this.comboDatasetTypeCol2.TabIndex = 20;
            // 
            // comboDatasetTypeCol3
            // 
            this.comboDatasetTypeCol3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDatasetTypeCol3.FormattingEnabled = true;
            this.comboDatasetTypeCol3.Location = new System.Drawing.Point(873, 107);
            this.comboDatasetTypeCol3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboDatasetTypeCol3.Name = "comboDatasetTypeCol3";
            this.comboDatasetTypeCol3.Size = new System.Drawing.Size(235, 24);
            this.comboDatasetTypeCol3.TabIndex = 21;
            // 
            // comboDatasetTypeCol4
            // 
            this.comboDatasetTypeCol4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDatasetTypeCol4.FormattingEnabled = true;
            this.comboDatasetTypeCol4.Location = new System.Drawing.Point(873, 141);
            this.comboDatasetTypeCol4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboDatasetTypeCol4.Name = "comboDatasetTypeCol4";
            this.comboDatasetTypeCol4.Size = new System.Drawing.Size(235, 24);
            this.comboDatasetTypeCol4.TabIndex = 22;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(6, 39);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(65, 21);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "Apply";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(6, 73);
            this.checkBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(65, 21);
            this.checkBox2.TabIndex = 1;
            this.checkBox2.Text = "Apply";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(6, 107);
            this.checkBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(65, 21);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.Text = "Apply";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(6, 141);
            this.checkBox4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(65, 21);
            this.checkBox4.TabIndex = 3;
            this.checkBox4.Text = "Apply";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(880, 251);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(113, 57);
            this.buttonOk.TabIndex = 28;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button5.Image = global::LcmsNet.Properties.Resources.FillDownAll;
            this.button5.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button5.Location = new System.Drawing.Point(96, 242);
            this.button5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(183, 65);
            this.button5.TabIndex = 27;
            this.button5.Text = "Apply All";
            this.button5.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // mbutton_cancel
            // 
            this.mbutton_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mbutton_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mbutton_cancel.Location = new System.Drawing.Point(1003, 251);
            this.mbutton_cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mbutton_cancel.Name = "mbutton_cancel";
            this.mbutton_cancel.Size = new System.Drawing.Size(113, 57);
            this.mbutton_cancel.TabIndex = 29;
            this.mbutton_cancel.Text = "Cancel";
            this.mbutton_cancel.UseVisualStyleBackColor = true;
            this.mbutton_cancel.Click += new System.EventHandler(this.mbutton_cancel_Click);
            // 
            // formMethodFillDown
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1132, 322);
            this.ControlBox = false;
            this.Controls.Add(this.mbutton_cancel);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "formMethodFillDown";
            this.Text = "Method(s) Filldown";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownVolCol1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownVolCol2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownVolCol3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownVolCol4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label6;
        private ComboBox comboLcMethodCol3;
        private ComboBox comboLcMethodCol2;
        private ComboBox comboLcMethodCol1;
        private ComboBox comboLcMethodCol4;
        private ComboBox comboInstMethodCol1;
        private ComboBox comboInstMethodCol2;
        private ComboBox comboInstMethodCol3;
        private ComboBox comboInstMethodCol4;
        private NumericUpDown upDownVolCol1;
        private NumericUpDown upDownVolCol2;
        private NumericUpDown upDownVolCol3;
        private NumericUpDown upDownVolCol4;
        private Label label9;
        private Label label10;
        private Button buttonOk;
        private Label label1;
        private ComboBox comboDatasetTypeCol1;
        private ComboBox comboDatasetTypeCol2;
        private ComboBox comboDatasetTypeCol3;
        private ComboBox comboDatasetTypeCol4;
        private Button mbutton_fillVolume;
        private Button mbutton_fillDatasetType;
        private Button mbutton_fillInstrument;
        private Button mbutton_fillLCMethod;
        private Button button5;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private CheckBox checkBox4;
        private Button mbutton_cancel;
    }
}