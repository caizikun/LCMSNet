﻿using System;
using LcmsNetCommonControls.Devices.ContactClosure;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Logging;
using LcmsNetSDK;

namespace LcmsNet.Devices.ContactClosure
{
    /// <summary>
    /// Triggers a mass spectrometer or other device using a labjack TTL pulse.
    /// </summary>
    public class ContactClosureU3ViewModel : ContactClosureViewModelBase<enumLabjackU3OutputPorts>
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContactClosureU3ViewModel()
        {
        }

        #endregion

        #region Members

        /// <summary>
        /// The contact closure class used for triggering a pulse.
        /// </summary>
        private classContactClosureU3 m_contactClosure;

        private const int CONST_MINIMUMVOLTAGE = -5;
        private const int CONST_MAXIMUMVOLTAGE = 5;
        private const int CONST_MINIMUMPULSELENGTH = 0;
        private bool m_loading;
        private enumLabjackU3OutputPorts selectedOutputPort;

        #endregion

        #region Properties

        public override double MinimumVoltage => CONST_MINIMUMVOLTAGE;
        public override double MaximumVoltage => CONST_MAXIMUMVOLTAGE;
        public override int MinimumPulseLength => CONST_MINIMUMPULSELENGTH;

        /// <summary>
        /// Gets or sets the output port of the device.
        /// </summary>
        public override enumLabjackU3OutputPorts Port
        {
            get { return m_contactClosure.Port; }
            set
            {
                if (this.RaiseAndSetIfChangedRetBool(ref selectedOutputPort, value) && m_loading == false)
                {
                    m_contactClosure.Port = value;
                }
            }
        }
        /// <summary>
        /// Determines if the device is in emulation mode or not.
        /// </summary>
        public bool Emulation
        {
            get
            {
                var emulated = true;
                if (m_contactClosure != null)
                {
                    emulated = m_contactClosure.Emulation;
                }
                return emulated;
            }
            set
            {
                if (m_contactClosure != null)
                {
                    m_contactClosure.Emulation = value;
                }
            }
        }

        /// <summary>
        /// The associated device (contact closure)
        /// </summary>
        public override IDevice Device
        {
            get { return m_contactClosure; }
            set
            {
                if (value != null)
                {
                    RegisterDevice(value);
                }
            }
        }

        #endregion

        #region Methods

        private void RegisterDevice(IDevice device)
        {

            m_loading = true;
            m_contactClosure = device as classContactClosureU3;

            if (m_contactClosure != null)
            {
                Port = m_contactClosure.Port;
            }
            if (m_contactClosure != null)
            {
                m_contactClosure.DeviceSaveRequired += CC_DeviceSaveRequired;
            }
            SetBaseDevice(m_contactClosure);
            m_loading = false;
        }

        public virtual void CC_DeviceSaveRequired(object sender, EventArgs e)
        {
            //Propogate this event
            OnSaveRequired();
        }

        /// <summary>
        /// Handles sending a pulse to the Contact Closure when the user presses the button to do so.
        /// </summary>
        protected override void SendPulse()
        {
            if (CONST_MINIMUMVOLTAGE <= Voltage && Voltage <= CONST_MAXIMUMVOLTAGE && CONST_MINIMUMPULSELENGTH <= PulseLength)
            {
                try
                {
                    m_contactClosure.Trigger(PulseLength, Port, Voltage);

                }
                catch (Exception ex)
                {
                    classApplicationLogger.LogError(0, "Could not manually send a pulse in the contact closure.", ex);
                }
            }
        }

        #endregion
    }
}
