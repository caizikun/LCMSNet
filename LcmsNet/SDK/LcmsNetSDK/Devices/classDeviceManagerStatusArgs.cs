﻿using System;

namespace LcmsNetDataClasses.Devices
{
    /// <summary>
    /// Arguments for manager status arguments.
    /// </summary>
    public class classDeviceManagerStatusArgs : EventArgs
    {
        public classDeviceManagerStatusArgs(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the status message.
        /// </summary>
        public string Message { get; private set; }
    }
}