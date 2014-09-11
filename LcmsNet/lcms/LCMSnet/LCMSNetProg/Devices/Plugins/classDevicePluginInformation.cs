﻿using System;
using LcmsNetDataClasses.Devices;


namespace LcmsNet.Devices
{
    /// <summary>
    /// Encapsulates a device type and other extracted properties.
    /// </summary>
    public class classDevicePluginInformation
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="name"></param>
        public classDevicePluginInformation(Type deviceType, classDeviceControlAttribute attribute)
        {
            DeviceType      = deviceType;
            DeviceAttribute = attribute;
        }

        #region Properties
        /// <summary>
        /// Gets or sets the device type.
        /// </summary>
        public Type DeviceType { get; set; }
        /// <summary>
        /// Gets or sets the attribute of the device.
        /// </summary>
        public classDeviceControlAttribute DeviceAttribute {get; set; }
        #endregion

        /// <summary>
        /// Returns the name of the device.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string name = base.ToString();
            if (DeviceAttribute != null)
            {
                name = DeviceAttribute.Name;
            }
            return name;
        }
    }
}
