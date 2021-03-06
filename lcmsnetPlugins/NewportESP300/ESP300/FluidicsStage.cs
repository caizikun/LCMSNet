﻿using FluidicsSDK.Base;
using System.Drawing;
using LcmsNetDataClasses.Devices;

namespace Newport.ESP300
{
    public sealed class FluidicsStage : FluidicsDevice
    {
        private classNewportStage m_obj;

        private const int STAGE_WIDTH = 100;
        private const int STAGE_HEIGHT = 100;

        public FluidicsStage()
        {
            AddRectangle(new Point(0, 0), new Size(STAGE_WIDTH, STAGE_HEIGHT), Color.Black, new SolidBrush(Color.White), true, null);
            AddPort(new Point(STAGE_WIDTH + 14, STAGE_HEIGHT / 2));
            AddPort(new Point(Loc.X - 14, Loc.Y + STAGE_HEIGHT / 2));
            m_info_controls_box = new Rectangle(new Point(0, 0), new Size(STAGE_WIDTH, STAGE_HEIGHT));
        }


        public override int CurrentState
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        protected override Rectangle UpdateControlBoxLocation()
        {
            return new Rectangle(Loc.X, Loc.Y, STAGE_WIDTH, STAGE_HEIGHT);
        }

        public override string StateString()
        {
            return m_obj.CurrentPos;
        }

        public override void ActivateState(int state)
        {

        }

        protected override void ClearDevice(IDevice device)
        {
            m_obj = null;
        }

        protected override void SetDevice(IDevice device)
        {
            m_obj = device as classNewportStage;
        }

    }
}
