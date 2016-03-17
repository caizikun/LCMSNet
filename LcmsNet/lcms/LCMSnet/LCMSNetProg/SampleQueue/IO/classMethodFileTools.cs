﻿//*********************************************************************************************************
// Written by Dave Clark, Brian LaMarche, Christopher Walters for the US Department of Energy
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2010, Battelle Memorial Institute
// Created 03/01/2010
//
// Last modified 09/16/2014
//*********************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Logging;
using LcmsNet.Method;
using LcmsNetDataClasses.Data;

namespace LcmsNet.SampleQueue.IO
{
    /// <summary>
    /// Methods for copying trigger files and other data to DMS.
    /// </summary>
    public class classMethodFileTools : IMethodWriter
    {
        public classMethodFileTools()
        {
        }

        #region Constants

        private const string METHOD_FOLDER_NAME = "MethodFiles";
        private const string LOCAL_METHOD_FOLDER_NAME = "CompletedMethods";

        #endregion

        #region Methods

        /// <summary>
        /// Writes information generated by an incomplete sample run.
        /// </summary>
        /// <param name="sample"></param>
        public static void WriteIncompleteMethodFiles(classSampleData sample)
        {
            if (sample == null)
            {
                classApplicationLogger.LogError(0, "WriteIncompleteMethodFiles: No sample specified");
                return;
            }

            // Exit if method folder creation disabled
            if (!bool.Parse(classLCMSSettings.GetParameter("CreateMethodFolders")))
            {
                string msg = "WriteMethodFiles: Sample " + sample.DmsData.DatasetName +
                             ", Method folder creation disabled";
                classApplicationLogger.LogMessage(0, msg);
                return;
            }

            // Make the method transfer folder
            string localFolder = MakeLocalMethodFolder(sample);
            if (localFolder == "")
            {
                string message = "Could not create a folder for the output method data for: " + sample.LCMethod.Name;
                classApplicationLogger.LogError(0, message);
                return;
            }

            // ----------------------------------------------------------------------------------------------------
            // Write the LC Method file
            // ----------------------------------------------------------------------------------------------------
            string methodFileName = classSampleData.GetTriggerFileName(sample, ".incompleteLcmethod");
            string lcMethodFileNamePath = Path.Combine(localFolder, methodFileName);
            classLCMethodWriter lcWriter = new classLCMethodWriter();
            try
            {
                lcWriter.WriteMethod(lcMethodFileNamePath, sample.LCMethod);
            }
            catch (Exception ex)
            {
                string msg = "Could not write the LC Method file for: " + sample.LCMethod.Name;
                classApplicationLogger.LogError(0, msg, ex, sample);
                return;
            }
            finally
            {
                lcWriter = null;
            }
        }

        /// <summary>
        /// Creates a folder for the methods associated with a sample, then copies it to a bionet transfer folder
        /// </summary>
        /// <param name="sample"></param>
        public static void WriteMethodFiles(classSampleData sample)
        {
            if (sample == null)
            {
                classApplicationLogger.LogError(0, "WriteMethodFiles: No sample specified");
                return;
            }

            // Exit if method folder creation disabled
            if (!bool.Parse(classLCMSSettings.GetParameter("CreateMethodFolders")))
            {
                string msg = "WriteMethodFiles: Sample " + sample.DmsData.DatasetName +
                             ", Method folder creation disabled";
                classApplicationLogger.LogMessage(0, msg);
                return;
            }

            // Make the method transfer folder
            string localFolder = MakeLocalMethodFolder(sample);
            if (localFolder == "")
            {
                string message = "Could not create a folder for the output method data for: " + sample.LCMethod.Name;
                classApplicationLogger.LogError(0, message);
                return;
            }

            // ----------------------------------------------------------------------------------------------------
            // Write the LC Method file
            // ----------------------------------------------------------------------------------------------------
            classLCMethodWriter methodWriter = new classLCMethodWriter();
            string methodFileName = classSampleData.GetTriggerFileName(sample, ".lcmethod");
            string lcMethodFileNamePath = Path.Combine(localFolder, methodFileName);
            classLCMethodWriter lcWriter = new classLCMethodWriter();
            try
            {
                lcWriter.WriteMethod(lcMethodFileNamePath, sample.LCMethod);
            }
            catch (Exception ex)
            {
                string msg = "Could not write the LC Method file for: " + sample.LCMethod.Name;
                classApplicationLogger.LogError(0, msg, ex, sample);
                return;
            }
            finally
            {
                methodWriter = null;
            }

            // ----------------------------------------------------------------------------------------------------
            // Now try to write all of the event performance data (e.g. pump methods etc.) to the server
            // ----------------------------------------------------------------------------------------------------
            //string performanceFile =
            //try
            //{
            //    sample.LCMethod.WritePerformanceData(localFolder);
            //}
            //catch (Exception ex)
            //{
            //    string message = "Could not write the performance data files.";
            //    classApplicationLogger.LogError(0, message, ex, sample);
            //    return;
            //}

            // ----------------------------------------------------------------------------------------------------
            // Exit if method folder copy disabled
            // ----------------------------------------------------------------------------------------------------
            bool shouldCopyFolder = bool.Parse(classLCMSSettings.GetParameter("CopyMethodFolders"));
            if (!shouldCopyFolder)
            {
                string msg = "The method data was not copied to the server for: " + sample.DmsData.DatasetName +
                             ".  the method folder copy is disabled";
                classApplicationLogger.LogMessage(0, msg);
                return;
            }

            // Move the collected method data to the xfer server
            string remoteTargetFolder = CreateRemoteFolderPath();
            try
            {
                bool remoteFolderExists = Directory.Exists(remoteTargetFolder);
                if (!remoteFolderExists)
                {
                    Directory.CreateDirectory(remoteTargetFolder);
                }

                string localName = Path.GetFileName(lcMethodFileNamePath);
                File.Copy(lcMethodFileNamePath, Path.Combine(remoteTargetFolder, localName), true);
                File.Delete(lcMethodFileNamePath);

                string message = string.Format("Method information for {0} was copied to {1}",
                    sample.DmsData.DatasetName,
                    remoteTargetFolder);
                classApplicationLogger.LogMessage(0, message, sample);
            }
            catch (Exception ex)
            {
                string message = string.Format("Method information for {0} was NOT copied to {1}",
                    sample.DmsData.DatasetName,
                    remoteTargetFolder);
                classApplicationLogger.LogError(0, message, ex, sample);
            }
        }

