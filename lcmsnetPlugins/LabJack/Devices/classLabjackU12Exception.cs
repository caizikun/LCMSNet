﻿//*********************************************************************************************************
// Written by John Ryan, Dave Clark, Brian LaMarche for the US Department of Energy
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2009, Battelle Memorial Institute
// Created 08/17/2009
//
// Last modified 08/17/2009
//*********************************************************************************************************

using System;

namespace LcmsNet.Devices.ContactClosure
{
    /// <summary>
    /// A class containing exceptions generated by the Labjack U12
    /// </summary>
    public class classLabjackU12Exception : Exception
    {
        public classLabjackU12Exception()
            : base()
        {
        }
        public classLabjackU12Exception(string message)
            : base(message)
        {
        }
        public classLabjackU12Exception(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
