﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using FluidicsSDK.Base;
using FluidicsSDK.Graphic;
using FluidicsSDK.Managers;
using LcmsNetDataClasses.Devices;

namespace FluidicsSDK.Devices.Valves
{
    public sealed class SixPortInjectionFluidicsValveWpf : TwoPositionValveWpf
    {
        private const int NUMBER_OF_PORTS = 6;
        ISixPortInjectionValve m_valve;

        public SixPortInjectionFluidicsValveWpf() :
            base(NUMBER_OF_PORTS)
        {
            m_states = SetupStates();
            m_currentState = TwoPositionState.PositionA;
            base.ActivateState(m_states[m_currentState]);
            var stateControlSize = new Size(15, 15);
            var stateControl1Loc = new Point(Center.X - (stateControlSize.Width * 2), Center.Y - (stateControlSize.Height / 2));
            var stateControl2Loc = new Point(Center.X + stateControlSize.Width, Center.Y - (stateControlSize.Height / 2));
            var stateControlRectangle = new Rect(stateControl1Loc, stateControlSize);
            var stateControlRectangle2 = new Rect(stateControl2Loc, stateControlSize);
            //add left control
            AddPrimitive(new FluidicsTriangleWpf(stateControlRectangle, Orient.Left), LeftButtonAction);
            //add right control
            AddPrimitive(new FluidicsTriangleWpf(stateControlRectangle2, Orient.Right), RightButtonAction);
            //add top "injection port"
            AddPrimitive(new FluidicsRectangleWpf(new Point(Loc.X + (int)Size.Width / 4, Loc.Y - 30), new Size((int)Size.Width / 2, 10), Colors.Black, Brushes.White));
            AddPrimitive(new FluidicsRectangleWpf(new Point(Loc.X + (((int)Size.Width / 2) - 5), Loc.Y - 20), new Size(10, 20), Colors.Black, Brushes.White));
            // add injection loop
            AddPrimitive(new FluidicsLineWpf(m_portList[2].Center, m_portList[5].Center));
            AddPrimitive(new FluidicsRectangleWpf(new Point(Center.X - 25, Center.Y - 15), new Size(50, 30), Colors.Black, Brushes.White, true, 1));
        }

        public override void Render(DrawingContext g, byte alpha, float scale = 1)
        {
            base.Render(g, alpha, scale);
            var stringScale = (int)Math.Round(scale < 1 ? -(1 / scale) : scale, 0, MidpointRounding.AwayFromZero);

            var font = new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var volumeText = new FormattedText(Volume.ToString(CultureInfo.InvariantCulture) + " \u00b5" + "L", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, font, (11.0F * stringScale) * (96.0 / 72.0), Brushes.Black);
            g.DrawText(volumeText, new Point((Center.X * scale - 20), (Center.Y * scale - 10)));
        }

        private void SetValvePosition(TwoPositionState pos)
        {
            m_valve.SetPosition(pos);
        }

        protected override void SetDevice(IDevice device)
        {
            m_valve = device as ISixPortInjectionValve;
            try
            {
                if (m_valve != null)
                {
                    Volume = m_valve.InjectionVolume;
                    m_valve.PositionChanged += m_valve_PositionChanged;
                    m_valve.InjectionVolumeChanged += m_valve_InjectionVolumeChanged;
                }
            }
            catch (Exception)
            {
                // MessageBox.Show("Null valve: " + ex.Message);
            }
        }

        void m_valve_InjectionVolumeChanged(object sender, EventArgs e)
        {
            Volume = m_valve.InjectionVolume;
            DeviceChanged?.Invoke(this, new FluidicsDevChangeEventArgs());
        }

        /// <summary>
        /// Setup the devices states
        /// </summary>
        /// <returns>a dictionary of with TwoPositionState enums as the keys and lists of tuples of int, int type as the values </returns>
        private Dictionary<TwoPositionState, List<Tuple<int, int>>> SetupStates()
        {
            var states = new Dictionary<TwoPositionState, List<Tuple<int, int>>>
            {
                {TwoPositionState.PositionB, GenerateState(0, Ports.Count - 1)},
                {TwoPositionState.PositionA, GenerateState(1, Ports.Count)}
            };
            return states;
        }

        void m_valve_PositionChanged(object sender, ValvePositionEventArgs<TwoPositionState> e)
        {
            ActivateState((int)e.Position);
        }

        /// <summary>
        /// Take a list of tuples and use it to create the internal connections
        /// of the device.
        /// </summary>
        /// <param name="state">a list of tuples, each tuple represents a single internal connection</param>
        public override void ActivateState(List<Tuple<int, int>> state)
        {
            classFluidicsModerator.Moderator.BeginModelSuspension();
            //remove internal connections
            foreach (var p in Ports)
            {
                foreach (var c in p.Connections)
                {
                    if (c.InternalConnectionOf == this)
                    {
                        ConnectionManagerWpf.GetConnectionManager.Remove(c, this);
                    }
                }
            }

            //create new internal connections
            foreach (var t in state)
            {
                ConnectionManagerWpf.GetConnectionManager.Connect(m_portList[t.Item1], m_portList[t.Item2], this);
            }
            // add injection loop connection.
            ConnectionManagerWpf.GetConnectionManager.Connect(m_portList[2], m_portList[5], this);
            var injectionLoopConnection = ConnectionManagerWpf.GetConnectionManager.FindConnection(m_portList[2], m_portList[5]);
            injectionLoopConnection.Transparent = true;
            classFluidicsModerator.Moderator.EndModelSuspension(true);
        }

        protected override void ClearDevice(IDevice device)
        {
            m_valve = null;
        }

        private void ChangePosition(bool left)
        {
            var pos = (int)m_currentState;
            if (m_currentState != TwoPositionState.Unknown)
            {
                if (left)
                {
                    pos--;
                    if (pos < (int)TwoPositionState.PositionA)
                    {
                        pos = (int)TwoPositionState.PositionB;
                    }
                }
                else
                {
                    pos++;
                    if (pos > (int)TwoPositionState.PositionB)
                    {
                        pos = (int)TwoPositionState.PositionA;
                    }
                }
            }
            else
            {
                pos = (int)TwoPositionState.PositionA;
            }
            m_valve.SetPosition((TwoPositionState)pos);
        }

        /// <summary>
        /// action to take when left state primitive is clicked
        /// </summary>
        private void LeftButtonAction()
        {
            ChangePosition(true);
        }

        /// <summary>
        /// action to take when right state primitive is clicked
        /// </summary>
        private void RightButtonAction()
        {
            ChangePosition(false);
        }

        public double Volume
        {
            get;
            set;
        }

        public override event EventHandler<FluidicsDevChangeEventArgs> DeviceChanged;
    }
}
