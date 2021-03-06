﻿/*********************************************************************************************************
// Written by Dave Clark, Brian LaMarche for the US Department of Energy
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2009, Battelle Memorial Institute
// Created 06/30/2009
//
// Updates:
// - 9/08/09 (BLL) Added the Name property to the list
// - 9/14/09 (BLL) Added XML settings file save and load methods.
/*********************************************************************************************************/

namespace LcmsNetDataClasses.Devices
{
    /// <summary>
    /// Describes the interface for user control's that interface a given hardware device or object.
    /// </summary>
    public interface IDeviceControl
    {
        #region "Events"

        event DelegateNameChanged NameChanged;
        event DelegateSaveRequired SaveRequired;

        #endregion

        #region "Properties"

        /// <summary>
        /// Indicates control state
        /// </summary>
        bool Running { get; set; }

        /// <summary>
        /// Gets the device to be controlled that's associated with this interface
        /// </summary>
        IDevice Device { get; set; }

        /// <summary>
        /// Gets or sets the name of the device control.
        /// </summary>
        string Name { get; set; }

        #endregion
    }
}