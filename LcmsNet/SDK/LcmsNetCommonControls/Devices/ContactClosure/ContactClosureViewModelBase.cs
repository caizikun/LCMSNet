﻿using System;
using System.Linq;
using System.Reactive;
using System.Windows.Controls;
using LcmsNetDataClasses.Devices;
using LcmsNetSDK;

namespace LcmsNetCommonControls.Devices.ContactClosure
{
    /// <summary>
    /// Base view model for a contact closure
    /// </summary>
    /// <typeparam name="T">Enum, with the output port options</typeparam>
    public abstract class ContactClosureViewModelBase<T> : BaseDeviceControlViewModel, IDeviceControlWpf where T : struct
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected ContactClosureViewModelBase()
        {
            outputPortComboBoxOptions = new ReactiveUI.ReactiveList<T>(Enum.GetValues(typeof(T)).Cast<T>());
            SendPulseCommand = ReactiveUI.ReactiveCommand.Create(() => SendPulse());
        }

        #endregion

        #region Members

        private int pulseLength;
        private double voltage;
        private T selectedPort;

        /// <summary>
        /// OutputPortComboBoxOptions backing field
        /// </summary>
        protected readonly ReactiveUI.ReactiveList<T> outputPortComboBoxOptions;

        #endregion

        #region Properties

        /// <summary>
        /// The Port options to show in the ComboBox
        /// </summary>
        public ReactiveUI.IReadOnlyReactiveList<T> OutputPortComboBoxOptions => outputPortComboBoxOptions;

        /// <summary>
        /// The minimum voltage allowed for the contact closure
        /// </summary>
        public abstract double MinimumVoltage { get; }

        /// <summary>
        /// The maximum voltage allowed by the contact closure
        /// </summary>
        public abstract double MaximumVoltage { get; }

        /// <summary>
        /// The minimum pulse length allowed by the contact closure
        /// </summary>
        public abstract int MinimumPulseLength { get; }

        /// <summary>
        /// The pulse length to run
        /// </summary>
        public int PulseLength
        {
            get { return pulseLength; }
            set { this.RaiseAndSetIfChanged(ref pulseLength, value); }
        }

        /// <summary>
        /// The voltage of the pulse
        /// </summary>
        public double Voltage
        {
            get { return voltage; }
            set { this.RaiseAndSetIfChanged(ref voltage, value); }
        }

        /// <summary>
        /// Command to trigger a pulse
        /// </summary>
        public ReactiveUI.ReactiveCommand<Unit, Unit> SendPulseCommand { get; private set; }

        /// <summary>
        /// Gets or sets the output port of the device.
        /// </summary>
        public virtual T Port
        {
            get { return selectedPort; }
            set { this.RaiseAndSetIfChanged(ref selectedPort, value); }
        }

        /// <summary>
        /// The device this view model is associated with
        /// </summary>
        public abstract IDevice Device { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Get the Default view for this view model
        /// </summary>
        /// <returns></returns>
        public virtual UserControl GetDefaultView()
        {
            return new ContactClosureView();
        }

        /// <summary>
        /// Handles sending a pulse to the Contact Closure when the user presses the button to do so.
        /// </summary>
        protected abstract void SendPulse();

        #endregion
    }
}