        /// <summary>
        /// Makes a local method folder named the same as the dataset folder
        /// </summary>
        /// <param name="sample">Sample we're making a request for</param>
        /// <returns>On success, string containing folder name/path; empty string on failure</returns>
        private static string MakeLocalMethodFolder(classSampleData sample)
        {
            string message;

            // Verify local method folder exists. Otherwise, create the folder.
            string localFolder = Path.Combine(classLCMSSettings.GetParameter("ApplicationPath"),
                LOCAL_METHOD_FOLDER_NAME);
            if (!Directory.Exists(localFolder))
            {
                try
                {
                    Directory.CreateDirectory(localFolder);
                }
                catch (Exception ex)
                {
                    message = "Could not create the method folder " + localFolder;
                    classApplicationLogger.LogError(0, message, ex, sample);
                    return "";
                }
            }
            return localFolder;
        }

        /// <summary>
        /// Test for presence of completed sample method folders that need to be moved to DMS
        /// </summary>
        /// <returns>TRUE if files found; FALSE otherwise</returns>
        public static bool CheckLocalMethodFolders()
        {
            // Check for presence of local method directory
            string localMethodXferFolder = Path.Combine(classLCMSSettings.GetParameter("ApplicationPath"),
                LOCAL_METHOD_FOLDER_NAME);
            if (!Directory.Exists(localMethodXferFolder))
                return false; // If no directory, there are no folders needing transfer

            // Get a list of the folders in the transfer folder
            string[] methodFolders = Directory.GetDirectories(localMethodXferFolder);

            // Check for method folders in the transfer folder
            if (methodFolders.Length < 1)
            {
                return false; // No method folders to copy
            }
            else
            {
                return true; // There are folders to copy
            }
        }

        /// <summary>
        /// Moves local sample method files to the DMS transfer folder
        /// </summary>
        public static void MoveLocalMethodFiles()
        {
            string localFolder = Path.Combine(classLCMSSettings.GetParameter("ApplicationPath"),
                LOCAL_METHOD_FOLDER_NAME);

            if (!Directory.Exists(localFolder))
            {
                classApplicationLogger.LogError(0, "The local methods folder does not exist.");
                return;
            }

            string remoteFolder = CreateRemoteFolderPath();
            bool exists = Directory.Exists(remoteFolder);
            if (!exists)
            {
                classApplicationLogger.LogError(0, "Creating the remote folder: " + remoteFolder);
                try
                {
                    Directory.CreateDirectory(remoteFolder);
                }
                catch
                {
                    //TODO: Chris/Brian Fix this!  Or Log that the directory is not available!
                }
            }

            // Get a list of the folders in the transfer folder
            string[] files = Directory.GetFiles(localFolder, "*.lcmethod");

            foreach (string file in files)
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    string remotePath = CreateRemoteFolderPath();

                    System.IO.File.Copy(file, Path.Combine(remotePath, fileName), true);
                    System.IO.File.Delete(file);
                    classApplicationLogger.LogMessage(1,
                        string.Format("Copying method file data.  Copied {0} to {1}.", file, remotePath));
                }
                catch (Exception ex)
                {
                    classApplicationLogger.LogError(0, "Could not copy the file " + file, ex);
                }
            }
        }

        /// <summary>
        /// Creates the remote system path.
        /// </summary>
        /// <returns></returns>
        public static string CreateRemoteFolderPath()
        {
            string path = Path.Combine(classLCMSSettings.GetParameter("TriggerFileFolder"), METHOD_FOLDER_NAME);
            DateTime now = DateTime.Now;
            int month = now.Month;
            int year = now.Year;
            int quarter = (month / 4) + 1;
            path = Path.Combine(path, string.Format("{0}_{1}", year, quarter));
            return path;
        }

        #endregion

        #region IMethodWriter Members

        void IMethodWriter.WriteMethodFiles(classSampleData sample)
        {
            classMethodFileTools.WriteMethodFiles(sample);
        }

        bool IMethodWriter.CheckLocalMethodFolders()
        {
            return classMethodFileTools.CheckLocalMethodFolders();
        }

        void IMethodWriter.MoveLocalMethodFiles()
        {
            classMethodFileTools.MoveLocalMethodFiles();
        }

        string IMethodWriter.CreateRemoteFolderPath()
        {
            return classMethodFileTools.CreateRemoteFolderPath();
        }

        #endregion
    }
}