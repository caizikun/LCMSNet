﻿
//*********************************************************************************************************
// Written by Dave Clark, Brian LaMarche for the US Department of Energy 
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2010, Battelle Memorial Institute
// Created 02/23/2010
//
// Last modified 02/23/2010
//*********************************************************************************************************
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Method;
using LcmsNetSQLiteTools;
using LcmsNet.Configuration;
using LcmsNet.Method;
namespace LcmsNet.SampleQueue.Forms
{
    /// <summary>
    /// Form for filling in LC Method, Acquisition Method, and vial volume for a group of samples
    /// </summary>
	public partial class formMethodFillDown : Form
	{
		#region "Class variables"
        List<classSampleData> mobject_Samples;
        Dictionary<string, classLCMethod> mdict_methodNameMap;
        /// <summary>
        /// Reverse lookup for methods.
        /// </summary>
        Dictionary<classLCMethod, string> mdict_methods;
        private classLCMethodManager m_manager;
		#endregion

        #region "Constructors"
        public formMethodFillDown()
        {
            InitializeComponent();
        }
        public formMethodFillDown(classLCMethodManager manager, List<string> instrumentMethods)
        {
            InitializeComponent();

            mdict_methodNameMap = new Dictionary<string, classLCMethod>();
            mdict_methods       = new Dictionary<classLCMethod,string>();
            m_manager           = manager;

            // Clear the dropdowns
            ClearDropDowns();

            // Fill the instrument method dropdowns
            foreach (string tmpStr in instrumentMethods)
            {
                comboInstMethodCol1.Items.Add(tmpStr);
                comboInstMethodCol2.Items.Add(tmpStr);
                comboInstMethodCol3.Items.Add(tmpStr);
                comboInstMethodCol4.Items.Add(tmpStr);
            }
            // Fill the dataset type dropdowns
            List<string> datesetTypeList = classSQLiteTools.GetDatasetTypeList(false);
            foreach (string tmpStr in datesetTypeList)
            {
                comboDatasetTypeCol1.Items.Add(tmpStr);
                comboDatasetTypeCol2.Items.Add(tmpStr);
                comboDatasetTypeCol3.Items.Add(tmpStr);
                comboDatasetTypeCol4.Items.Add(tmpStr);
            }
            foreach (classLCMethod method in m_manager.Methods.Values)
            {
                AddMethod(method);
            }
            // Set initial dropdown values
            SetInitialDropdownValue(this.tableLayoutPanel1.Controls);
            manager.MethodAdded   += new DelegateMethodUpdated(manager_MethodAdded);
            manager.MethodRemoved += new DelegateMethodUpdated(manager_MethodRemoved);
            manager.MethodUpdated += new DelegateMethodUpdated(manager_MethodUpdated);
        }

        bool manager_MethodUpdated(object sender, classLCMethod method)
        {
            UpdateMethod(method);
            return true;
        }        
        bool manager_MethodRemoved(object sender, classLCMethod method)
        {
            RemoveMethod(method);
            return true;
        }
        bool manager_MethodAdded(object sender, classLCMethod method)
        {
            AddMethod(method);
            return true;
        }

