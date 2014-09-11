﻿//*********************************************************************************************************
// Written by John Ryan, Dave Clark, Brian LaMarche for the US Department of Energy 
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2011, Battelle Memorial Institute
// Created 03/03/2011
//
// Last modified 03/03/2011
//*********************************************************************************************************
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Xml;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Method;
using LcmsNetDataClasses.Logging;

namespace LcmsNet.Devices.Pumps
{
	[Serializable]

	[classDeviceControlAttribute(typeof(controlPumpIsco),
										  typeof(controlPumpIscoGlyph),
										  "ISCO Pump",
										  "Pumps")]
	public class classPumpIsco : IDevice
	{
		//*********************************************************************************************************
		// Interface to ISCO pumps
		//**********************************************************************************************************

		#region "Constants"
		#endregion

		#region "Class variables"
			// Device name
			string mstring_Name;

			// Device version
			string mstring_Version;

			// Device status
			enumDeviceStatus menum_Status;

			// Emulation mode
			bool mbool_Emulation;

			// Address for pump controller
			int mint_UnitAddr;

			// Flags for initialization
			bool mbool_Initialized = false;
			bool mbool_Initializing = true;

			// Pump control mode (local/remote)
			enumIscoControlMode menum_ControlMode = enumIscoControlMode.Local;
			
			// Pump operation mode (Constant pressure/Constant flow)
			enumIscoOperationMode menum_OpMode = enumIscoOperationMode.ConstantPressure;

			// Number of pumps connected to this controller (3 max)
			int mint_PumpCount;

			// Status data for each pump
			classPumpIscoData[] mobj_PumpData;

			// Maximum ranges for pump parameters
			classPumpIscoRangeData[] mobj_PumpRanges;

			// Setpoint limits for the pumps
			classPumpIscoSetpointLimits[] mobj_SetpointLimits;

			// Serial port properties
			classIscoSerPortProps mobj_PortProps;

			// Serial port object
			SerialPort mobj_SerialPort;
		#endregion

		#region "Delegates"
		#endregion

		#region "Events"
			public event DelegateIscoPumpRefreshCompleteHandler RefreshComplete;
			public event DelegateIscoPumpInitializationCompleteHandler InitializationComplete;
			public event DelegateIscoPumpInitializingHandler Initializing;
			public event DelegateIscoPumpControlModeSetHandler ControlModeSet;
			public event DelegateIscoPumpOpModeSetHandler OperationModeSet;
			public event DelegateIscoPumpDisconnected Disconnected;
		#endregion

		#region "Properties"
			/// <summary>
			/// Emulation mode
			/// </summary>
			public bool Emulation
			{
				get { return mbool_Emulation; }
				set { mbool_Emulation = value; }
			}

			/// <summary>
			/// Serial port name
			/// </summary>
			[classPersistenceAttribute("PortName")]
			public string PortName
			{
				get { return mobj_SerialPort.PortName; }
                set { mobj_SerialPort.PortName = value; }
			}

            public bool IsOpen()
            {
                return mobj_SerialPort.IsOpen;
            }
			
			/// <summary>
			/// Serial port baud rate
			/// </summary>
			[classPersistenceAttribute("BaudRate")]
			public int BaudRate
			{
				get { return mobj_SerialPort.BaudRate; }
                set { mobj_SerialPort.BaudRate = value; }
			}

			/// <summary>
			/// Serial port read timeout (msec)
			/// </summary>
			[classPersistenceAttribute("ReadTimeout")]
			public int ReadTimeout
			{
				get { return mobj_SerialPort.ReadTimeout; }
                set { mobj_SerialPort.ReadTimeout = value; }
			}

			/// <summary>
			/// Serial port write timeout (msec)
			/// </summary>
			[classPersistenceAttribute("WriteTimeout")]
			public int WriteTimeout
			{
				get { return mobj_SerialPort.WriteTimeout; }
                set { mobj_SerialPort.WriteTimeout = value; }
			}

			/// <summary>
			/// Class initialization state
			/// </summary>
			public bool Initialized
			{
				get { return mbool_Initialized; }
			}

			/// <summary>
			/// Number of pumps connected to this controller (1 min, 3 max)
            /// </summary>
            [classPersistenceAttribute("PumpCount")]
			public int PumpCount
			{
				get { return mint_PumpCount; }
				set { mint_PumpCount = value; }
			}

			/// <summary>
			/// Address for the pump controller
            /// </summary>
            [classPersistenceAttribute("UnitAddress")]
			public int UnitAddress
			{
				get { return mint_UnitAddr; }
				set
				{ 
					mint_UnitAddr = value;
					mobj_PortProps.UnitAddress = mint_UnitAddr;
				}
			}

			/// <summary>
			/// Minimum pump controller address
			/// </summary>
			public int UnitAddressMin { get { return mobj_PortProps.UnitAddressMin; } }

			/// <summary>
			/// Maximum pump controller address
			/// </summary>
			public int UnitAddressMax { get { return mobj_PortProps.UnitAddressMax; } }

			/// <summary>
			/// Control mode (local/remote)
			/// </summary>
			public enumIscoControlMode ControlMode
			{
				get { return menum_ControlMode; }
				set { SetControlMode(value); }
			}

			/// <summary>
			/// Operation mode (Const flow/Const pressure)
			/// </summary>
			public enumIscoOperationMode OperationMode
			{
				get { return menum_OpMode; }
				set { SetOperationMode(value); }
			}

			/// <summary>
			/// Current data for all pumps
			/// </summary>
			public classPumpIscoData[] PumpData { get { return mobj_PumpData; } }
		#endregion

		#region "Constructors"
			public classPumpIsco()
			{
                mstring_Name = "Isco Pump";
                //mstring_Name = classDeviceManager.Manager.CreateUniqueDeviceName(mstring_Name);
				// Initialize pump data array
				mobj_PumpData = new classPumpIscoData[] { new classPumpIscoData(), new classPumpIscoData(), 
										new classPumpIscoData() };

				// Initialize setpoint array
				mobj_SetpointLimits = new classPumpIscoSetpointLimits[] { new classPumpIscoSetpointLimits(), 
												new classPumpIscoSetpointLimits(), new classPumpIscoSetpointLimits() };

                // Initialize pump range array
                mobj_PumpRanges = new classPumpIscoRangeData[] { new classPumpIscoRangeData(), 
                                                new classPumpIscoRangeData(), new classPumpIscoRangeData() };

                mobj_SerialPort = new SerialPort();

				// Configure the serial port with default settings
				mobj_PortProps = new classIscoSerPortProps();
				ConfigSerialPort(mobj_PortProps);
			}	
		#endregion

		#region "Methods"
			/// <summary>
			/// Converts a numeric (zero-based) pump index into an alpha pump designation
			/// </summary>
			/// <param name="pumpIndx">Index</param>
			/// <param name="pumpAIsBlank">If TRUE, returns "" when pump A is specified</param>
			/// <returns>Empty string, "A", "B", or "C", depending on input enum and pumpAIsBlank setting</returns>
			public string ConvertPumpIndxToString(int pumpIndx, bool pumpAIsBlank)
			{
				string retStr = "";

				if (pumpAIsBlank && (pumpIndx == 0))
				{
					// Pump A just uses an empty string for the pump designation
					retStr = "";
				}
				else retStr = ((char)(pumpIndx + 65)).ToString();	// Pump index is assumed to be zero-based

				return retStr;
			}	

			/// <summary>
			/// Extracts the innermost exception from a nested exception
			/// </summary>
			/// <param name="ex">Input exception</param>
			/// <returns>Innermost exception</returns>
			public Exception GetBaseException(Exception ex)
			{
				Exception retExc = ex;
				if (!(ex.InnerException == null))
				{
					retExc = ex.InnerException;
					GetBaseException(ex.InnerException);
				}

				return retExc;
			}	

			/// <summary>
			/// Gets an object containing data for specified pump
			/// </summary>
			/// <param name="pumpIndx">Index of pump (0-2)</param>
			/// <returns>Object containing pump data</returns>
			public classPumpIscoData GetPumpData(int pumpIndx)
			{
				if (pumpIndx < mobj_PumpData.Length)
				{
					return mobj_PumpData[pumpIndx];
				}
				else return null;
			}

