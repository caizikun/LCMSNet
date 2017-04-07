﻿//*********************************************************************************************************
// Written by Dave Clark for the US Department of Energy
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2009, Battelle Memorial Institute
// Created 01/07/2009
//
// Updates:
// - 02/04/2009 (DAC) - Removed well plate and well, which were xferred to PAL data class
// - 02/10/2009 (DAC) - Added method for obtaining current values of all properties
// - 02/17/2009 (DAC) - Changed to inherited class; moved cache ops to base class
// - 03/09/2009 (DAC) - Added comment property
// - 04/02/2009 (DAC) - Added field for MRM file index
// - 08/11/2009 (DAC) - Added field for run request batch number
//
//*********************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LcmsNetSDK;

namespace LcmsNetDataClasses
{
    /// <summary>
    /// Class file for handling data used in communication with DMS
    /// </summary>
    [Serializable]
    public class classDMSData : classDataClassBase, INotifyPropertyChangedExt
    {
        public classDMSData()
        {
            Batch = -1;
            Block = -1;
            CartName = "";
            CartConfigName = "";
            Comment = "";
            DatasetName = "";
            DatasetType = "";
            Experiment = "";
            MRMFileID = -1;
            ProposalID = "";
            RequestID = 0;
            RequestName = "";
            RunOrder = -1;
            SelectedToRun = false;
            UsageType = "";
            UserList = "";
        }

        #region Property Backing Variables

        private string requestName;
        private string datasetName;
        private string datasetType;
        private int block;
        private int runOrder;
        private int batch;

        #endregion

        #region "Properties"

        /// <summary>
        /// Flag for determining if request from DMS has been selected for running
        /// </summary>
        public bool SelectedToRun { get; set; }

        /// <summary>
        /// Name of request in DMS. Becomes sample name in LCMS and forms part
        /// of dataset name sample after run
        /// </summary>
        public string RequestName
        {
            get { return requestName; }
            set
            {
                if (this.RaiseAndSetIfChangedRetBool(ref requestName, value))
                {
                    if (string.IsNullOrEmpty(DatasetName))
                    {
                        DatasetName = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the sample after editing the request name.
        /// </summary>
        public string DatasetName
        {
            get { return datasetName; }
            set { this.RaiseAndSetIfChanged(ref datasetName, value); }
        }

        /// <summary>
        /// Numeric ID of request in DMS
        /// </summary>
        public int RequestID { get; set; }

        /// <summary>
        /// Experiment name
        /// </summary>
        public string Experiment { get; set; }

        /// <summary>
        /// Dataset type (ie, HMS-MSn, HMS, etc)
        /// </summary>
        public string DatasetType
        {
            get { return datasetType; }
            set { this.RaiseAndSetIfChanged(ref datasetType, value); }
        }

        /// <summary>
        /// EMSL usage type
        /// </summary>
        public string UsageType { get; set; }

        /// <summary>
        /// EUS sser proposal ID
        /// </summary>
        public string ProposalID { get; set; }

        /// <summary>
        /// EUS user list
        /// </summary>
        public string UserList { get; set; }

        /// <summary>
        /// Name of cart used for sample run
        /// </summary>
        public string CartName { get; set; }

        /// <summary>
        /// Name of cart configuration used for sample run
        /// </summary>
        public string CartConfigName { get; set; }

        /// <summary>
        /// Comment field
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// File ID for locating MRM file to download
        /// </summary>
        public int MRMFileID { get; set; }

        /// <summary>
        /// Block ID for blocking/randomizing
        /// </summary>
        public int Block
        {
            get { return block; }
            set { this.RaiseAndSetIfChanged(ref block, value); }
        }

        /// <summary>
        /// Run order for blocking/randomizing
        /// </summary>
        public int RunOrder
        {
            get { return runOrder; }
            set { this.RaiseAndSetIfChanged(ref runOrder, value); }
        }

        /// <summary>
        /// Batch number for blocking/randomizing
        /// </summary>
        public int Batch
        {
            get { return batch; }
            set { this.RaiseAndSetIfChanged(ref batch, value); }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(DatasetName))
            {
                if (string.Equals(RequestName, DatasetName))
                {
                    return "Request " + RequestName;
                }

                return "Dataset " + DatasetName;
            }

            if (!string.IsNullOrWhiteSpace(Experiment))
                return "Experiment " + Experiment;

            if (!string.IsNullOrWhiteSpace(RequestName))
                return "Request " + RequestName;

            return "RequestID " + RequestID;
        }

        #endregion

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
