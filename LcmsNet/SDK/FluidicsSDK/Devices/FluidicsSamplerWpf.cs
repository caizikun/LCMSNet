﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FluidicsSDK.Base;

namespace FluidicsSDK.Devices
{
    public class FluidicsSamplerWpf : FluidicsDeviceWpf, IFluidicsDevice
    {
        readonly Point DEFAULT_POINT = new Point(0, 0);

        public FluidicsSamplerWpf()
            : base()
        {
            DefineGraphics(DEFAULT_POINT);
        }

        public FluidicsSamplerWpf(Point loc)
        {
            DefineGraphics(loc);
        }

        /// <summary>
        /// defines the graphics that make up the PAL primitive
        /// </summary>
        /// <param name="basePoint">location for the "main" graphic to start at</param>
        public void DefineGraphics(Point basePoint)
        {
            //define body
            //base rectangle
            AddRectangle(basePoint, new Size(180, 40), Colors.Black, Brushes.White, fill: true);
            // offset from base rectangle (5, 25)
            AddRectangle(new Point(basePoint.X + 5, basePoint.Y + 25), new Size(170, 8), Colors.Black, Brushes.White, fill: true);

            //define sampler arm
            // offset from base rectangle(25, -35)
            var samplerArmBase = new Point(basePoint.X + 25, basePoint.Y - 35);
            AddRectangle(samplerArmBase, new Size(35, 110), Colors.Black, Brushes.White, fill: true);
            // offset from sampler arm outer rectangle(5, 45)
            AddRectangle(new Point(samplerArmBase.X + 5, samplerArmBase.Y + 45), new Size(25, 62), Colors.Black, Brushes.Gray, fill: true);

            //define sample cabinet + drawers
            //offset from base rectangle (90, 41)
            var cabinetBase = new Point(basePoint.X + 90, basePoint.Y + 41);
            AddRectangle(cabinetBase, new Size(80, 110), Colors.Black, Brushes.White, fill: true);
            //offset from cabinet outer rectangle (5, 8)
            var firstDrawerBase = new Point(cabinetBase.X + 5, cabinetBase.Y + 8);
            AddRectangle(firstDrawerBase, new Size(70, 25), Colors.Black, Brushes.White, fill: true);
            //offset from first drawer(0, 35)
            var secondDrawerBase = new Point(firstDrawerBase.X, firstDrawerBase.Y + 35);
            AddRectangle(secondDrawerBase, new Size(70, 25), Colors.Black, Brushes.White, fill: true);
            //offset from second drawer(0, 35)
            AddRectangle(new Point(secondDrawerBase.X, secondDrawerBase.Y + 35), new Size(70, 25), Colors.Black, Brushes.White, fill: true);
        }

        /// <summary>
        /// IDevice required
        /// </summary>
        /// <param name="device"></param>
        protected override void ClearDevice(LcmsNetDataClasses.Devices.IDevice device)
        {
            IDevice = null;
        }

        /// <summary>
        /// IDevice required
        /// </summary>
        /// <param name="device"></param>
        protected override void SetDevice(LcmsNetDataClasses.Devices.IDevice device)
        {
            IDevice = device;
        }

        /// <summary>
        /// fluidics device required method
        /// </summary>
        /// <param name="state"></param>
        public override void ActivateState(int state)
        {
            //do nothing with this
        }

        /// <summary>
        /// fluidics device required method
        /// </summary>
        /// <returns></returns>
        public override string StateString()
        {
            return string.Empty;
        }

        /// <summary>
        /// fluidics device required method
        /// </summary>
        public override int CurrentState
        {
            //PAL doesn't have a state as valves do
            get
            {
                return -1;
            }
            set
            {
                // do nothing
            }
        }

        public override Size Size
        {
            get
            {
                // get the primitve with the highest value for location + size in both width and height
                var width = m_primitives.Max(x => x.Loc.X + x.Size.Width);
                var height = m_primitives.Max(x => x.Loc.Y + x.Size.Height);
                //subtract the smallest x and y from respective measures
                width -= m_primitives.Min(x => x.Loc.X);
                height -= m_primitives.Min(x => x.Loc.Y);
                // return new size, as width - smallest x = total width of the device
                // and height - smallest y = total height of the device.
                return new Size(width, height);
            }
        }

        #region IFluidicsDevice Members

        public event EventHandler DeviceSaveRequired
        {
            add { }
            remove { }
        }

        #endregion
    }
}