        private void UpdateMethod(classLCMethod method)
        {
            string oldName = null;
            bool contains  = mdict_methods.ContainsKey(method);
            if (contains)
            {
                mdict_methodNameMap.Remove(oldName);
                mdict_methodNameMap.Add(method.Name, method);

                comboLcMethodCol1.Items.Remove(oldName);
                comboLcMethodCol2.Items.Remove(oldName);
                comboLcMethodCol3.Items.Remove(oldName);
                comboLcMethodCol4.Items.Remove(oldName);

                mdict_methods[method] = method.Name;
                comboLcMethodCol1.Items.Add(method.Name);
                comboLcMethodCol2.Items.Add(method.Name);
                comboLcMethodCol3.Items.Add(method.Name);
                comboLcMethodCol4.Items.Add(method.Name);                 
            }
        }
        private void RemoveMethod(classLCMethod method)
        {
            if (mdict_methodNameMap.ContainsKey(method.Name))
            {
                mdict_methodNameMap.Remove(method.Name);
                mdict_methods.Remove(method);
                comboLcMethodCol1.Items.Remove(method.Name);
                comboLcMethodCol2.Items.Remove(method.Name);
                comboLcMethodCol3.Items.Remove(method.Name);
                comboLcMethodCol4.Items.Remove(method.Name);
            }
        }
        private void AddMethod(classLCMethod method)
        {
            if (mdict_methodNameMap.ContainsKey(method.Name))
            {
                mdict_methodNameMap[method.Name]    = method;
                mdict_methods[method]               = method.Name;
            }
            else
            {
                comboLcMethodCol1.Items.Add(method.Name);
                comboLcMethodCol2.Items.Add(method.Name);
                comboLcMethodCol3.Items.Add(method.Name);
                comboLcMethodCol4.Items.Add(method.Name);
                mdict_methodNameMap.Add(method.Name, method); 
                mdict_methods.Add(method, method.Name);
            }
        }
		#endregion

		#region "Methods"
			/// <summary>
			/// Initializes the form
			/// </summary>
			/// <param name="samples">List of samples to process</param>
			/// <param name="lcMethods">Available LC methods</param>
			/// <param name="instMethods">List of available instrument methods</param>
			public void InitForm(List<classSampleData> samples)
			{
				// Store the sample data for later use
				mobject_Samples = samples;
			}	

			/// <summary>
			/// Sets all dropdowns to first item in list
			/// </summary>
			private void SetInitialDropdownValue(Control.ControlCollection collection)
			{
				foreach (Control testControl in collection)
				{
					if (testControl.GetType() == typeof(ComboBox))
					{
						ComboBox tempControl = (ComboBox)testControl;
                        if (tempControl.Items.Count > 0)
						    tempControl.SelectedIndex = 0;
					}
				}
			}
            private void EnsureItemsAreSelected()
            {
                foreach (Control testControl in this.tableLayoutPanel1.Controls)
                {
                    if (testControl.GetType() == typeof(ComboBox))
                    {
                        ComboBox tempControl = (ComboBox)testControl;
                        if (tempControl.Items.Count > 0 && tempControl.SelectedIndex < 0)
                            tempControl.SelectedIndex = 0;
                    }
                }
            }

			/// <summary>
			/// Clears all the method dropdowns prior to reloading them
			/// </summary>
			private void ClearDropDowns()
			{
				foreach (Control testControl in tableLayoutPanel1.Controls)
				{
					if (testControl.GetType() == typeof(ComboBox))
					{
						ComboBox tempControl = (ComboBox)testControl;
						tempControl.Items.Clear();
					}
				}
			}	
        
			/// <summary>
			/// Updates the sample list per the operator's selections and returns the list
			/// </summary>
			/// <returns>Sample list modified with methods and vial volumes</returns>
			public List<classSampleData> GetModifiedSampleList()
			{
				// Return and exit
				return mobject_Samples;
			}

		#endregion

		#region "Event handlers"
			private void buttonOk_Click(object sender, EventArgs e)
			{				
				this.DialogResult = DialogResult.OK;
			}
		#endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            EnsureItemsAreSelected();
            List<classLCMethod> methods = new List<classLCMethod>();
            if (checkBox1.Checked)
            {
                string name = comboLcMethodCol1.Text;
                if (mdict_methodNameMap.ContainsKey(name))
                {
                    methods.Add(mdict_methodNameMap[name]);
                }
            }
            if (checkBox2.Checked)
            {
                string name = comboLcMethodCol2.Text;
                if (mdict_methodNameMap.ContainsKey(name))
                {
                    methods.Add(mdict_methodNameMap[name]);
                }
            }
            if (checkBox3.Checked)
            {
                string name = comboLcMethodCol3.Text;
                if (mdict_methodNameMap.ContainsKey(name))
                {
                    methods.Add(mdict_methodNameMap[name]);
                }
            }
            if (checkBox4.Checked)
            {
                string name = comboLcMethodCol4.Text;
                if (mdict_methodNameMap.ContainsKey(name))
                {
                    methods.Add(mdict_methodNameMap[name]);
                }
            }
            if (methods.Count < 1)
                return;
            
