﻿/*********************************************************************************************************
 * Written by Christopher Walters for U.S. Department of Energy
 * Pacific Northwest National Laboratory, Richland, WA
 * Copyright 2014 Battle Memorial Institute
 *
 *********************************************************************************************************/
using System;
using FluidicsSDK.Devices;
using LcmsNetDataClasses.Devices;
using System.IO.Ports;
namespace LcmsNet.Devices.Valves
{
    [Serializable]
    ////[classDeviceMonitoring(enumDeviceMonitoringType.Message, "")]
    [classDeviceControlAttribute(typeof(controlValveVICI2Pos),
                                 "Six-port SPE",
                                 "Valves Two-Position")
    ]
    class classValveVICISPE:classValveVICI2Pos, ISolidPhaseExtractor
    {
        public classValveVICISPE()
            : base()
        {
        }

        public classValveVICISPE(SerialPort port)
            : base(port)
        {
        }
    }
}
