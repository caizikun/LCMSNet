﻿//*********************************************************************************************************
// Written by John Ryan, Dave Clark, Brian LaMarche for the US Department of Energy
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2009, Battelle Memorial Institute
// Created 08/17/2009
//
// Updates
// - 12/01/09 (DAC) - Modified to accomodate change of vial from string to int
//*********************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using LcmsNetDataClasses.Devices;

namespace LcmsNet.Devices.Pal
{    
    public partial class controlPal : controlBaseDeviceControl, IDeviceControl
    {
        #region Members

        /// <summary>
        /// The class which controls the PAL itself.
        /// </summary>
        private classPal m_Pal;

        /// <summary>
        /// An event listener to watch for events from the PAL class
        /// </summary>
        //public PalEventListener m_PalEventListener;

        #endregion

        #region Constructors

        /// <summary>
        /// The main constructor. Creates the PAL class and initializes the port.
        /// </summary>
        public controlPal()
        {
            InitializeComponent();
            var portNames = System.IO.Ports.SerialPort.GetPortNames();
            foreach (var portName in portNames)
            {
                mcombo_portNames.Items.Add(portName);
            }
            
            mcombo_portNames.SelectedIndexChanged += mcombo_portNames_SelectedIndexChanged;
        }

        void mcombo_portNames_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// Registers the device events and user interface.
        /// </summary>
        /// <param name="device"></param>
        private void RegisterDevice(IDevice device)
        {
          
            m_Pal = device as classPal;

            if (m_Pal != null)
            {
                m_Pal.DeviceSaveRequired += Pal_DeviceSaveRequired;
                m_Pal.Free               += OnFree;
                m_Pal.TrayNames          += m_Pal_PalTrayListReceived;
                m_Pal.MethodNames        += (sender, e) => {ProcessMethods(e.MethodList); };
     
                mcomboBox_VialRange.DataSource      = Enum.GetNames(typeof(enumVialRanges));
                mcomboBox_VialRange.SelectedItem    = m_Pal.VialRange.ToString();                           
            }

            SetBaseDevice(m_Pal);
        }

        #endregion

        #region Events
        /// <summary>
        /// Indicates that the device is available to take commands
        /// </summary>
        public event DelegateFree Free;
        #endregion

        #region Properties
        //TODO: This. There are wait/free events, do I still need this?
        /// <summary>
        /// Keeps track of whether or not the PAL is occupied.
        /// </summary>
        public override bool Running
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        /// <summary>
        /// Decides whether or not the PAL is emulated.
        /// </summary>
        public bool Emulation
        {
            get
            {
                return m_Pal.Emulation;
            }
            set
            {
                m_Pal.Emulation = value;
            }
        }

        /// <summary>
        /// The associated device (PAL).
        /// </summary>
        public IDevice Device
        {
            get
            {
                return m_Pal;
            }
            set
            {
                m_Pal = value as classPal;
                if (m_Pal != null && !DesignMode)
                {
                    try
                    {
                        mcombo_portNames.SelectedText = m_Pal.PortName;
                    }
                    catch
                    {
                        // ignored
                    }
                    RegisterDevice(value);
                }
            }
        }
        #endregion

        #region Methods
        
        /// <summary>
        /// Indicates that the device is available to take commands
        /// </summary>
        public virtual void OnFree(object sender)
        {
            Free?.Invoke(this);
            // m_runningMethodManually = false;
            //mButton_RunMethod.Text = "Run Method";
        }

        public virtual void Pal_DeviceSaveRequired(object sender, EventArgs e)
        {
            //Propogate this
            //TODO: Figure out if this actually worked or not
            //System.Windows.Forms.MessageBox.Show("OH SNAP WE NEED TO SAVE");
            OnSaveRequired();
        }