			/// <summary>
			/// Gets an object containing ranges for specified pump
			/// </summary>
			/// <param name="pumpIndx">Index of pump (0-2)</param>
			/// <returns>Object containing range data</returns>
			public classPumpIscoRangeData GetPumpRanges(int pumpIndx)
			{
				if (pumpIndx < mobj_PumpRanges.Length)
				{
					return mobj_PumpRanges[pumpIndx];
				}
				else return null;
			}	

			/// <summary>
			/// Gets an object containing setpoint limits for specified pump
			/// </summary>
			/// <param name="pumpIndx">Index of pump (0-2)</param>
			/// <returns>Object containing setpoint limit data</returns>
			public classPumpIscoSetpointLimits GetSetpointLimits(int pumpIndx)
			{
				if (pumpIndx < mobj_SetpointLimits.Length)
				{
					return mobj_SetpointLimits[pumpIndx];
				}
				else return null;
			}	
		#endregion

		#region "Pump control and monitoring methods"
			/// <summary>
			/// Closes serial port and resets selected parameters
			/// </summary>
			public bool Disconnect()
			{
#if DACTEST
				LogMessage("Beginning Disconnect");	// DAC testing
#endif
				
				if (mbool_Emulation)
				{
					// Emulation mode - just fake disconnecting
					menum_ControlMode = enumIscoControlMode.Local;
					mbool_Initialized = false;
					if (Disconnected != null) Disconnected();
					return true;
				}

#if DACTEST
				LogMessage("Disconnect: Testing initialization");	// DAC testing
#endif

				if (mbool_Initialized)
				{
#if DACTEST
					LogMessage("Disconnect: Setting control mode to Local");	// DAC testing
#endif

					// Set the control mode to Local
					SetControlMode(enumIscoControlMode.Local);

#if DACTEST
					LogMessage("Disconnect: Closing serial port");	// DAC testing
#endif

					// Close the serial port
					try
					{
						mobj_SerialPort.Close();
					}
					catch (Exception ex)
					{
						classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception closing serial port during Disconnect",
																	GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
																	classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
						if (Error != null) Error(this, args);
						return false;
					}

#if DACTEST
					LogMessage("Disconnect: Setting initialization state to FALSE");	// DAC testing
#endif


					mbool_Initialized = false;
				}

#if DACTEST
				LogMessage("Disconnect: Firing Disconnected event");	// DAC testing
#endif


				if (Disconnected != null) Disconnected();
				return true;
			}	

			/// <summary>
			/// Sets pump control mode to local or remote
			/// </summary>
			/// <param name="newMode">New control mode</param>
			/// <returns>TRUE for success; FALSE otherwise</returns>
			public bool SetControlMode(enumIscoControlMode newMode)
			{
				if (mbool_Emulation)
				{
					menum_ControlMode = newMode;
					if (ControlModeSet != null) ControlModeSet(newMode);
					return true;
				}

				// If already in same mode, no need to change it
				if (menum_ControlMode == newMode) return true;

				// check initialization state
				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				// Build the command
				string cmd = "";
				string notifyString = "";

				switch (newMode)
				{
					case enumIscoControlMode.External:
						cmd = "EXTERNAL";
						notifyString = classIscoStatusNotifications.GetNotificationString(newMode.ToString());
						break;
					case enumIscoControlMode.Local:
						cmd = "LOCAL";
						notifyString = classIscoStatusNotifications.GetNotificationString(newMode.ToString());
						break;
					case enumIscoControlMode.Remote:
						cmd = "REMOTE";
						notifyString = classIscoStatusNotifications.GetNotificationString(newMode.ToString());
						break;
				}

				// Send command to pump controller
				string resp = "";
				try
				{
					resp = SendCommand(cmd, true);
					menum_ControlMode = newMode;
                    classDeviceStatusEventArgs args = new classDeviceStatusEventArgs(menum_Status, notifyString, this);
					if (StatusUpdate != null) StatusUpdate(this, args);
					if (ControlModeSet != null) ControlModeSet(newMode);
					return true;
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception setting control mode", GetBaseException(ex),
												enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
												classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					if (ControlModeSet != null) ControlModeSet(menum_ControlMode);
					return false;
				}
			}
            [classLCMethodAttribute("Set Mode", enumMethodOperationTime.Parameter, "", -1, false)]
            public bool SetOperationMode(double timeout, enumIscoOperationMode newMode)
            {
                return SetOperationMode(newMode);
            }
			/// <summary>
			/// Sets the operation mode for all pumps (Const flow/Const pressure)
			/// </summary>
			/// <param name="newMode">New operation mode</param>
			/// <returns>TRUE for success; FALSE otherwise</returns>
			public bool SetOperationMode(enumIscoOperationMode newMode)
			{
				if (mbool_Emulation)
				{
					menum_OpMode = newMode;
					if (OperationModeSet != null) OperationModeSet(newMode);
					return true;
				}

				// If already in same mode, no need to change it
				if (menum_OpMode == newMode) return true;

				string baseCmd = "";
				string notifyString = "";

				// Initialize the command
				switch (newMode)
				{
					case enumIscoOperationMode.ConstantFlow:
						baseCmd = "CONST FLOW";
						notifyString = classIscoStatusNotifications.GetNotificationString(newMode.ToString());
						break;
					case enumIscoOperationMode.ConstantPressure:
						baseCmd = "CONST PRESS";
						notifyString = classIscoStatusNotifications.GetNotificationString(newMode.ToString());
						break;
				}

				bool success = true;
				for (int indx = 0; indx < mint_PumpCount; indx++)
				{
					// Build the command for each pump
					string cmd = baseCmd + ConvertPumpIndxToString(indx, true);

					// Send command to pump controller
					try
					{
						string resp = SendCommand(cmd, true);
					}
					catch (Exception ex)
					{
						classDeviceErrorEventArgs errArgs = new classDeviceErrorEventArgs("Exception setting operation mode", 
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
						if (Error != null) Error(this, errArgs);
						success = false;
					}
				}

				// All finished - save and report status
				if (success)
				{
					menum_OpMode = newMode;
                    classDeviceStatusEventArgs args = new classDeviceStatusEventArgs(menum_Status, notifyString, this);
					if (StatusUpdate != null) StatusUpdate(this, args);
					if (OperationModeSet != null) OperationModeSet(newMode);
					return true;
				}
				else
				{
					if (OperationModeSet != null) OperationModeSet(menum_OpMode);
					return false;
				}
			}	

			/// <summary>
			/// Initializes the pump configuration
			/// </summary>
			/// <returns></returns>
			private bool InitDevice(ref string msg)
			{
				string notifyStr;

				if (Initializing != null) 
                    Initializing();

				// If in emulation mode, just return
				if (mbool_Emulation) 
                    return true;

				// Close and reopen com port (to ensure any settings changes take effect)
				if (!Disconnect())
				{
					msg = "Initialization error during Disconnect";
					notifyStr = classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.InitializationError.ToString());
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(msg, null,
												enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
												notifyStr);
                    if (Error != null)
                    {
                        Error(this, args);
                    }
					return false;
				}

				mbool_Initializing = true;

				// Load the com port parameters and open the port
				try
				{
					mobj_SerialPort.Open();
                    if (!mobj_SerialPort.IsOpen)
                    {
                        throw new Exception(string.Format("The serial port on the ISCO pump {0} could not be opened.", mstring_Name));
                    }
				}
				catch (Exception ex)
				{
					msg = "Initialization error when opening serial port";
					notifyStr = classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.InitializationError.ToString());
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(msg, ex,
												enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
												notifyStr);
					if (Error != null) Error(this, args);
					return false;
				}

				// Get the units data from the pump
                if (!ReadPumpUnits())
                {
                    msg = "Could not read pump units.";
                    return false;
                }


				// Refresh the pump data
				if (!Refresh())
                {
                    msg = "Could not retrieve pump data.";
                    return false;
                }

				// Get the setpoint and operating limits data
				for (int pumpIndx = 0; pumpIndx < mint_PumpCount; pumpIndx++)
				{
					// Setpoint limits
					mobj_SetpointLimits[pumpIndx].MinFlowSp    = ReadMinFlowSetpoint(pumpIndx);
					mobj_SetpointLimits[pumpIndx].MaxFlowSp    = ReadMaxFlowSetpoint(pumpIndx);
					mobj_SetpointLimits[pumpIndx].MinPressSp   = ReadMinPressSetpoint(pumpIndx);
					mobj_SetpointLimits[pumpIndx].MaxPressSp   = ReadMaxPressSetpoint(pumpIndx);
					mobj_SetpointLimits[pumpIndx].MaxFlowLimit = ReadMaxFlowLimit(pumpIndx);

					// Range limits
					mobj_PumpRanges[pumpIndx] = ReadPumpRanges(pumpIndx);
				}

