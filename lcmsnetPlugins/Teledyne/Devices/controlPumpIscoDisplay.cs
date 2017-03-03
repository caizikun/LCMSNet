﻿//*********************************************************************************************************
// Written by John Ryan, Dave Clark, Brian LaMarche for the US Department of Energy
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2011, Battelle Memorial Institute
// Created 02/25/2011
//
// Last modified 02/25/2011
//*********************************************************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using LcmsNetDataClasses.Devices;

namespace LcmsNet.Devices.Pumps
{
    /// <summary>
    /// Control UI for one of the ISCO pumps in a controller
    /// </summary>
    public partial class controlPumpIscoDisplay : UserControl
    {
        #region "Constants"
        #endregion

        #region "Class variables"
            int m_PumpIndx;
            enumIscoOperationMode m_OperationMode;

            double mdouble_MaxPressureSp = 10000D;  // PSI
            double mdouble_MinPressureSp = 10D;
            double mdouble_MaxFlowSp = 25D; // mL/min
            double mdouble_MinFlowSp = 0.0010D;
            double mdouble_MaxFlowLimit = 50D;  // mL/min
        #endregion

        #region "Delegates"
            private delegate void delegateUpdateNumericDisplayHandler(double newValue);
            private delegate void delegateUpdateTextDisplayHandler(string newValue);
        #endregion

        #region "Events"
            public event DelegateIscoPumpDisplaySetpointHandler SetpointChanged;
            public event DelegateIscoPumpDisplayHandler StartRefill;
            public event DelegateIscoPumpDisplayHandler StartPump;
            public event DelegateIscoPumpDisplayHandler StopPump;
        #endregion

        #region "Properties"
            // Pump flow
            public double FlowRate { set { UpdateFlowrateDisplay(value); } }

            // Pump pressure
            public double Pressure { set { UpdatePressDisplay(value); } }

            // Pump volume
            public double Volume { set { UpdateVolumeDisplay(value); } }

            // Pressure or flow setpoint
            public double Setpoint
            {
                get { return double.Parse(mtextBox_Setpoint.Text); }
                set { UpdateSetPointDisplay(value); }
            }

            // Pump index
            public int PumpIndex { get { return m_PumpIndx; } }

            // Operation mode
            public enumIscoOperationMode OperationMode { set { SetOperationModeDisplays(value); } }

            // Max flow setpoint
            public double MaxFlowSp
            {
                get { return mdouble_MaxFlowSp; }
                set { mdouble_MaxFlowSp = value; }
            }

            // Min flow setpoint
            public double MinFlowSp
            {
                get { return mdouble_MinFlowSp; }
                set { mdouble_MinFlowSp = value; }
            }

            // Max flow limit
            public double MaxFlowLimit
            {
                get { return mdouble_MaxFlowLimit; }
                set { mdouble_MaxFlowLimit = value; }
            }

            // Max pressure setpoint
            public double MaxPressSp
            {
                get { return mdouble_MaxPressureSp; }
                set { mdouble_MaxPressureSp = value; }
            }

            // Min pressure setpoint
            public double MinPressSp
            {
                get { return mdouble_MinPressureSp; }
                set { mdouble_MinPressureSp = value; }
            }

            // Problem status
            public enumIscoProblemStatus ProblemStatus { set { UpdateProblemLabel(value); } }
        #endregion

        #region "Constructors"
            public controlPumpIscoDisplay()
            {
                InitializeComponent();
            }   
        #endregion

        #region "Methods"
            /// <summary>
            /// Initializes the control display
            /// </summary>
            /// <param name="pumpIndx"></param>
            /// <returns></returns>
            public bool InitControl(int pumpIndx)
            {
                // Store the index and update display
                SetPumpIndex(pumpIndx);
                
                return true;
            }   

            /// <summary>
            /// Storees the pump index and updates the user control display
            /// </summary>
            /// <param name="indx">Zero-based index for pump</param>
            private void SetPumpIndex(int indx)
            {
                m_PumpIndx = indx;
                var pumpStr = "";
                switch (m_PumpIndx)
                {
                    case 0:
                        pumpStr = "A";
                        break;
                    case 1:
                        pumpStr = "B";
                        break;
                    case 2:
                        pumpStr = "C";
                        break;
                }

                mlabel_PumpName.Text = "Pump " + pumpStr;
            }   

            /// <summary>
            /// Sets the displays based on operation mode
            /// </summary>
            /// <param name="newMode">New operation mode</param>
            private void SetOperationModeDisplays(enumIscoOperationMode newMode)
            {
                m_OperationMode = newMode;
                switch (newMode)
                {
                    case enumIscoOperationMode.ConstantFlow:
                        mlabel_SetpontUnits.Text = classIscoConversions.GetFlowUnitsString();
                        mbutton_ChangeSetpoint.Text = "Set Flow";
                        break;
                    case enumIscoOperationMode.ConstantPressure:
                        mlabel_SetpontUnits.Text = classIscoConversions.GetPressUnitsString();
                        mbutton_ChangeSetpoint.Text = "Set Press";
                        break;
                }
            }   

            /// <summary>
            /// Determines if test value is within specified range
            /// </summary>
            /// <param name="testVal">Test value</param>
            /// <param name="minVal">Minimum value</param>
            /// <param name="maxVal">Maximum value</param>
            /// <returns>TRUE if value is within range; FALSE otherwise</returns>
            private bool IsValueInRange(double testVal, double minVal, double maxVal)
            {
                if ((testVal >= minVal) && (testVal <= maxVal))
                {
                    return true;
                }
                else return false;
            }   

