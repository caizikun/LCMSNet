﻿using System;
using System.Collections.Generic;
using LcmsNetDataClasses.Logging;
using LcmsNetDataClasses.Devices;

//using DirectControl;

namespace Eksigent.Devices.Pumps
{
    public partial class EksigentPumpControl : controlBaseDeviceControl, IDeviceControl
    {
        /// <summary>
        /// Notification driver object.
        /// </summary>
        private EksigentPump m_pump;
        /// <summary>
        /// Delegate for updating text on the form from another thread.
        /// </summary>
        private readonly EventHandler<classDeviceStatusEventArgs> m_statusUpdateDelegate;

        public EksigentPumpControl()
        {
            InitializeComponent();

            m_statusUpdateDelegate = UpdateStatusLabelText;
        }
        
        public void RegisterDevice(IDevice device)
        {
            m_pump                   = device as EksigentPump;
            if (m_pump != null)
            {
                m_pump.Error             += m_pump_Error;
                m_pump.StatusUpdate      += m_pump_StatusUpdate;
                m_pump.RequiresOCXInitialization += m_pump_RequiresOCXInitialization;
                m_pump.PumpStatus        += m_pump_PumpStatus;
                m_pump.MethodNames       += m_pump_MethodNames;
                m_pump.ChannelNumbers    += m_pump_ChannelNumbers;
            }
            SetBaseDevice(device);
        }

        void m_pump_RequiresOCXInitialization(object sender, EventArgs e)
        {
            try
            {
                
            }
            catch(Exception ex)
            {
                classApplicationLogger.LogError(0, "Could not register the Eksigent control software OCX. " + ex.Message, ex);
            }
        }

        #region IDeviceControl Members
       
        public IDevice Device
        {
            get
            {
                return m_pump;
            }
            set
            {
                RegisterDevice(value);
            }
        }
        #endregion
               
        #region Pump Event Handlers
        /// <summary>
        /// Handles the channel numbers.
        /// </summary>
        /// <param name="totalChannels"></param>
        void m_pump_ChannelNumbers(int totalChannels)
        {
            var value = Convert.ToInt32(mnum_channels.Value);

            if (value > totalChannels)
            {
                mnum_channels.Value = totalChannels;
            }

            mnum_channels.Minimum = Math.Min(1, totalChannels);
            mnum_channels.Maximum = totalChannels;
        }
        void UpdateStatusLabelText(object sender, classDeviceStatusEventArgs e)
        {
            mlabel_pumpStatus.Text = e.Message;
        }
        /// <summary>
        /// Handles pump status changes, not just status of the object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_pump_PumpStatus(object sender, classDeviceStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(m_statusUpdateDelegate, sender, e);
            }
            else
            {
                UpdateStatusLabelText(sender, e);
            }
        }
        /// <summary>
        /// Updates the list box with the appropiate method names.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        void m_pump_MethodNames(object sender, List<object> data)
        {
            mcomboBox_methods.BeginUpdate();
            mcomboBox_methods.Items.Clear();
            foreach (var datum in data)
            {
                mcomboBox_methods.Items.Add(datum);
            }
            mcomboBox_methods.EndUpdate();

            if (mcomboBox_methods.Items.Count > 0)
            {
                mcomboBox_methods.SelectedIndex = 0;
            }
        }
        void m_pump_StatusUpdate(object sender, classDeviceStatusEventArgs e)
        {
            UpdateStatusDisplay(e.Message);
        }
        void m_pump_Error(object sender, classDeviceErrorEventArgs e)
        {
            UpdateStatusDisplay(e.Error);
        }
        #endregion

        #region Button Event Handlers
        private void mbutton_updateMethods_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            m_pump.GetMethods();
        }
        private void mbutton_methodMenu_Click(object sender, EventArgs e)
        {

            UpdateStatusDisplay("");
            if (mnum_channels.Value < 1)
            {
                UpdateStatusDisplay("Set the channel first other than zero.");
                return;
            }
            var methodData = mcomboBox_methods.SelectedItem;
            if (methodData == null)
            {
                UpdateStatusDisplay("Select a method first.");
                return;
            }
            m_pump.ShowMethodMenu(Convert.ToInt32(mnum_channels.Value), methodData.ToString());
        }
        private void mbutton_DirectControl_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            if (mnum_channels.Value < 1)
            {
                UpdateStatusDisplay("Set the channel first other than zero.");
                return;
            }
            try
            {
                m_pump.ShowDirectControl(Convert.ToInt32(mnum_channels.Value), Handle.ToInt32());
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }
        }
        private void mbutton_mobilePhase_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            if (mnum_channels.Value < 1)
            {
                UpdateStatusDisplay("Set the channel first other than zero.");
                return;
            }
            
            try
            {
                m_pump.ShowMobilePhaseMenu(Convert.ToInt32(mnum_channels.Value), Handle.ToInt32());
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }
        }
        private void mbutton_instrumentConfig_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            
            try
            {
                m_pump.ShowInstrumentConfigMenu();
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }
        }
        private void mbutton_advancedSettings_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            
            try
            {
                m_pump.ShowAdvancedSettings(Handle.ToInt32());
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }
        }
        private void mbutton_diagnosticsMenu_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            if (mnum_channels.Value < 1)
            {
                UpdateStatusDisplay("Set the channel first other than zero.");
                return;
            }

            try
            {
                m_pump.ShowDiagnosticsMenu(Convert.ToInt32(mnum_channels.Value), Handle.ToInt32());
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }
        }
        private void mbutton_mainWindow_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            
            try
            {
                m_pump.ShowMainWindow(Handle.ToInt32());
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }
        }
        private void mbutton_AlertsMenu_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            
            try
            {
                m_pump.ShowAlertsMenu();
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }
        }
        private void mbutton_start_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");

            if (mnum_channels.Value < 1)
            {
                UpdateStatusDisplay("Set the channel first other than zero.");
                return;
            }
            try
            {
                var methodData = mcomboBox_methods.SelectedItem;
                if (methodData == null)
                {
                    UpdateStatusDisplay("Select a method first.");
                    return;
                }
                m_pump.StartMethod(0, Convert.ToDouble(mnum_channels.Value), methodData.ToString());
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }

        }
        private void mbutton_stop_Click(object sender, EventArgs e)
        {
            UpdateStatusDisplay("");
            if (mnum_channels.Value < 1)
            {
                UpdateStatusDisplay("Set the channel first other than zero.");
                return;
            }

            try
            {
                m_pump.StopMethod(0, Convert.ToInt32(mnum_channels.Value));
            }
            catch (Exception ex)
            {
                classApplicationLogger.LogError(0, ex.Message);
            }
        }
        #endregion
    }
}
