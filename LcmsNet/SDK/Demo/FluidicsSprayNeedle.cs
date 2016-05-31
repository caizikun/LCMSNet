﻿/*********************************************************************************************************
 * Written by Brian LaMarche and Christopher Walters for U.S. Department of Energy
 * Pacific Northwest National Laboratory, Richland, WA
 * Copyright 2013 Battle Memorial Institute
 * Created 12/31/2013
 * 
 * Last Modified 1/3/2013 By Christopher Walters 
 ********************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LcmsNetDataClasses.Devices;
using FluidicsSDK.Devices;
using System.Windows.Forms;
using FluidicsSDK.Base;
using FluidicsSDK.Managers;
using FluidicsSDK.Graphic;


namespace DemoPluginLibrary
{
    public class FluidicsSprayNeedle:FluidicsDevice
    {
        #region Members
        private const int MAIN_RECT_WIDTH = 50;
        private const int MAIN_RECT_HEIGHT = 20;
        #endregion

        #region Methods
        public FluidicsSprayNeedle()
        {
            Point mainStartPoint = new Point(0, 0);
            Point leftMostStartPoint = new Point(-(MAIN_RECT_WIDTH / 3) - 5, 0);
            Point rightMostStartPoint = new Point(MAIN_RECT_WIDTH + 5, 0);


            //main rectangle            
            base.AddRectangle(mainStartPoint, new Size(MAIN_RECT_WIDTH, MAIN_RECT_HEIGHT), Color.Black, Brushes.White);

            //left most rectangle            
            base.AddRectangle(leftMostStartPoint, new Size(MAIN_RECT_WIDTH / 3, MAIN_RECT_HEIGHT), Color.Black, Brushes.White);  
           
            // bottom left parallelogram + connecting line
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X, leftMostStartPoint.Y + MAIN_RECT_HEIGHT), new Point(leftMostStartPoint.X + 3, leftMostStartPoint.Y + MAIN_RECT_HEIGHT + 10)));
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X + MAIN_RECT_WIDTH / 3, leftMostStartPoint.Y + MAIN_RECT_HEIGHT),
                new Point(leftMostStartPoint.X + MAIN_RECT_WIDTH / 3 + 3, leftMostStartPoint.Y +MAIN_RECT_HEIGHT + 10)));
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X + 3, leftMostStartPoint.Y + MAIN_RECT_HEIGHT + 10),
                 new Point(leftMostStartPoint.X + MAIN_RECT_WIDTH / 3 + 3, leftMostStartPoint.Y + MAIN_RECT_HEIGHT + 10)));
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X + MAIN_RECT_WIDTH / 3 + 3, leftMostStartPoint.Y + MAIN_RECT_HEIGHT + 10), new Point(mainStartPoint.X + 3, mainStartPoint.Y + MAIN_RECT_HEIGHT + 5)));
            
            //bottom middle trapezoid
            base.AddPrimitive(new FluidicsLine(new Point(mainStartPoint.X, mainStartPoint.Y + MAIN_RECT_HEIGHT), new Point(mainStartPoint.X + 5, mainStartPoint.Y + MAIN_RECT_HEIGHT + 10)));
            base.AddPrimitive(new FluidicsLine(new Point(mainStartPoint.X + MAIN_RECT_WIDTH, mainStartPoint.Y + MAIN_RECT_HEIGHT), 
                new Point(mainStartPoint.X + MAIN_RECT_WIDTH - 5, mainStartPoint.Y + MAIN_RECT_HEIGHT + 10)));
            base.AddPrimitive(new FluidicsLine(new Point(mainStartPoint.X + 5, mainStartPoint.Y + MAIN_RECT_HEIGHT + 10), 
                new Point(mainStartPoint.X + MAIN_RECT_WIDTH - 5, mainStartPoint.Y + MAIN_RECT_HEIGHT + 10)));

            // bottom right parallelogram + connecting line
            base.AddPrimitive(new FluidicsLine(new Point(rightMostStartPoint.X, rightMostStartPoint.Y + MAIN_RECT_HEIGHT), new Point(rightMostStartPoint.X - 3, rightMostStartPoint.Y + MAIN_RECT_HEIGHT + 10)));
            base.AddPrimitive(new FluidicsLine(new Point(rightMostStartPoint.X + MAIN_RECT_WIDTH / 3, rightMostStartPoint.Y + MAIN_RECT_HEIGHT),
                new Point(rightMostStartPoint.X + MAIN_RECT_WIDTH / 3 - 3, rightMostStartPoint.Y + MAIN_RECT_HEIGHT + 10)));
            base.AddPrimitive(new FluidicsLine(new Point(rightMostStartPoint.X - 3, rightMostStartPoint.Y + MAIN_RECT_HEIGHT + 10),
                 new Point(rightMostStartPoint.X + MAIN_RECT_WIDTH / 3 - 3, rightMostStartPoint.Y + MAIN_RECT_HEIGHT + 10)));
            base.AddPrimitive(new FluidicsLine(new Point(rightMostStartPoint.X - 3, rightMostStartPoint.Y + MAIN_RECT_HEIGHT + 10), new Point(mainStartPoint.X + MAIN_RECT_WIDTH - 3, mainStartPoint.Y + MAIN_RECT_HEIGHT + 5)));
                                    
            
            // rightmost rectangle            
            base.AddRectangle(rightMostStartPoint, new Size(MAIN_RECT_WIDTH / 3, MAIN_RECT_HEIGHT), Color.Black, Brushes.White);

            // upper left parallelogram + connecting line
            base.AddPrimitive(new FluidicsLine(leftMostStartPoint, new Point(leftMostStartPoint.X + 3, leftMostStartPoint.Y - 10)));
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X + MAIN_RECT_WIDTH / 3, leftMostStartPoint.Y),
                new Point(leftMostStartPoint.X + MAIN_RECT_WIDTH / 3 + 3, leftMostStartPoint.Y - 10)));
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X + 3, leftMostStartPoint.Y - 10), new Point(leftMostStartPoint.X + MAIN_RECT_WIDTH / 3 + 3, leftMostStartPoint.Y - 10)));
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X + MAIN_RECT_WIDTH / 3 + 2, leftMostStartPoint.Y - 10), new Point(mainStartPoint.X + 3, mainStartPoint.Y - 5)));
            
            // upper middle trapezoid
            base.AddPrimitive(new FluidicsLine(mainStartPoint, new Point(mainStartPoint.X + 5, mainStartPoint.Y - 10)));
            base.AddPrimitive(new FluidicsLine(new Point(mainStartPoint.X + MAIN_RECT_WIDTH, mainStartPoint.Y), new Point(mainStartPoint.X + MAIN_RECT_WIDTH - 5, mainStartPoint.Y - 10)));
            base.AddPrimitive(new FluidicsLine(new Point(mainStartPoint.X + 5, mainStartPoint.Y - 10), new Point(mainStartPoint.X + MAIN_RECT_WIDTH - 4, mainStartPoint.Y - 10)));
           
            // upper right parallelogram + connecting line
            base.AddPrimitive(new FluidicsLine(rightMostStartPoint, new Point(rightMostStartPoint.X - 3, rightMostStartPoint.Y - 10)));
            base.AddPrimitive(new FluidicsLine(new Point(rightMostStartPoint.X + MAIN_RECT_WIDTH / 3, rightMostStartPoint.Y),
                new Point(rightMostStartPoint.X + MAIN_RECT_WIDTH / 3 - 3, rightMostStartPoint.Y - 10)));
            base.AddPrimitive(new FluidicsLine(new Point(rightMostStartPoint.X - 3, rightMostStartPoint.Y - 10), 
                new Point(rightMostStartPoint.X + MAIN_RECT_WIDTH / 3 - 3, rightMostStartPoint.Y - 10)));
            base.AddPrimitive(new FluidicsLine(new Point(rightMostStartPoint.X - 2, rightMostStartPoint.Y - 10), new Point(mainStartPoint.X + MAIN_RECT_WIDTH - 3, mainStartPoint.Y - 5)));

            // needle     
            Point needleStartPoint = new Point(leftMostStartPoint.X - MAIN_RECT_WIDTH / 2, MAIN_RECT_HEIGHT / 3);
            base.AddRectangle(needleStartPoint, new Size(MAIN_RECT_WIDTH / 2, MAIN_RECT_HEIGHT / 2), Color.Black, Brushes.White);
            //needle tip
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X - MAIN_RECT_WIDTH / 2, MAIN_RECT_HEIGHT / 3), new Point(needleStartPoint.X - 25, (MAIN_RECT_HEIGHT / 3) +
                MAIN_RECT_HEIGHT / 4)));
            base.AddPrimitive(new FluidicsLine(new Point(leftMostStartPoint.X - MAIN_RECT_WIDTH / 2, MAIN_RECT_HEIGHT / 3 + (MAIN_RECT_HEIGHT / 2)), new Point(needleStartPoint.X- 25,
                (MAIN_RECT_HEIGHT / 3) + MAIN_RECT_HEIGHT / 4)));

            base.AddPort(new Point(rightMostStartPoint.X + MAIN_RECT_WIDTH/3 + 12, rightMostStartPoint.Y + MAIN_RECT_HEIGHT/2));
            m_info_controls_box.X = leftMostStartPoint.X;
            m_info_controls_box.Y = m_primitives.Max(x => x.Loc.Y);
            Ports[0].Sink = true;
        }

        public override bool Contains(Point location)
        {
            bool contains = false;
            int minX = m_primitives.Min(z => z.Loc.X);
            int maxX = m_primitives.Max(z => z.Loc.X);
            int minY = m_primitives.Min(z => z.Loc.Y);
            int maxY = m_primitives.Max(z => z.Loc.Y);
            if ((minX - 20 <= location.X && location.X <= maxX + 20) && (minY - 20 <= location.Y && location.Y <= maxY + 20))
            {
                contains = true;
            }
            return contains;
        }

        public override void ActivateState(int state)
        {
        }

         protected override void SetDevice(IDevice device)
        {        
        }

         protected override void ClearDevice(IDevice device)
         {
         }

        public override string StateString()
        {
            return string.Empty;
        }           
        #endregion

        #region Properties

        public override int CurrentState
        {
            get
            {
                //pumps don't have a state of this kind.
                return -1;
            }
            set
            {
                //do nothing
            }
        }   


        #endregion
    }
}