				// Set remote mode
				if (!SetControlMode(enumIscoControlMode.Remote)) 
                {
                    msg = "Could not set the control mode.";
                    return false;
                }

				mbool_Initializing = false;
				mbool_Initialized = true;



				// Tell world initializaion is complete
				if (InitializationComplete != null) 
                    InitializationComplete();

				return true;
			}	

			/// <summary>
			/// Refreshes the stored data for a pump
			/// </summary>
			/// <returns>TRUE for success; FALSE otherwise</returns>
			public bool Refresh()
			{
				if (mbool_Emulation) return true;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				string cmd = "G&";
				string resp = "";
				try
				{
					resp = SendCommand(cmd, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception refreshing status", GetBaseException(ex),
													enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				try
				{
					mobj_PumpData = ParseStatusMessage(resp);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception parsing status message", 
													GetBaseException(ex),
													enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.MessageParseError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				// Get operation mode and setpoint for each pump
				for (int pumpIndx = 0; pumpIndx < mint_PumpCount; pumpIndx++)
				{
					// Operation mode
					mobj_PumpData[pumpIndx].OperationMode = menum_OpMode;

					// Setpoint
					mobj_PumpData[pumpIndx].SetPoint = ReadSetpoint(pumpIndx, mobj_PumpData[pumpIndx].OperationMode);

					// Refill rate
					mobj_PumpData[pumpIndx].RefillRate = ReadRefillRate(pumpIndx);
				}
				
				// Check for errors
				ReportPumpErrors();

				// Notify anybody who happens to be listening
				if (RefreshComplete != null) RefreshComplete();

				return true;
			}

            //public double GetPumpVolume(double timeout, int pumpId)
            public double GetPumpVolume(double timeout, enumISCOPumpChannels pump)
            {
                Refresh();
                return mobj_PumpData[(int)pump].Volume;
            }

			/// <summary>
			/// Gets a flow or pressure setpoint
			/// </summary>
			/// <param name="pumpIndx">Index of pump</param>
			/// <param name="opMode">Operation mode (determines whether to get flow or pressure SP)</param>
			/// <returns>Current setpoint; -1000 if there's a problem</returns>
			public double ReadSetpoint(int pumpIndx, enumIscoOperationMode opMode)
			{
				if (mbool_Emulation) return 0.0;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				string cmdStr = "";

				// Create the command string
				switch (opMode)
				{
					case enumIscoOperationMode.ConstantPressure:
						cmdStr = "SETPRESS" + ConvertPumpIndxToString(pumpIndx, false);
						break;
					case enumIscoOperationMode.ConstantFlow:
						cmdStr = "SETFLOW" + ConvertPumpIndxToString(pumpIndx, false);
						break;
				}
				
				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting setpoint", GetBaseException(ex),
													enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				return ParseNumericValue(resp);
			}	

			/// <summary>
			/// Gets the pump's minimum flow setpoint
			/// </summary>
			/// <param name="pumpIndx">Index of pump</param>
			/// <returns>Min flow setpoint; -1000 if there's a problem</returns>
			public double ReadMinFlowSetpoint(int pumpIndx)
			{
				if (mbool_Emulation) return 0.0;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				string cmdStr = "MINFLOW" + ConvertPumpIndxToString(pumpIndx, false);

				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting setpoint limit", 
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				return ParseNumericValue(resp);
			}	

			/// <summary>
			/// Gets the pump's maximum flow setpoint
			/// </summary>
			/// <param name="pumpIndx">Index of pump</param>
			/// <returns>Max flow setpoint; -1000 if there's a problem</returns>
			public double ReadMaxFlowSetpoint(int pumpIndx)
			{
				if (mbool_Emulation) return 0.0;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				string cmdStr = "MAXFLOW" + ConvertPumpIndxToString(pumpIndx, false);

				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting setpoint limit",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				return ParseNumericValue(resp);
			}	

			/// <summary>
			/// Gets the max flow limit for a pump
			/// </summary>
			/// <param name="pumpIndx">Index of pump</param>
			/// <returns>Max flow limit; -1000 if there's a problem</returns>
			public double ReadMaxFlowLimit(int pumpIndx)
			{
				if (mbool_Emulation) return 30.0;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				string cmdStr = "MFLOW" + ConvertPumpIndxToString(pumpIndx, false);

				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting setpoint limit",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				return ParseNumericValue(resp);
			}	

			/// <summary>
			/// Gets the pump's minimum pressure setpoint
			/// </summary>
			/// <param name="pumpIndx">Index of pump</param>
			/// <returns>Min pressure setpoint; -1000 if there's a problem</returns>
			public double ReadMinPressSetpoint(int pumpIndx)
			{
				if (mbool_Emulation) return 0.0;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				string cmdStr = "MINPRESS" + ConvertPumpIndxToString(pumpIndx, false);

				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting setpoint limit",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				return ParseNumericValue(resp);
			}	

			/// <summary>
			/// Gets the pump's maximum pressure setpoint
			/// </summary>
			/// <param name="pumpIndx">Index of pump</param>
			/// <returns>Max pressure setpoint; -1000 if there's a problem</returns>
			public double ReadMaxPressSetpoint(int pumpIndx)
			{
				if (mbool_Emulation) return 0.0;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				string cmdStr = "MAXPRESS" + ConvertPumpIndxToString(pumpIndx, false);

				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting setpoint limit",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				return ParseNumericValue(resp);
			}	

			/// <summary>
			/// Gets the maximum pump ranges
			/// </summary>
			/// <param name="pumpIndx">Index of pump</param>
			/// <returns>Object containing pump range data</returns>
			public classPumpIscoRangeData ReadPumpRanges(int pumpIndx)
			{
				if (mbool_Emulation) return null;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return null;
				}

				string cmdStr = "RANGE" + ConvertPumpIndxToString(pumpIndx, false);

				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting setpoint limit",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return null;
				}

				return ParseRangeMessage(resp);
			}


            [classLCMethodAttribute("Refill", enumMethodOperationTime.Parameter, "", -1, false)]
            //public bool StartRefill(double timeout, int pumpIndx, double refillRate)
            public bool StartRefill(double timeout, enumISCOPumpChannels pump, double refillRate)
            {
                return StartRefill((int)pump, refillRate);
            }
			/// <summary>
			/// Starts a refill for specified pump
			/// </summary>
			/// <param name="pumpIndx">Index for pump to refill</param>
			/// <param name="refillRate">Rate of refill</param>
			/// <returns>TRUE for success; FALSE otherwise</returns>
			public bool StartRefill(int pumpIndx, double refillRate)
			{
				if (mbool_Emulation) return true;

				if (!mbool_Initialized)
				{
					classDeviceErrorEventArgs errorArgs = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, errorArgs);
					return false;
				}

				string cmdStrRate = "";
				string cmdStrStart = "";

				// Create the command string for the refill rate
				cmdStrRate = "REFILL" + ConvertPumpIndxToString(pumpIndx, true) + "=" + refillRate.ToString();

				// Create the command string for starting the refill
				cmdStrStart = "REFILL" + ConvertPumpIndxToString(pumpIndx, true);

				// Send the rate command
				try
				{
					string resp = SendCommand(cmdStrRate, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception setting refill rate", 
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				// Send the start command
				try
				{
					SendCommand(cmdStrStart, false);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception starting refill",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				// Everything worked, so send message and return success
				classDeviceStatusEventArgs statusArgs = new classDeviceStatusEventArgs(enumDeviceStatus.Initialized,
							classIscoStatusNotifications.GetNotificationString(enumIscoOperationStatus.Refilling.ToString() +
                            ConvertPumpIndxToString(pumpIndx, false)), this);
				if (StatusUpdate != null) StatusUpdate(this, statusArgs);
				return true;
			}
            /// <summary>
            /// Starts the pump.
            /// </summary>
            /// <param name="timeout"></param>
            /// <param name="channel"></param>
            /// <returns></returns>
            [classLCMethodAttribute("Start Pump", enumMethodOperationTime.Parameter, "", -1, false)]
            public bool StartPump(double timeout, enumISCOPumpChannels pump)
            {
                return StartPump(timeout, (int) pump);
            }
			/// <summary>
			/// Starts running the specified pump
			/// </summary>
			/// <param name="pumpIndx">Pump to start</param>
			/// <returns>TRUE for success; FALSE otherwise</returns>
            ///             
			public bool StartPump(double timeout, int pumpIndx)
			{
				if (mbool_Emulation) return true;

				if (!mbool_Initialized)
				{
					classDeviceErrorEventArgs errorArgs = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, errorArgs);
					return false;
				}

				string cmdStr = "RUN" + ConvertPumpIndxToString(pumpIndx, true);

				// Send the command
				try
				{
					string resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception starting pump",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				// Everything worked, so send message and return success
				classDeviceStatusEventArgs statusArgs = new classDeviceStatusEventArgs(enumDeviceStatus.Initialized,
							classIscoStatusNotifications.GetNotificationString(enumIscoOperationStatus.Running.ToString() +
                            ConvertPumpIndxToString(pumpIndx, false)), this);
				if (StatusUpdate != null) StatusUpdate(this, statusArgs);
				return true;
			}	

			/// <summary>
			/// Stops specified pump
			/// </summary>
			/// <param name="pumpIndx">Pump to stop</param>
			/// <returns>TRUE for success; FALSE otherwise</returns>
            /// 

            [classLCMethodAttribute("Stop Pump", enumMethodOperationTime.Parameter, "", -1, false)]
            public bool StopPump(double timeout, enumISCOPumpChannels pump)
            {
                return StopPump(timeout, (int)pump);
            }            
			public bool StopPump(double timeout, int pumpIndx)
			{
				if (mbool_Emulation) return true;

				if (!mbool_Initialized)
				{
					classDeviceErrorEventArgs errorArgs = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, errorArgs);
					return false;
				}

				string cmdStr = "STOP" + ConvertPumpIndxToString(pumpIndx, true);

				// Send the command
				try
				{
					string resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception stopping pump",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				// Everything worked, so send message and return success
				classDeviceStatusEventArgs statusArgs = new classDeviceStatusEventArgs(enumDeviceStatus.Initialized,
							classIscoStatusNotifications.GetNotificationString(enumIscoOperationStatus.Stopped.ToString() +
                            ConvertPumpIndxToString(pumpIndx, false)), this);
				if (StatusUpdate != null) StatusUpdate(this, statusArgs);
				return true;
            }
            [classLCMethodAttribute("Set Flow Rate", enumMethodOperationTime.Parameter, "", -1, false)]
            //public bool SetFlow(double timeout, int pumpIndx, double newFlow)
            public bool SetFlow(double timeout, enumISCOPumpChannels pump, double newFlow)
            {
                return SetFlow((int)pump, newFlow);
            }
			/// <summary>
			/// Sets the flow rate for a pump in constant flow mode
			/// </summary>
			/// <param name="pumpIndx">Pump to set</param>
			/// <param name="newFlow">New flow rate</param>
			/// <returns>TRUE for success; FALSE otherwise</returns>
			public bool SetFlow(int pumpIndx, double newFlow)
			{
				if (mbool_Emulation) return true;

				if (!mbool_Initialized)
				{
					classDeviceErrorEventArgs errorArgs = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, errorArgs);
					return false;
				}

				string cmdStr = "FLOW" + ConvertPumpIndxToString(pumpIndx, true) + "=" + newFlow.ToString();

				// Send the command
				try
				{
					string resp = SendCommand(cmdStr, false);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception setting flow",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				// Everything worked, so send message and return success
				classDeviceStatusEventArgs statusArgs = new classDeviceStatusEventArgs(enumDeviceStatus.Initialized,
							classIscoStatusNotifications.GetNotificationString(enumIscoOperationStatus.FlowSet.ToString() +
                            ConvertPumpIndxToString(pumpIndx, false)), this);
				if (StatusUpdate != null) StatusUpdate(this, statusArgs);
				return true;
			}	

			/// <summary>
			/// Sets pressure for a pump in constant pressure mode
			/// </summary>
			/// <param name="pumpIndx">Pump to set</param>
			/// <param name="newPress">New pressure setpoint</param>
			/// <returns>TRUE for success; FALSE otherwise</returns>            
            [classLCMethodAttribute("Set Pressure", enumMethodOperationTime.Parameter, "", -1, false)]
            public bool SetPressure(double timeout, enumISCOPumpChannels pump, double NewPressure)
            {
                return SetPressure((int)pump, NewPressure);
            }
			public bool SetPressure(int pumpIndx, double newPress)
			{
				if (mbool_Emulation) return true;

				if (!mbool_Initialized)
				{
					classDeviceErrorEventArgs errorArgs = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, errorArgs);
					return false;
				}

				string cmdStr = "PRESS" + ConvertPumpIndxToString(pumpIndx, true) + "=" + newPress.ToString();

				// Send the command
				try
				{
					string resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception setting pressure",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				// Everything worked, so send message and return success
				classDeviceStatusEventArgs statusArgs = new classDeviceStatusEventArgs(enumDeviceStatus.Initialized,
							classIscoStatusNotifications.GetNotificationString(enumIscoOperationStatus.PressSet.ToString() +
                            ConvertPumpIndxToString(pumpIndx, false)), this);
				if (StatusUpdate != null) StatusUpdate(this, statusArgs);
				return true;
			}	

			/// <summary>
			/// Reports errors found from a pump status request
			/// </summary>
			private void ReportPumpErrors()
			{
				string notifyMsg = "";
				bool errorFound;

				for (int indx = 0; indx < mint_PumpCount; indx++)
				{
					notifyMsg = "";
					errorFound = false;
					switch (mobj_PumpData[indx].ProblemStatus)
					{
						case enumIscoProblemStatus.None:
							// No action necessary
							break;
						case enumIscoProblemStatus.OverPressure:
							notifyMsg = classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.OverPressure.ToString() +
								ConvertPumpIndxToString(indx, false));
							errorFound = true;
							break;
						case enumIscoProblemStatus.UnderPressure:
							notifyMsg = classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.UnderPressure.ToString() +
								ConvertPumpIndxToString(indx, false));
							errorFound = true;
							break;
						case enumIscoProblemStatus.CylinderBottom:
							notifyMsg = classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.CylinderBottom.ToString() +
								ConvertPumpIndxToString(indx, false));
							errorFound = true;
							break;
						case enumIscoProblemStatus.CylinderEmpty:
							notifyMsg = classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.CylinderEmpty.ToString() +
								ConvertPumpIndxToString(indx, false));
							errorFound = true;
							break;
						case enumIscoProblemStatus.MotorFailure:
							notifyMsg = classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.MotorFailure.ToString() +
								ConvertPumpIndxToString(indx, false));
							errorFound = true;
							break;
					}