        /// <summary>
        /// Converts the raw method list string into a list of methods.
        /// </summary>
        /// <param name="rawMethodList">The string which the PAL class returns after GetMethodList()</param>
        public void ProcessMethods(List<string> rawMethodList)
        {
            //LcmsNetDataClasses.Logging.classApplicationLogger.LogMessage(LcmsNetDataClasses.Logging.classApplicationLogger.CONST_STATUS_LEVEL_DETAILED, "PAL ADVANCED CONTROL PROCESS METHODS:" + rawMethodList.Count);
            if (rawMethodList != null)
            {
                mcomboBox_MethodList.BeginUpdate();
                //Clear out the previous list
                mcomboBox_MethodList.Items.Clear();

                //That was cool, so now fill up the combobox
                foreach (var s in rawMethodList)
                {
                    mcomboBox_MethodList.Items.Add(s);
                }
                mcomboBox_MethodList.EndUpdate();
            }
        }

        /// <summary>
        /// Handles when the PAL says it has tray data.
        /// </summary>
        void m_Pal_PalTrayListReceived(object sender, classAutoSampleEventArgs args)
        {
            ProcessTrays(args.TrayList);
        }

        /// <summary>
        /// Converts the raw tray list string into a list of trays.
        /// </summary>
        /// <param name="trayList">The string which the PAL class returns after GetTrayList()</param>
        public void ProcessTrays(List<string> trayList)
        {
            /*LcmsNetDataClasses.Logging.classApplicationLogger.LogMessage(
                                                               LcmsNetDataClasses.Logging.classApplicationLogger.CONST_STATUS_LEVEL_DETAILED,
                                                               "ADVANCED CONTROL PROCESS TRAYS:" + trayList.Count);*/
            if (trayList != null)
            {
                //Clear out the previous list
                mcomboBox_tray.Items.Clear();
                mcomboBox_tray.BeginUpdate();

                //That was cool, so now fill up the combobox
                foreach (var s in trayList)
                {
                    mcomboBox_tray.Items.Add(s);
                }
                mcomboBox_tray.EndUpdate();
            }          
        }

        private void mButton_RefreshMethods_Click(object sender, EventArgs e)
        {
             m_Pal.ListMethods();
             m_Pal.ListTrays();
        }

        private void mButton_RunMethod_Click(object sender, EventArgs e)
        {
            if (mcomboBox_tray.SelectedItem == null)
            {
                MessageBox.Show("No tray selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (mcomboBox_MethodList.SelectedItem == null)
            {
                MessageBox.Show("No method selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (m_Pal.GetStatus().Contains("READY"))
            {
                m_Pal.LoadMethod(mcomboBox_MethodList.SelectedItem.ToString(), mcomboBox_tray.SelectedItem.ToString(), Convert.ToInt32(mnum_vial.Value), Convert.ToString(mnum_volume.Value, CultureInfo.InvariantCulture));
                m_Pal.StartMethod(1000);
            }
            else
            {
                m_Pal.ContinueMethod(0);
            }
        }

        private void mButton_Initialize_Click(object sender, EventArgs e)
        {
            try
            {
                var errorMessage = "";
                m_Pal.Initialize(ref errorMessage);
            }
            catch
            {
                mTextBox_Status.Text = "Could not initialize.";
            }
        }

        private void mButton_StatusRefresh_Click(object sender, EventArgs e)
        {
            mTextBox_Status.Text = m_Pal.GetStatus();
        }

        #endregion
        private void mbutton_stopMethod_Click(object sender, EventArgs e)
        {
            m_Pal.StopMethod();
        }
        private void mcomboBox_VialRange_SelectionChangeCommitted(object sender, EventArgs e)
        {
            m_Pal.VialRange = (enumVialRanges)Enum.Parse(typeof(enumVialRanges), mcomboBox_VialRange.Text);
        }

        private void mbutton_apply_Click(object sender, EventArgs e)
        {
            m_Pal.PortName       = mcombo_portNames.SelectedItem as string;
            mTextBox_Status.Text    = "Port name changed to " + m_Pal.PortName;
        }
    }
}