            /// <summary>
            /// Flow rate display update using delegate
            /// </summary>
            /// <param name="newFlow">New flow value</param>
            private void UpdateFlowrateDisplay(double newFlow)
            {
                if (mtextBox_ActualFlow.InvokeRequired)
                {
                    var d = new delegateUpdateNumericDisplayHandler(UpdateFlowrateDisplay);
                    mtextBox_ActualFlow.BeginInvoke(d, new object[] { newFlow });
                }
                else mtextBox_ActualFlow.Text = newFlow.ToString();
            }   

            /// <summary>
            /// Pressure display update using delegate
            /// </summary>
            /// <param name="newPres">New pressure</param>
            private void UpdatePressDisplay(double newPres)
            {
                if (mtextBox_ActualPressure.InvokeRequired)
                {
                    var d = new delegateUpdateNumericDisplayHandler(UpdatePressDisplay);
                    mtextBox_ActualPressure.BeginInvoke(d, new object[] { newPres });
                }
                else mtextBox_ActualPressure.Text = newPres.ToString();
            }   

            /// <summary>
            /// Volume display update using delegate
            /// </summary>
            /// <param name="newVol">New volume</param>
            private void UpdateVolumeDisplay(double newVol)
            {
                if (mtextBox_ActualVolume.InvokeRequired)
                {
                    var d = new delegateUpdateNumericDisplayHandler(UpdateVolumeDisplay);
                    mtextBox_ActualVolume.BeginInvoke(d, new object[] { newVol });
                }
                else mtextBox_ActualVolume.Text = newVol.ToString("##0.000");
            }   

            /// <summary>
            /// Setpoint display update using delegate
            /// </summary>
            /// <param name="newSetpoint">New setpoint</param>
            private void UpdateSetPointDisplay(double newSetpoint)
            {
                if (mtextBox_Setpoint.InvokeRequired)
                {
                    var d = new delegateUpdateNumericDisplayHandler(UpdateSetPointDisplay);
                    mtextBox_Setpoint.BeginInvoke(d, new object[] { newSetpoint });
                }
                else mtextBox_Setpoint.Text = newSetpoint.ToString();
            }   

            /// <summary>
            /// Updates problem status display
            /// </summary>
            /// <param name="newStatus"></param>
            private void UpdateProblemLabel(enumIscoProblemStatus newStatus)
            {
                var tmpStr = "";
                switch (newStatus)
                {
                    case enumIscoProblemStatus.CylinderBottom:
                        tmpStr = "Bottom";
                        break;
                    case enumIscoProblemStatus.CylinderEmpty:
                        tmpStr = "Empty";
                        break;
                    case enumIscoProblemStatus.MotorFailure:
                        tmpStr = "Mot Fail";
                        break;
                    case enumIscoProblemStatus.None:
                        tmpStr = "";
                        break;
                    case enumIscoProblemStatus.OverPressure:
                        tmpStr = "Over Press";
                        break;
                    case enumIscoProblemStatus.UnderPressure:
                        tmpStr = "Under Press";
                        break;
                    default:
                        tmpStr = "";
                        break;
                }

                UpdateProblemLabelDelegated(tmpStr);
            }   

            /// <summary>
            /// Updates the problem status display via delegate
            /// </summary>
            /// <param name="newValue">New value to display</param>
            private void UpdateProblemLabelDelegated(string newValue)
            {
                if (mlabel_ProbStatus.InvokeRequired)
                {
                    var d = new delegateUpdateTextDisplayHandler(UpdateProblemLabelDelegated);
                    mlabel_ProbStatus.BeginInvoke(d, new object[] { newValue });
                }
                else mlabel_ProbStatus.Text = newValue;
            }   
        #endregion

        #region "Form event handlers"
            /// <summary>
            /// Set Press or Set Flow button clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void mbutton_ChangeSetpoint_Clicked(object sender, EventArgs e)
            {
                double testVal;
                try
                {
                    testVal = double.Parse(mtextBox_Setpoint.Text);
                }
                catch
                {
                    MessageBox.Show("Invalid value entered");
                    return;
                }

                bool validInput;

                if (m_OperationMode == enumIscoOperationMode.ConstantFlow)
                {
                    validInput = IsValueInRange(testVal, mdouble_MinFlowSp, mdouble_MaxFlowSp);
                }
                else validInput = IsValueInRange(testVal, mdouble_MinPressureSp, mdouble_MaxPressureSp);

                if (validInput)
                {
                SetpointChanged?.Invoke(this, m_PumpIndx, double.Parse(mtextBox_Setpoint.Text));
            }
                else MessageBox.Show("Input value not in valid range");
            }   

            /// <summary>
            /// Refill button clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Refill_Clicked(object sender, EventArgs e)
            {
            StartRefill?.Invoke(this, m_PumpIndx);
        }   

            /// <summary>
            /// Start Pump button clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void StartPump_Clicked(object sender, EventArgs e)
            {
            StartPump?.Invoke(this, m_PumpIndx);
        }   

            /// <summary>
            /// Stop Pump button clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void StopPump_Clicked(object sender, EventArgs e)
            {
            StopPump?.Invoke(this, m_PumpIndx);
        }   
        #endregion

            private void label2_Click(object sender, EventArgs e)
            {

            }
    }   
}
