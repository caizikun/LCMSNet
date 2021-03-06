﻿using System;
using LcmsNetDataClasses.Devices;
using FluidicsSDK.Devices;
using FluidicsSDK.Managers;
using FluidicsSDK.Base;
using System.Drawing;
using FluidicsSDK.Graphic;

namespace FluidicsPack
{

    public sealed class FluidicsColumnGlyph : FluidicsDevice, IFluidicsDevice
    {
        public event EventHandler DeviceSaveRequired
        {
            add { }
            remove { }
        }

        private IDevice m_device;

        public FluidicsColumnGlyph()
        {
            var newsize = new Size(20, 200);

            GraphicsPrimitive primitive = new FluidicsRectangle(new Point(0, 0),
                                                                newsize,
                                                                Color.Black,
                                                                 Brushes.White);

            AddPrimitive(primitive);

            var points = GeneratePortLocs();
            var inputPort = new Port(points[0], this)
            {
                Source = false,
                Sink = false
            };

            var exitPort = new Port(points[1], this)
            {
                Source = false,
                Sink = false
            };

            AddPort(inputPort);
            AddPort(exitPort);
            ConnectionManager.GetConnectionManager.Connect(inputPort, exitPort, this);
            var c = ConnectionManager.GetConnectionManager.FindConnection(inputPort, exitPort);
            c.Transparent = true;
            m_info_controls_box.Width = 20;
            m_info_controls_box.Height = 200;


        }

        protected override void SetDevice(IDevice device)
        {
            m_device = device;
        }

        protected override void ClearDevice(IDevice device)
        {
            m_device = null;
        }

        public override void ActivateState(int state)
        {

        }

        private Point[] GeneratePortLocs()
        {
            var points = new Point[2];
            points[0] = new Point(Convert.ToInt32(Loc.X + (Size.Width / 2)), Loc.Y);
            points[1] = new Point(Convert.ToInt32(Loc.X + (Size.Width / 2)), Loc.Y + Convert.ToInt32(Size.Height - m_info_controls_box.Size.Height));

            return points;
        }


        /// <summary>
        /// draw controls to screen override of base fluidicsdevice method
        /// </summary>
        /// <param name="g"></param>
        /// <param name="alpha"></param>
        /// <param name="scale"></param>
        protected override void DrawControls(Graphics g, int alpha, float scale)
        {
            var realColor = Color.FromArgb(alpha, Color.Black.R, Color.Black.G, Color.Black.B);

            using (new Pen(realColor))
            using (IDisposable b = new SolidBrush(realColor))
            {
                var br = (SolidBrush)b;
                //determine font size, used to scale font with graphics primitives
                var stringScale = (int)Math.Round(scale < 1 ? -(1 / scale) : scale, 0, MidpointRounding.AwayFromZero);
                using (var stringFont = new Font("Calibri", 11 + stringScale))
                {
                    // draw name to screen
                    var name = DeviceName;

                    m_info_controls_box = UpdateControlBoxLocation();

                    //place the name at the top middle of the box
                    var sf = new StringFormat(StringFormatFlags.DirectionVertical);
                    var nameSize = g.MeasureString(name, stringFont);

                    var nameLocation = CreateStringLocation((int)(m_info_controls_box.Y * scale), nameSize.Height, scale);
                    g.DrawString(name, stringFont, br, nameLocation, sf);
                }
            }
        }


        /// <summary>
        /// update the location of the control box.
        /// </summary>
        /// <returns></returns>
        protected override Rectangle UpdateControlBoxLocation()
        {
            var top = Ports[0].Center.Y + Ports[0].Radius;
            var x = Ports[0].Center.X;
            return new Rectangle(x, top, m_info_controls_box.Width, m_info_controls_box.Height);
        }

        /// <summary>
        /// create location of a string to be drawn in the control box.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="stringHeight"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        protected override Point CreateStringLocation(int y, float stringHeight, float scale)
        {
            // The height is actually our "width" here, since we are drawing the string vertically.
            return new Point((int)((m_info_controls_box.X * scale) - stringHeight / 2),
                    (int)(y + 10));
        }

        public override string StateString()
        {
            return "";
        }

        public override int CurrentState
        {
            get;
            set;
        }
    }
}