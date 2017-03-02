﻿//*********************************************************************************************************
// Written by Dave Clark, Brian LaMarche for the US Department of Energy
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2009, Battelle Memorial Institute
// Created 04/14/2009
//
// Last modified 04/14/2009
//                      - 12/01/2009 (DAC) - Modified to accomodate change of vial from string to int
//*********************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Logging;

namespace LcmsNet.SampleQueue.IO
{
    class classQueueImportXML : ISampleQueueReader
    {
        //*********************************************************************************************************
        // Imports an XML file from LCMS
        //**********************************************************************************************************

        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        public classQueueImportXML()
        {
        }

        /// <summary>
        /// Reads the XML file into a list
        /// </summary>
        /// <param name="path">Name and path of file to import</param>
        /// <returns>List<classSampleData> containing samples read from XML file</returns>
        public List<classSampleData> ReadSamples(string path)
        {
            List<classSampleData> returnList = new List<classSampleData>();

            // Verify input file exists
            if (!File.Exists(path))
            {
                string ErrMsg = "Import file " + path + " not found";
                classApplicationLogger.LogMessage(0, ErrMsg);
                return returnList;
            }

            // Open the file
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch (Exception Ex)
            {
                string ErrMsg = "Exception loading XML file " + path;
                classApplicationLogger.LogError(0, ErrMsg, Ex);
                throw new classDataImportException(ErrMsg, Ex);
            }

            // Get all the nodes under QueueSettings node
            XmlNodeList nodeList = doc.SelectNodes("//QueueSettings/*");

            // If no nodes found, report and exit
            if (nodeList.Count < 1)
            {
                string ErrMsg = "No data found for import in file " + path;
                classApplicationLogger.LogMessage(0, ErrMsg);
                return returnList;
            }

            // Get the data for each sample and add it to the return list
            foreach (XmlNode currentNode in nodeList)
            {
                if (currentNode.Name.StartsWith("Item") && !currentNode.Name.Equals("ItemCount"))
                {
                    try
                    {
                        classSampleData newSample = ConvertXMLNodeToSample(currentNode);
                        returnList.Add(newSample);
                    }
                    catch (Exception Ex)
                    {
                        string ErrMsg = "Exception converting XML item node to sample " + currentNode.Name;
                        classApplicationLogger.LogError(0, ErrMsg, Ex);
                        throw new classDataImportException(ErrMsg, Ex);
                    }
                }
            }

            return returnList;
        }

        /// <summary>
        /// Converts an individual XML node into a sampledata object
        /// </summary>
        /// <param name="ItemNode">XML node containing data for 1 sample</param>
        /// <returns>classSampleData object containing data from the XML node</returns>
        private classSampleData ConvertXMLNodeToSample(XmlNode ItemNode)
        {
            classSampleData retData = new classSampleData(false);
            string tempStr;

            // Description (DMS.Name)
            tempStr = ConvertNullToString(ItemNode.SelectSingleNode("Description").InnerText);
            // Value is mandatory for this field, so check for it
            if (tempStr != "")
            {
                retData.DmsData.RequestName = tempStr;
            }
            else
            {
                classApplicationLogger.LogMessage(0, "Description field empty or missing. Import cannot be performed");
                return null;
            }

            // Selection Method (PAL.Method)
            retData.PAL.Method = ConvertNullToString(ItemNode.SelectSingleNode("Selection/Method").InnerText);

            // Tray (PAL.Tray) (aka wellplate)
            retData.PAL.PALTray = ConvertNullToString(ItemNode.SelectSingleNode("Selection/Tray").InnerText);

            // Vial (PAL.Vial) (aka well)
            string tmpStr = ConvertNullToString(ItemNode.SelectSingleNode("Selection/Vial").InnerText);
            if (tmpStr == "")
            {
                retData.PAL.Well = 0;
            }
            else
            {
                retData.PAL.Well = int.Parse(tmpStr);
            }

            // Volume (Volume)
            retData.Volume = ConvertNullToDouble(ItemNode.SelectSingleNode("Selection/Volume").InnerText);

            // Separation Method (Experiment.ExperimentName)
            string methodName = ConvertNullToString(ItemNode.SelectSingleNode("Separation/Method").InnerText);
            retData.LCMethod = new LcmsNetDataClasses.Method.classLCMethod();
            retData.LCMethod.Name = methodName;

            // Acquisition Method (InstrumentData.MethodName)
            retData.InstrumentData.MethodName =
                ConvertNullToString(ItemNode.SelectSingleNode("Acquisition/Method").InnerText);

            // DMS RequestNumber (DMSData.RequestID)
            retData.DmsData.RequestID = ConvertNullToInt(ItemNode.SelectSingleNode("DMS/RequestNumber").InnerText);

            // DMS Comment (DMSData.Comment)
            retData.DmsData.Comment = ConvertNullToString(ItemNode.SelectSingleNode("DMS/Comment").InnerText);

            // DMS DatasetType (DMSData.DatasetType)
            retData.DmsData.DatasetType = ConvertNullToString(ItemNode.SelectSingleNode("DMS/DatasetType").InnerText);

            // DMS Experiment (DMSData.Experiment)
            retData.DmsData.Experiment = ConvertNullToString(ItemNode.SelectSingleNode("DMS/Experiment").InnerText);

            // DMS EMSLProposalID (DMSData.ProposalID)
            retData.DmsData.ProposalID = ConvertNullToString(ItemNode.SelectSingleNode("DMS/EMSLProposalID").InnerText);

            // DMS EMSLUsageType (DMSData.UsageType)
            retData.DmsData.UsageType = ConvertNullToString(ItemNode.SelectSingleNode("DMS/EMSLUsageType").InnerText);

            // DMS EMSLUser (DMSData.UserList)
            retData.DmsData.UserList = ConvertNullToString(ItemNode.SelectSingleNode("DMS/EMSLUser").InnerText);

            // It's all in, so return
            return retData;
        }

        /// <summary>
        /// Utility method to convert a null or empty string value in the XML file to a string
        /// </summary>
        /// <param name="InpVal">String from XML parser</param>
        /// <returns>If input string is empty or null, returns empty string. Otherwise returns input string</returns>
        private string ConvertNullToString(string InpVal)
        {
            if (string.IsNullOrEmpty(InpVal))
            {
                return string.Empty;
            }
            else
            {
                return InpVal;
            }
        }

        /// <summary>
        /// Utility method to convert a null or empty string value in the XML file to an int
        /// </summary>
        /// <param name="InpVal">String from XML parser</param>
        /// <returns>If input string is empty or null, returns 0. Otherwise returns input string converted to int</returns>
        private int ConvertNullToInt(string InpVal)
        {
            if (string.IsNullOrEmpty(InpVal))
            {
                return 0;
            }
            else
            {
                return int.Parse(InpVal);
            }
        }

        /// <summary>
        /// Utility method to convert a null or empty string value in the XML file to a double
        /// </summary>
        /// <param name="InpVal">String from XML parser</param>
        /// <returns>If input string is empty or null, returns 0.0. Otherwise returns input string converted to double</returns>
        private double ConvertNullToDouble(string InpVal)
        {
            if (string.IsNullOrEmpty(InpVal))
            {
                return 0.0;
            }
            else
            {
                return double.Parse(InpVal);
            }
        }

        #endregion
    }
} // End namespace