            int i = 0;
            foreach (classSampleData samples in mobject_Samples)
            {
                classLCMethod tempMethod = methods[i];

                if (tempMethod.Column != samples.ColumnData.ID)
                {
                    if (tempMethod.Column >= 0)
                    {
                        samples.ColumnData = classCartConfiguration.Columns[tempMethod.Column];
                    }
                }
                samples.LCMethod = tempMethod;

                i++;
                // mod?
                if (i >= methods.Count)
                    i = 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mbutton_fillInstrument_Click(object sender, EventArgs e)
        {
            if (comboInstMethodCol1.Items.Count < 1)
                return;

            EnsureItemsAreSelected();

            

            List<string> methods = new List<string>();
            if (checkBox1.Checked)
            {
                string name = comboInstMethodCol1.Text;
                methods.Add(name);
            }
            if (checkBox2.Checked)
            {
                string name = comboInstMethodCol2.Text;
                methods.Add(name);
            }
            if (checkBox3.Checked)
            {
                string name = comboInstMethodCol3.Text;
                methods.Add(name);
            }
            if (checkBox4.Checked)
            {
                string name = comboInstMethodCol4.Text;
                methods.Add(name);
            }
            if (methods.Count < 1)
                return;

            int i = 0;
            foreach (classSampleData sample in mobject_Samples)
            {
                sample.InstrumentData = new classInstrumentInfo();
                sample.InstrumentData.MethodName = methods[i];
                i++;
                // mod?
                if (i >= methods.Count)
                    i = 0;
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mbutton_fillVolume_Click(object sender, EventArgs e)
        {

            EnsureItemsAreSelected();
            try
            {
                List<double> methods = new List<double>();
                if (checkBox1.Checked)
                {
                    double value = (double)upDownVolCol1.Value;
                    methods.Add(value);
                }
                if (checkBox2.Checked)
                {
                    double value = (double)upDownVolCol2.Value;
                    methods.Add(value);
                }
                if (checkBox3.Checked)
                {
                    double value = (double)upDownVolCol3.Value;
                    methods.Add(value);
                }
                if (checkBox4.Checked)
                {
                    double value = (double)upDownVolCol4.Value;
                    methods.Add(value);
                }
                if (methods.Count < 1)
                    return;

                int i = 0;
                foreach (classSampleData sample in mobject_Samples)
                {
                    sample.Volume = methods[i];
                    i++;
                    // mod?
                    if (i >= methods.Count)
                        i = 0;
                }
            }
            catch(KeyNotFoundException)
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mbutton_fillDatasetType_Click(object sender, EventArgs e)
        {

            EnsureItemsAreSelected(); 
            List<string> methods = new List<string>();
            if (checkBox1.Checked)
            {
                string value = comboDatasetTypeCol1.Text;
                methods.Add(value);
            }
            if (checkBox2.Checked)
            {
                string value = comboDatasetTypeCol2.Text;
                methods.Add(value);
            }
            if (checkBox3.Checked)
            {
                string value = comboDatasetTypeCol3.Text;
                methods.Add(value);
            }
            if (checkBox4.Checked)
            {
                string value = comboDatasetTypeCol4.Text;
                methods.Add(value);
            }
            if (methods.Count < 1)
                return;

            int i = 0;
            foreach (classSampleData sample in mobject_Samples)
            {
                sample.DmsData.DatasetType = methods[i];
                i++;
                // mod?
                if (i >= methods.Count)
                    i = 0;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            // This is lazy programming....
            EnsureItemsAreSelected(); 
            button1_Click(null, null);
            mbutton_fillInstrument_Click(null, null);        
            mbutton_fillVolume_Click(null, null);        
            mbutton_fillDatasetType_Click(null, null);                
        }

        private void mbutton_cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
	}	
}	
