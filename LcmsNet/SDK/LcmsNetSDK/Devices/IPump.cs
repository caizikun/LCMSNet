﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LcmsNetSDK.Data;

namespace LcmsNetDataClasses.Devices.Pumps
{
    /// <summary>
    /// Interface for displaying pump data 
    /// </summary>
    public interface IPump
    {
        /// <summary>
        /// Name of the device.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Event fired when new pump data is available.
        /// </summary>
        event EventHandler<PumpDataEventArgs> MonitoringDataReceived;
        /// <summary>
        /// Event fired when a pump setting has changed and needs to be saved.
        /// </summary>
        event EventHandler DeviceSaveRequired;
        /// <summary>
        /// Gets or sets the list of mobile phases associated with the pump.
        /// </summary>
        List<MobilePhase> MobilePhases { get; set; }
    }

}