					if (errorFound)
					{
						// Pump had an error, so report it
						classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Pump error", null,
								enumDeviceErrorStatus.ErrorAffectsAllColumns, this, notifyMsg);
						if (Error != null) Error(this, args);
					}
				}
			}	

			/// <summary>
			/// Gets the pump units from the pump and stores them in static class classIscoConversions
			/// </summary>
			private bool ReadPumpUnits()
			{
				if (mbool_Emulation)
				{
					classIscoConversions.FlowUnits = enumIscoFlowUnits.ul_min;
					classIscoConversions.PressUnits = enumIscoPressureUnits.psi;
					return true;
				}

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classIscoConversions.FlowUnits = enumIscoFlowUnits.error;
					classIscoConversions.PressUnits = enumIscoPressureUnits.error;

					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				string cmdStr = "UNITSA";

				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classIscoConversions.FlowUnits = enumIscoFlowUnits.error;
					classIscoConversions.PressUnits = enumIscoPressureUnits.error;

					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting units",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return false;
				}

				if (ParseUnitsMessage(resp))
				{
					return true;
				}
				else return false;
			}	

			/// <summary>
			/// Reads the current refill rate setting
			/// </summary>
			/// <param name="pumpIndx">Index for pump</param>
			/// <returns>Refill rate; -1000 on error</returns>
			private double ReadRefillRate(int pumpIndx)
			{
				if (mbool_Emulation) return 5.0;

				if (!(mbool_Initialized || mbool_Initializing))
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Device not initialized", null,
										enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
										classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.DeviceNotInitialized.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				string cmdStr = "RLIMIT" + ConvertPumpIndxToString(pumpIndx, false);

				// Send the command and wait for a response
				string resp = "";
				try
				{
					resp = SendCommand(cmdStr, true);
				}
				catch (Exception ex)
				{
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs("Exception getting refill rate",
													GetBaseException(ex), enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
													classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.ComError.ToString()));
					if (Error != null) Error(this, args);
					return -1000;
				}

				return ParseNumericValue(resp);
			}	
		#endregion

		#region "Serial port operation methods"
			/// <summary>
			/// Configures the serial port
			/// </summary>
			/// <param name="portProps">Port properties</param>
			private void ConfigSerialPort(classIscoSerPortProps portProps)
			{
				if (mobj_SerialPort == null)
				{
					mobj_SerialPort = new SerialPort();
				}
                else if (mobj_SerialPort.IsOpen)
                {
                    mobj_SerialPort.Close();
                }

				mobj_SerialPort.PortName = portProps.PortName;
				mobj_SerialPort.BaudRate = portProps.BaudRate;
				mobj_SerialPort.StopBits = portProps.StopBits;
				mobj_SerialPort.DataBits = portProps.DataBits;
				mobj_SerialPort.Handshake = portProps.HandShake;
				mobj_SerialPort.Parity = portProps.Parity;
				mobj_SerialPort.NewLine = portProps.NewLine;
				mobj_SerialPort.ReadTimeout = portProps.ReadTimeout;
				mobj_SerialPort.WriteTimeout = portProps.WriteTimeout;
				mint_UnitAddr = portProps.UnitAddress;
			}	

			/// <summary>
			/// Sends a command to a pump (overload using default parameters)
			/// ack = Received
			/// WaitResp = false
			/// </summary>
			/// <param name="cmdStr">Command string to send</param>
			/// <returns>Empty string or string containing response from pump</returns>
			private string SendCommand(string cmdStr)
			{
				try
				{
					return SendCommand(cmdStr, enumIscoMsgAckCodes.Recvd, false);
				}
				catch (IscoExceptionWriteTimeout)
				{
					throw new IscoExceptionWriteTimeout();
				}
				catch (IscoExceptionReadTimeout)
				{
					throw new IscoExceptionReadTimeout();
				}
				catch (IscoExceptionMessageError ex1)
				{
					throw new IscoExceptionMessageError(ex1.Message);
				}
				catch (IscoExceptionUnauthroizedAccess ex2)
				{
					throw new IscoExceptionUnauthroizedAccess(ex2.Message);
				}
			}	

			/// <summary>
			/// Sends a command to a pump (Overload to allow waiting for a response from pump)
			/// Sets ack = Received
			/// </summary>
			/// <param name="cmdStr">Command string to send</param>
			/// <param name="waitResp">TRUE if a response is expected from pump; FALSE otherwise</param>
			/// <returns>Empty string or string containing response from pump</returns>
			private string SendCommand(string cmdStr, bool waitResp)
			{
				try
				{
					return SendCommand(cmdStr, enumIscoMsgAckCodes.Recvd, waitResp);
				}
				catch (IscoExceptionWriteTimeout)
				{
					throw new IscoExceptionWriteTimeout();
				}
				catch (IscoExceptionReadTimeout)
				{
					throw new IscoExceptionReadTimeout();
				}
				catch (IscoExceptionMessageError ex1)
				{
					throw new IscoExceptionMessageError(ex1.Message);
				}
				catch (IscoExceptionUnauthroizedAccess ex2)
				{
					throw new IscoExceptionUnauthroizedAccess(ex2.Message);
				}
			}	

			/// <summary>
			/// Sends a command to a pump (Overload allowing specification of all params)
			/// </summary>
			/// <param name="cmdStr">Command string to send</param>
			/// <param name="ack">Message acknowledgement code</param>
			/// <param name="waitResp">TRUE if a response is expected from pump; FALSE otherwise</param>
			/// <returns>Empty string or string containing response from pump</returns>
			private string SendCommand(string cmdStr, enumIscoMsgAckCodes ack, bool waitResp)
			{
				string errMsg;

				// If the serial port isn't open, then open it
				if (!mobj_SerialPort.IsOpen)
				{
					try
					{
						mobj_SerialPort.Open();
					}
					catch (UnauthorizedAccessException ex)
					{
						throw new IscoExceptionUnauthroizedAccess("Could not access the COM port: " + ex.Message);
					}
				}

				string cmd = BuildCtrlMsgFrame(0, mint_UnitAddr, cmdStr, ack);

				// Send the command
				try
				{
#if DACTEST
					LogMessage("SEND: " + cmd);	// DAC testing
#endif
					mobj_SerialPort.WriteLine("\r" + cmd); // DASNET messages are preceded by CR to ensure controller is listening
				}
				catch (TimeoutException)
				{
					throw new IscoExceptionWriteTimeout();
				}
				catch (UnauthorizedAccessException ex)
				{
					errMsg = "Could not access the COM port: " + ex.Message;
					throw new IscoExceptionUnauthroizedAccess(errMsg);
				}

				// If we're supposed to wait for a response, then read it in
				string respStr = "";
				string msgIn;
				enumIscoMsgAckCodes ackIn = enumIscoMsgAckCodes.PumpComError;
				if (waitResp)
				{
					try
					{
						msgIn = mobj_SerialPort.ReadLine();
#if DACTEST
						LogMessage("RECV: " + msgIn);	// DAC testing
#endif
					}
					catch (TimeoutException)
					{
#if DACTEST
						LogMessage("Timeout Exception");
#endif
						throw new IscoExceptionReadTimeout();
					}
					catch (UnauthorizedAccessException ex)
					{
						errMsg = "Could not access the COM port: " + ex.Message;
#if DACTEST
						LogMessage("COM port access exception");	// DAC testing
#endif
						throw new IscoExceptionUnauthroizedAccess(errMsg);
					}

					// Process the received response
					bool result;

					try
					{
						result = ParseMsgFrame(msgIn, ref ackIn, ref respStr);
					}
					catch (IscoExceptionMessageError ex)
					{
						throw new IscoExceptionMessageError(ex.Message);
					}

					if (!result)
					{
						throw new IscoExceptionMessageError("ISCO message error: Bad checksum");
					}
					else if (ackIn == enumIscoMsgAckCodes.PumpBusy)
					{
						throw new IscoExceptionMessageError("ISCO message error: Pump unit busy");
					}
					else if (ackIn == enumIscoMsgAckCodes.PumpComError) 
						throw new IscoExceptionMessageError("ISCO message error: Pump unit error");
				}

				return respStr;
			}	

