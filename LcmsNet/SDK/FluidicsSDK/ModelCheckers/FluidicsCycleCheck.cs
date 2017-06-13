﻿/*********************************************************************************************************
 * Written by Christopher Walters for U.S. Department of Energy
 * Pacific Northwest National Laboratory, Richland, WA
 * Copyright 2014 Battle Memorial Institute
 *
 *********************************************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using LcmsNetDataClasses;
using LcmsNetSDK;
using FluidicsSDK.Base;
using FluidicsSDK.Managers;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace FluidicsSDK.ModelCheckers
{
    public class FluidicsCycleCheck:IFluidicsModelChecker
    {
        private readonly List<string> m_notifications;

        private const string cycle = "Cycle in physical configuration";

        public FluidicsCycleCheck()
        {
            Name = "Cyclical Path";
            IsEnabled = true;
            m_notifications = new List<string> { cycle };
        }

        private string name;
        private bool isEnabled;
        private ModelStatusCategory category;

        public string Name
        {
            get { return name; }
            set { this.RaiseAndSetIfChanged(ref name, value); }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { this.RaiseAndSetIfChanged(ref isEnabled, value); }
        }

        public ModelStatusCategory Category
        {
            get { return category; }
            set { this.RaiseAndSetIfChanged(ref category, value); }
        }

        public IEnumerable<ModelStatus> CheckModel()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var status = new List<ModelStatus>();
            var sources = PortManager.GetPortManager.Ports.FindAll(x => x.Source);
            foreach (var source in sources)
            {
                var pathTaken = new List<Connection>();
                var cycleFound = FindCycles(source, new List<Port>(), pathTaken);
                if (cycleFound)
                {
                    foreach(var connection in pathTaken)
                    {
                        connection.Color = Color.Red;
                    }
                    status.Add(new ModelStatus("Cycle found", "Cycle found in path", Category, null, TimeKeeper.Instance.Now.ToString(CultureInfo.InvariantCulture), null, source.ParentDevice.IDevice));
                    if (StatusUpdate != null)
                    {
                        const string message = "Cycle in physical configuration";
                        var deviceName = source.ParentDevice.IDevice.Name;
                        StatusUpdate(this, new LcmsNetDataClasses.Devices.classDeviceStatusEventArgs(LcmsNetDataClasses.Devices.enumDeviceStatus.Initialized, message, deviceName, this));
                    }
                }
            }
            watch.Stop();
            //LcmsNetDataClasses.Logging.classApplicationLogger.LogMessage(LcmsNetDataClasses.Logging.classApplicationLogger.CONST_STATUS_LEVEL_CRITICAL, "Cycle check time elapsed: " + watch.Elapsed.TotalMilliseconds.ToString() + "ms");
            return status;
        }

        //Uses a depth-first search to find cycles.
        private bool FindCycles(Port startingSource, ICollection<Port> visitedPorts, ICollection<Connection> pathTaken, Connection PrevConnection=null)
        {
            visitedPorts.Add(startingSource);
            // check where every connection from the starting source goes
            foreach(var conn in startingSource.Connections)
            {
                var otherEnd = conn.FindOppositeEndOfConnection(startingSource);
                if (PrevConnection == null || conn.ID != PrevConnection.ID) // if conn.ID is the same as PrevConnection.ID, ignore it, since that's the connection we just traveled, and we don't want to go backwards as this would be detected as a cycle, when in reality it is not.
                {
                    pathTaken.Add(conn);
                    // If at any point we find a port we've already been to, there is a cycle in the graph,
                    // or in other words, we have connections that lead in a "circular" path back to a place we've already been
                    if (visitedPorts.Contains(otherEnd))
                    {
                        // cycleFound = true;
                        return true;
                    }

                    var cycleFound = FindCycles(otherEnd, visitedPorts, pathTaken, conn);
                    if (cycleFound) {
                        return true;
                    }
                }
            }
            // cycleFound is false
            return false;
        }


        public List<string> GetStatusNotificationList()
        {
            return m_notifications;
        }

        public List<string> GetErrorNotificationList()
        {
            return new List<string>();
        }

        public event EventHandler<LcmsNetDataClasses.Devices.classDeviceStatusEventArgs> StatusUpdate;

        public event EventHandler<LcmsNetDataClasses.Devices.classDeviceErrorEventArgs> Error
        {
            add { }
            remove { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}