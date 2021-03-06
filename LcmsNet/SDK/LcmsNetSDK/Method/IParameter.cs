﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LcmsNet.Method
{
    /// <summary>
    /// Abstract Control class that allows the user to define how the value is grabbed
    /// from the user interface for setting a parameter value in the method editor.
    /// </summary>
    public interface ILCEventParameter
    {
        /// <summary>
        /// Gets the value set by the user.
        /// </summary>
        object ParameterValue { get; set; }

        /// <summary>
        /// Fired when a parameter changes.
        /// </summary>
        event EventHandler EventChanged;

        //bool IsTime { get; set; }
    }

    /// <summary>
    /// Text control for string user input when defining a method.
    /// </summary>
    public class controlParameterTextBox : TextBox, ILCEventParameter
    {
        #region IParameterBase Members

        /// <summary>
        /// Gets the text of the control.
        /// </summary>
        public object ParameterValue
        {
            get { return Text; }
            set
            {
                if (value != null && value.ToString() == Text)
                    return;

                if (value == null)
                    Text = "";
                else
                    Text = value.ToString();

                OnEventChanged();
            }
        }

        #endregion

        public event EventHandler EventChanged;

        /// <summary>
        /// Fires the event changed event.
        /// </summary>
        private void OnEventChanged()
        {
            EventChanged?.Invoke(this, null);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            OnEventChanged();
        }
    }

    /// <summary>
    /// Combo box control for enumerated user input when defining a method.
    /// </summary>
    public class controlParameterComboBox : ComboBox, ILCEventParameter
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public controlParameterComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
        }


        public event EventHandler EventChanged;

        /// <summary>
        /// Fires the event changed event.
        /// </summary>
        private void OnEventChanged()
        {
            EventChanged?.Invoke(this, null);
        }

        #region IParameterBase Members

        /// <summary>
        /// Gets the text of the control.
        /// </summary>
        public object ParameterValue
        {
            get { return SelectedItem; }
            set
            {
                if (value == null)
                    return;

                if (Items.Contains(value) == false)
                    Items.Add(value);

                SelectedItem = value;
            }
        }

        /// <summary>
        /// Event method to storing objects in the list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public void FillData(object sender, List<object> data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, List<object>>(FillData), sender, data);
            }
            else
            {
                var value = ParameterValue;

                Items.Clear();
                if (data == null || data.Count < 1)
                    return;

                if (data.Count > 0)
                {
                    var tempData = new object[data.Count];
                    data.CopyTo(tempData, 0);
                    Items.AddRange(tempData);

                    var index = -1;

                    if (value != null)
                        index = Items.IndexOf(value);

                    if (index >= 0)
                    {
                        ParameterValue = Items[index];
                    }
                    else
                    {
                        ParameterValue = Items[0];
                    }
                }
                Sorted = true;
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            OnEventChanged();
        }

        #endregion
    }
}