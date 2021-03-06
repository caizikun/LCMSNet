﻿using System;
using System.Collections.Generic;
using LcmsNetSDK;

namespace LcmsNetDataClasses
{
    [Obsolete("Interface deprecated. Use a direct reference to classDBTools.cs")]
    public interface IDmsTools
    {
        string ErrMsg { get; set; }
        string DMSVersion { get; }

        /// <summary>
        /// Determines if lcmsnet should attempt to force dmsvalidation, and refuse to queue samples if validator not found.
        /// </summary>
        bool ForceValidation { get; }

        void GetCartListFromDMS();
        void GetColumnListFromDMS();
        void GetDatasetTypeListFromDMS();
        void GetEntireColumnListListFromDMS();
        void GetExperimentListFromDMS();
        void GetInstrumentListFromDMS();

        Dictionary<int, int> GetMRMFileListFromDMS(int MinID, int MaxID);

        void GetMRMFilesFromDMS(string FileIndxList, ref List<classMRMFileData> fileData);

        void GetProposalUsers();

        List<classSampleData> GetSamplesFromDMS(classSampleQueryData queryData);

        void GetSepTypeListFromDMS();
        void GetUserListFromDMS();
        void LoadCacheFromDMS();
        void LoadCacheFromDMS(bool shouldLoadExperiment);
        bool UpdateDMSCartAssignment(string requestList, string cartName, string cartConfigName, bool updateMode);

        event LcmsNetSDK.ProgressEventHandler ProgressEvent;

        void OnProgressUpdate(ProgressEventArgs e);

    }
}