﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidicsSDK.Base;

namespace FluidicsSDK.Devices
{
    /// <summary>
    /// Interface to control a valve
    /// </summary>
    public interface IFourPortValve : IFluidicsDevice
    {        
        event           EventHandler<ValvePositionEventArgs<TwoPositionState>> PositionChanged;        
        int             GetPosition();
        void SetPosition(TwoPositionState s);
    }
}