#if DACTEST
			/// <summary>
			/// Logs an ISCO message to a file (testing only)
			/// </summary>
			/// <param name="msg"></param>
			public void LogMessage(string msg)
			{
				System.IO.FileInfo fi = new System.IO.FileInfo(System.Windows.Forms.Application.ExecutablePath);
				string outFile = System.IO.Path.Combine(fi.DirectoryName, "Log.txt");

				string logTxt = DateTime.UtcNow.Subtract(new TimeSpan(8, 0 , 0)).ToString("MM/dd/yyyy HH:mm:ss.ff") + ", " + msg;

				using (System.IO.StreamWriter w = System.IO.File.AppendText(outFile))
				{
					w.WriteLine(logTxt);
					w.Flush();
					w.Close();
				}
			}	
#endif
        #endregion

            #region "Message handling methods"
            /// <summary>
			/// Sums the ASCII codes for all characters in a string
			/// </summary>
			/// <param name="inpStr">Input string</param>
			/// <returns>Sum of all ASCII codes for chars in string</returns>
			private int SumAsciiCodes(string inpStr)
			{
				int outSum = 0;

				// Convert the input string to an array of char
				char[] tmpCharArray = inpStr.ToCharArray();

				foreach (char tmpChar in tmpCharArray)
				{
					// Assuming all characters involved have ASCII values < 255, then ASCII value can be 
					//	obtained via a simple cast from char to int
					outSum += (int)tmpChar;
				}

				return outSum;
			}	

			/// <summary>
			/// Calculate the (weird) checksum per ISCO instructions
			/// </summary>
			/// <param name="inpStr">Input string</param>
			/// <returns>String version of hex checksum value</returns>
			private string ObtainChecksum(string inpStr)
			{
				// Too strange to explain - see ISCO docs
				int tmpInt = SumAsciiCodes(inpStr) % 256;
				if (tmpInt > 0) tmpInt = 256 - tmpInt;

				return tmpInt.ToString("X2");
			}	

			/// <summary>
			/// Verifies the string that will be sent to ISCO pump is a multiple of 256
			/// </summary>
			/// <param name="inpStr">Message text</param>
			/// <param name="inpCheckSum">Checksum (hex) for message text</param>
			/// <returns>TRUE if multiple of 256; FALSE otherwise</returns>
			private bool VerifyChecksum(string inpStr, string inpCheckSum)
			{
				int tmpInt = SumAsciiCodes(inpStr) + int.Parse(inpCheckSum, System.Globalization.NumberStyles.HexNumber);

				if ((tmpInt % 256) == 0)
				{
					return true;
				}
				else return false;
			}	

			/// <summary>
			/// Parses a message received from a pump
			/// </summary>
			/// <param name="inpMsg">String (frame) received in acknowledgement of a sent message</param>
			/// <param name="ack">Message acknowldgement code</param>
			/// <param name="msgStr">Message text</param>
			/// <returns>TRUE if valid checksum was received; FALSE otherwise</returns>
			private bool ParseMsgFrame(string inpMsg, ref enumIscoMsgAckCodes ack, ref string msg)
			{
				/* Message format is:
				 *  Acknowledgement (1 char)
				 *  Message source (1 char) or space if no message
				 *  Length of message text (2 hex digits; 00 = 256 chars) if message present
				 *  Message text (256 char max) if message present
				 *  Checksum (2 digits)
				 */

				try
				{
					int respStrLen = inpMsg.Length;

					// Extract the checksum portion of the response string
					string chkSumStr = inpMsg.Substring(respStrLen - 2, 2);
					string frameBodyStr = inpMsg.Substring(0, respStrLen - 2);

					// Test the checksum. If invalid, no point in going further
					if (!VerifyChecksum(frameBodyStr, chkSumStr)) return false;

					// Get the message acknowledgement
					string ackStr = frameBodyStr.Substring(0, 1);
					switch (ackStr)
					{
						case "R":
							ack = enumIscoMsgAckCodes.Recvd;
							break;
						case "E":
							ack = enumIscoMsgAckCodes.PumpComError;
							break;
						case "B":
							ack = enumIscoMsgAckCodes.PumpBusy;
							break;
						default:
							// Shouldn't ever get here. If we did, something is wrong
							return false;
					}

					string msgSource = frameBodyStr.Substring(1, 1);
					if (msgSource != " ")
					{
						// If the message source isn't a space, then we need to extract the message
						string msgLenStr = frameBodyStr.Substring(2, 2);
						int msgLenInt = int.Parse(msgLenStr, System.Globalization.NumberStyles.HexNumber);
						msg = frameBodyStr.Substring(4, msgLenInt);
						// Some messages end with a ",", so delete it
						if (msg.EndsWith(",")) msg = msg.Remove(msg.Length - 1, 1);
					}
					else msg = "";

					return true;
				}
				catch (Exception ex)
				{
					// There was a problem parsing the message
					string errMsg = "Exception parsing message from pump: " + ex.Message;
					throw new IscoExceptionMessageError(errMsg);
				}	
			}	

			/// <summary>
			/// Builds a message frame for sending to pump controller
			/// </summary>
			/// <param name="srce">Message source</param>
			/// <param name="dest">Pump controller will forward message to</param>
			/// <param name="msg">Message text</param>
			/// <param name="ack">Acknowledgement code</param>
			/// <returns>Complete message frame</returns>
			private string BuildCtrlMsgFrame(int srce, int dest, string msg, enumIscoMsgAckCodes ack)
			{
				/* Message structure:
				 * Destination unit address (1 char)
				 * Acknowledgement code, normally "R" for sending (1 char)
				 * Message source unit address, normally "0" for sending from control computer
				 *		If no message, this is a space, aka 0x20(1 char)
				 * Length of message in hex. Max length is 256, with 256 indicated as "00" (2 char)
				 * Message (256 char max)
				 * Checksum (2 char)
				*/

				string outFrame = dest.ToString();

				switch (ack)
				{
					case enumIscoMsgAckCodes.Recvd:
						outFrame += "R";
						break;
					case enumIscoMsgAckCodes.PumpBusy:
						outFrame += "B";
						break;
					case enumIscoMsgAckCodes.PumpComError:
						outFrame += "E";
						break;
					default:
						// Should never get here
						return "";
				}

				int msgLength = msg.Length;

				if (msgLength == 0)
				{
					outFrame += " ";
				}
				else
				{
					string msgLengthStr = (msgLength % 256).ToString("X2");
					outFrame += srce.ToString() + msgLengthStr + msg;
				}

				return outFrame + ObtainChecksum(outFrame);
			}	                                  

			/// <summary>
			/// Converts the status code read from the pump to an operation status enum
			/// </summary>
			/// <param name="inpStr">Input status code</param>
			/// <returns>Operation status enum</returns>
			private enumIscoOperationStatus ConvertStatusCodeToEnum(string inpStr)
			{
				enumIscoOperationStatus outVal = enumIscoOperationStatus.Hold;

				switch (inpStr)
				{
					case "S":
						outVal = enumIscoOperationStatus.Stopped;
						break;
					case "R":
						outVal = enumIscoOperationStatus.Running;
						break;
					case "F":
						outVal = enumIscoOperationStatus.Refilling;
						break;
					case "H":
						outVal = enumIscoOperationStatus.Hold;
						break;
					case "E":
						outVal = enumIscoOperationStatus.Equilibrating;
						break;
					default:
						// Shouldn't ever get here
						throw new IscoException("Invalid operation status code: " + inpStr);
				}

				return outVal;
			}	

			/// <summary>
			/// Converts the control type code read from the pump to a control type enum
			/// </summary>
			/// <param name="inpStr">Input control type code</param>
			/// <returns>Control type enum</returns>
			private enumIscoControlMode ConvertControlModeCodeToEnum(string inpStr)
			{
				enumIscoControlMode outVal = enumIscoControlMode.Local;

				switch (inpStr)
				{
					case "L":
						outVal = enumIscoControlMode.Local;
						break;
					case "R":
						outVal = enumIscoControlMode.Remote;
						break;
					case "E":
						outVal = enumIscoControlMode.External;
						break;
					default:
						// Should never get here
						throw new IscoException("Invalid control type code received: " + inpStr);
				}

				return outVal;
			}	

			/// <summary>
			/// Converts the problem code read from the pump to a problem type enum
			/// </summary>
			/// <param name="inpStr"></param>
			/// <returns></returns>
			private enumIscoProblemStatus ConvertProblemCodeToEnum(string inpStr)
			{
				enumIscoProblemStatus outVal = enumIscoProblemStatus.None;

				switch (inpStr)
				{
					case "-":
						outVal = enumIscoProblemStatus.None;
						break;
					case "E":
						outVal = enumIscoProblemStatus.CylinderEmpty;
						break;
					case "B":
						outVal = enumIscoProblemStatus.CylinderBottom;
						break;
					case "O":
						outVal = enumIscoProblemStatus.OverPressure;
						break;
					case "U":
						outVal = enumIscoProblemStatus.UnderPressure;
						break;
					case "M":
						outVal = enumIscoProblemStatus.MotorFailure;
						break;
					default:
						// Should never get here
						throw new IscoException("Invalid problem code received: " + inpStr);
				}

				return outVal;
			}	

			/// <summary>
			/// Parses reply from a status request
			/// </summary>
			/// <param name="inpMsg">Status request reply string</param>
			/// <returns>Array containing status for all 3 pumps</returns>
			private classPumpIscoData[] ParseStatusMessage(string inpMsg)
			{
				// Set up return array
				classPumpIscoData[] retArray = { new classPumpIscoData(), new classPumpIscoData(), new classPumpIscoData() };

				// Wrap the whole mess in a try/catch just in case something goes wrong
				try
				{
					// Strip off unused prefix
					int signPos = inpMsg.IndexOf("=");
					string pumpData = inpMsg.Substring(signPos + 1, inpMsg.Length - signPos -1);

					// Split the input message into fields (aka tokens, in original VB6 code)
					string[] tokens = pumpData.Split(new char[] { ',' });

					/* ------------------------------------------------------------------------
					 * Token structure:
					 *		Array elements 0 - 2: Pump A, B, and C pressure
					 *		Array elements 3 - 7: Analog inputs 1-5
					 *		Array element 8: Digital inputs 1-8
					 *		Array element 9: Flow rate pump A
					 *		Array element 10: Volume pump A
					 *		Array element 11: Operation status pump A
					 *		Array element 12: Control status pump A
					 *		Array element 13: Problem status pump A
					 *		Array elements 14 - 18: Repeat of elements 9 - 13 for pump B
					 *		Array elements 19 - 23: Repeat of elements 9 - 13 for pump C
					 *		Array element 24: System flow rate
					 *		Array element 25: System pressure
					 *		Array element 26: Total volume
					*/

					// Parse the field data and store
					int tokenIndx = 0;
					for (int pump = 0; pump < mint_PumpCount; pump++)
					{
						// Sample time
						retArray[pump].PointTime = DateTime.UtcNow.Subtract(new TimeSpan(8, 0, 0));

						// Pressure
						retArray[pump].Pressure = classIscoConversions.ConvertPressFromString(tokens[pump]);

						// Flow
						tokenIndx = 9 + (pump * 5);
						retArray[pump].Flow = classIscoConversions.ConvertFlowFromString(tokens[tokenIndx]);

						// Volume
						retArray[pump].Volume = classIscoConversions.ConvertVolumeFromString(tokens[tokenIndx + 1]);

						// Operaton status
						retArray[pump].OperationStatus = ConvertStatusCodeToEnum(tokens[tokenIndx + 2]);

						// Control mode
						retArray[pump].ControlMode = ConvertControlModeCodeToEnum(tokens[tokenIndx + 3]);

						// Problem status
						retArray[pump].ProblemStatus = ConvertProblemCodeToEnum(tokens[tokenIndx + 4]);
					}

					return retArray;
				}
				catch (Exception ex)
				{
					string msg = "Exception parsing status string";
					string notifyStr = 
						classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.MessageParseError.ToString());
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(msg, ex,
																enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
																notifyStr);
					if (Error != null) Error(this, args);
					return null;
				}
			}	

			/// <summary>
			/// Parses a mesaage string to get the flow, pressure, or volume value
			/// </summary>
			/// <param name="inpMsg">Input message</param>
			/// <returns>Numeric value in string, or -1000 if there's a problem</returns>
			private double ParseNumericValue(string inpMsg)
			{
				try
				{
					int signPos = inpMsg.IndexOf("=");
					string valueStr = inpMsg.Substring(signPos + 1, inpMsg.Length - signPos - 1);
					return double.Parse(valueStr);
				}
				catch (Exception ex)
				{
					string msg = "Exception parsing numeric field";
					string notifyStr =
						classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.MessageParseError.ToString());
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(msg, ex,
																enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
																notifyStr);
					if (Error != null) Error(this, args);
					return -1000;
				}
			}	

			/// <summary>
			/// Parses the response to a RANGE command
			/// </summary>
			/// <param name="inpMsg">Input message</param>
			/// <returns>Object containing instrument range data</returns>
			private classPumpIscoRangeData ParseRangeMessage(string inpMsg)
			{
				// Split the input message at the comma's
				string[] rangeArray = inpMsg.Split(new char[] { ',' });

				// Verify correct number of arguments
				if (rangeArray.Length != 4)
				{
					string msg = "Invalid field count in RANGE response string";
					string notifyStr =
						classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.MessageParseError.ToString());
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(msg, null,
																enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
																notifyStr);
					if (Error != null) Error(this, args);
					return null;
				}

				int signIndx = 0;
				string parseStr = "";
				classPumpIscoRangeData retData = new classPumpIscoRangeData();
				try
				{
					// Pressure field
					signIndx = rangeArray[0].IndexOf("=");
					parseStr = rangeArray[0].Substring(signIndx, rangeArray[0].Length - signIndx);
					retData.MaxPressure = ParseNumericValue(parseStr);

					// Flow field
					signIndx = rangeArray[1].IndexOf("=");
					parseStr = rangeArray[1].Substring(signIndx, rangeArray[1].Length - signIndx);
					retData.MaxFlowRate = ParseNumericValue(parseStr);

					// Refill rate field
					signIndx = rangeArray[2].IndexOf("=");
					parseStr = rangeArray[2].Substring(signIndx, rangeArray[2].Length - signIndx);
					retData.MaxRefillRate = ParseNumericValue(parseStr);

					// Volume field
					signIndx = rangeArray[3].IndexOf("=");
					parseStr = rangeArray[3].Substring(signIndx, rangeArray[3].Length - signIndx);
					retData.MaxVolume = ParseNumericValue(parseStr);

					return retData;
				}
				catch (Exception ex)
				{
					string msg = "Exception parsing RANGE string";
					string notifyStr =
						classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.MessageParseError.ToString());
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(msg, ex,
																enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
																notifyStr);
					if (Error != null) Error(this, args);
					return null;
				}
			}	

			/// <summary>
			/// Parses the response to a "UNITS" command and sets unit conversion settings
			/// </summary>
			/// <param name="inpMsg">Message received from pump</param>
			private bool ParseUnitsMessage(string inpMsg)
			{
				// Split the message
				string[] unitsArray = inpMsg.Split(new char[] { ',' });

				// Verify correct number of arguments
				if (unitsArray.Length != 2)
				{
					classIscoConversions.PressUnits = enumIscoPressureUnits.error;
					classIscoConversions.FlowUnits = enumIscoFlowUnits.error;

					string msg = "Invalid field count in UNITSA response string";
					string notifyStr =
						classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.MessageParseError.ToString());
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(msg, null,
																enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
																notifyStr);
					if (Error != null) Error(this, args);
					return false;
				}

				int signIndx = 0;
				string tmpStr = "";
				try
				{
					// Pressure field
					signIndx = unitsArray[0].IndexOf("=");
					tmpStr = unitsArray[0].Substring(signIndx + 1, unitsArray[0].Length - signIndx - 1);
					classIscoConversions.PressUnits = classIscoConversions.ConvertPressStrToEnum(tmpStr);

					// Flow field
					signIndx = unitsArray[1].IndexOf("=");
					tmpStr = unitsArray[1].Substring(signIndx + 1, unitsArray[1].Length - signIndx - 1);
					classIscoConversions.FlowUnits = classIscoConversions.ConvertFlowStrToEnum(tmpStr);
				}
				catch (Exception ex)
				{
					string msg = "Exception parsing UNITSA string";
					string notifyStr =
						classIscoErrorNotifications.GetNotificationString(enumIscoProblemStatus.MessageParseError.ToString());
					classDeviceErrorEventArgs args = new classDeviceErrorEventArgs(msg, ex,
																enumDeviceErrorStatus.ErrorAffectsAllColumns, this,
																notifyStr);
					if (Error != null) Error(this, args);
					return false;
				}

				return true;
			}	
		#endregion

		#region "IDevice Properties"
			/// <summary>
			/// Device name
			/// </summary>
			public string Name
			{
				get { return mstring_Name; }
				set 
                {
                    if (value != null && value != mstring_Name)
                    {
                        mstring_Name = value;
                        if (DeviceSaveRequired != null)
                        {
                            DeviceSaveRequired(this, null);
                        }
                    }
                }
			}

			/// <summary>
			/// Device version
			/// </summary>
			public string Version
			{
				get { return mstring_Version; }
				set { mstring_Version = value; }
			}

			/// <summary>
			/// Device status
			/// </summary>
			public enumDeviceStatus Status
			{
				get { return menum_Status; }
				set { menum_Status = value; }
			}

			public System.Threading.ManualResetEvent AbortEvent
			{
				get;
                set;
			}

			/// <summary>
			/// Error type for device
			/// </summary>
			public enumDeviceErrorStatus ErrorType { get; set; }

			/// <summary>
			/// Device type for component
			/// </summary>
			public enumDeviceType DeviceType
			{
				get { return enumDeviceType.Component; }
			}
		#endregion

		#region "IDevice Methods"
			public bool Initialize(ref string errorMessage)
			{
				string notifyStr =
					classIscoStatusNotifications.GetNotificationString(enumIscoOperationStatus.Initializing.ToString());
                classDeviceStatusEventArgs args = new classDeviceStatusEventArgs(enumDeviceStatus.NotInitialized, notifyStr, this);
				if (StatusUpdate != null) StatusUpdate(this, args);

				if (!InitDevice(ref errorMessage))
				{					
					return false;
				}
				else
				{
					notifyStr = 
						classIscoStatusNotifications.GetNotificationString(enumIscoOperationStatus.InitSucceeded.ToString());
                    args = new classDeviceStatusEventArgs(enumDeviceStatus.Initialized, notifyStr, this);
					if (StatusUpdate != null) StatusUpdate(this, args);
					return true;
				}
			}	

			public bool Shutdown()
			{
				Disconnect();
				return true;
			}	

			public void SaveDeviceSettings(XmlElement deviceNode)
			{
				deviceNode.SetAttribute("Name", Name);
				deviceNode.SetAttribute("Emulation", Emulation.ToString());
				deviceNode.SetAttribute("PumpCount", PumpCount.ToString());
				deviceNode.SetAttribute("UnitAddress", UnitAddress.ToString());
				deviceNode.SetAttribute("PortName", PortName);
				deviceNode.SetAttribute("BaudRate", BaudRate.ToString());
				deviceNode.SetAttribute("ReadTimeout", ReadTimeout.ToString());
				deviceNode.SetAttribute("WriteTimeout", WriteTimeout.ToString());
			}	

			public void LoadDeviceSettings(XmlElement deviceNode)
			{
				XmlDocument doc = new XmlDocument();
				XmlElement root = doc.CreateElement("Root");
				doc.AppendChild(root);

				XmlNode importedNode = doc.ImportNode(deviceNode, true);
				doc.DocumentElement.AppendChild(importedNode);

				string xPath;
				XmlAttribute tmpAttribute;

				xPath = "//@Name";
				tmpAttribute = (XmlAttribute)doc.SelectSingleNode(xPath);
				if (tmpAttribute != null)
				{
					Name = tmpAttribute.Value;
				}

				xPath = "//@Emulation";
				tmpAttribute = (XmlAttribute)doc.SelectSingleNode(xPath);
				if (tmpAttribute != null)
				{
					Emulation = bool.Parse(tmpAttribute.Value);
				}

				xPath = "//@PumpCount";
				tmpAttribute = (XmlAttribute)doc.SelectSingleNode(xPath);
				if (tmpAttribute != null)
				{
					PumpCount = int.Parse(tmpAttribute.Value);
				}

				xPath = "//@UnitAddress";
				tmpAttribute = (XmlAttribute)doc.SelectSingleNode(xPath);
				if (tmpAttribute != null)
				{
					UnitAddress = int.Parse(tmpAttribute.Value);
				}

				xPath = "//@PortName";
				tmpAttribute = (XmlAttribute)doc.SelectSingleNode(xPath);
				if (tmpAttribute != null)
				{
					PortName = tmpAttribute.Value;
				}

				xPath = "//@BaudRate";
				tmpAttribute = (XmlAttribute)doc.SelectSingleNode(xPath);
				if (tmpAttribute != null)
				{
					BaudRate = int.Parse(tmpAttribute.Value);
				}

				xPath = "//@ReadTimeout";
				tmpAttribute = (XmlAttribute)doc.SelectSingleNode(xPath);
				if (tmpAttribute != null)
				{
					ReadTimeout = int.Parse(tmpAttribute.Value);
				}

				xPath = "//@WriteTimeout";
				tmpAttribute = (XmlAttribute)doc.SelectSingleNode(xPath);
				if (tmpAttribute != null)
				{
					WriteTimeout = int.Parse(tmpAttribute.Value);
				}
			}	

			public void RegiserDataProvider(string key, DelegateDeviceHasData remoteMethod)
            {
			}

			public void UnRegiserDataProvider(string key, DelegateDeviceHasData remoteMethod)
            {
			}

			public void WritePerformanceData(string directoryPath, string methodName, object[] parameters)
            {
			}

			public List<string> GetStatusNotificationList()
			{
				return classIscoStatusNotifications.GetNotificationListStrings();
			}	

			public List<string> GetErrorNotificationList()
			{
				return classIscoErrorNotifications.GetNotificationListStrings();
			}	
		#endregion

		#region "IDevice Events"
			public event EventHandler<classDeviceStatusEventArgs> StatusUpdate;

			public event EventHandler<classDeviceErrorEventArgs> Error;

			public event EventHandler DeviceSaveRequired;
		#endregion

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Creates a class that is used for monitoring the device.
        /// </summary>
        /// <returns></returns>
        public classMonitoringComponent GetHealthData()
        {            
            classMonitoringComponent component = new classMonitoringComponent();
            component.Status    = Status.ToString();
            component.Message   = "";
            component.Name      = Name;
            component.Type      = "Pump";
            component.Model     = "";

            classMonitoringMeasurementScalar measurementBaudRate = new classMonitoringMeasurementScalar();
            measurementBaudRate.Description = "Baud Rate of the communication to the ISCO pump";
            measurementBaudRate.Name         = "Baud Rate";
            measurementBaudRate.Type         = "integer";
            measurementBaudRate.Units        = "";
            measurementBaudRate.Value        = this.BaudRate.ToString();
            component.MeasurementData.Add(measurementBaudRate);
            
            classMonitoringMeasurementScalar measurementControlMode = new classMonitoringMeasurementScalar();
            measurementControlMode.Description      = "What mode the pumps are in for control.";
            measurementControlMode.Name             = "Control Mode";
            measurementControlMode.Type             = "string";
            measurementControlMode.Units            = "";
            measurementControlMode.Value            = this.ControlMode.ToString();
            component.MeasurementData.Add(measurementControlMode);
                                    
            classMonitoringMeasurementScalar measurementOperationMode = new classMonitoringMeasurementScalar();
            measurementOperationMode.Description      = "What mode the pumps are in for Operation.";
            measurementOperationMode.Name             = "Operation Mode";
            measurementOperationMode.Type             = "string";
            measurementOperationMode.Units            = "";
            measurementOperationMode.Value            = this.OperationMode.ToString();
            component.MeasurementData.Add(measurementOperationMode);

            classMonitoringMeasurementScalar measurementPumpCount = new classMonitoringMeasurementScalar();
            measurementPumpCount.Description    = "The number of pumps set.";
            measurementPumpCount.Name           = "PumpCount";
            measurementPumpCount.Type           = "integer";
            measurementPumpCount.Units          = "";
            measurementPumpCount.Value          = this.PumpCount.ToString();
            component.MeasurementData.Add(measurementPumpCount);

            classMonitoringMeasurementScalar measurementAddress = new classMonitoringMeasurementScalar();
            measurementAddress.Description       = "The address of the pump.";
            measurementAddress.Name              = "Address";
            measurementAddress.Type              = "string";
            measurementAddress.Units             = "";
            measurementAddress.Value             = this.UnitAddress.ToString();
            component.MeasurementData.Add(measurementAddress);
            
            classMonitoringMeasurementScalar measurementSendTimeout = new classMonitoringMeasurementScalar();
            measurementSendTimeout.Description  = "The timeout set for writing a message.";
            measurementSendTimeout.Name         = "Write Timeout";
            measurementSendTimeout.Type         = "int";
            measurementSendTimeout.Units        = "";
            measurementSendTimeout.Value        = this.WriteTimeout.ToString();
            component.MeasurementData.Add(measurementSendTimeout);
            
            classMonitoringMeasurementScalar measurementReceiveTimeout = new classMonitoringMeasurementScalar();
            measurementReceiveTimeout.Description       = "The timeout set for reading a message.";
            measurementReceiveTimeout.Name              = "Read Timeout";
            measurementReceiveTimeout.Type              = "int";
            measurementReceiveTimeout.Units             = "";
            measurementReceiveTimeout.Value             = this.ReadTimeout.ToString();
            component.MeasurementData.Add(measurementReceiveTimeout);

            classMonitoringMeasurementScalar port = new classMonitoringMeasurementScalar();
            port.Description    = "The port used to communicate with.";
            port.Name           = "Port";
            port.Type           = "string";
            port.Units          = "";
            port.Value          = this.PortName.ToString();
            component.MeasurementData.Add(port);
            
            return component;
        }  
	}	
}	// End namespace
