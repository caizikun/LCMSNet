﻿using System;
using System.Collections.Generic;


namespace LcmsNet.Devices.NetworkStart.Socket
{
    public enum enumNetStartMessageTypes
    {
        Unknown = 0,
        Query,
        Post,
        Execute,
        Acknowledge,
        Response,
        Error,
        System,
        SystemError
    }
}
