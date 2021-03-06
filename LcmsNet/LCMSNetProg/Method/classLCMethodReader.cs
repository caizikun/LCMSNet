﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Devices;
using LcmsNetDataClasses.Logging;
using LcmsNetDataClasses.Method;

namespace LcmsNet.Method
{
    /// <summary>
    /// Class that can handle reading a method from a file path.
    /// </summary>
    public class classLCMethodReader
    {
        #region Events and Delegates

        /// <summary>
        /// Definition for a method that
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate bool DelegateValidateDevice(object sender, classLCMethodDeviceArgs args);

        #endregion

        public classLCMethodReader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }

        /// <summary>
        /// Finds the type within the loaded assemblies.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Type FindType(string name)
        {
            Type constructedParameterType = null;
            // This could be a bit slow.  It's intended to load the
            // assemblies and their types.  Since we are loading plugins from another directory.
            // Then the CLR does not know where to look necessarily.  Here we are
            // looking within the loaded assemblies to find the type.
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var assemblies = new List<Assembly>();
            assemblies.AddRange(loadedAssemblies);
            assemblies.Add(Assembly.GetExecutingAssembly()); // Make sure we check the LCMS Net ones as well.

            foreach (var ass in assemblies)
            {
                try
                {
                    var types = ass.GetTypes();
                    foreach (var assType in types)
                    {
                        // Alternatively use .AssemblyQualifiedName;
                        if (assType.FullName == name)
                        {
                            constructedParameterType = assType;
                            return constructedParameterType;
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore...we don't need to worry about these problems.
                }
            }
            return constructedParameterType;
        }

        /// <summary>
        /// Reads the LC-Event from the specified node.
        /// </summary>
        /// <param name="node">Node that contains the event definition</param>
        /// <returns>An LC-Event</returns>
        private classLCEvent ReadEventNode(XmlNode node)
        {
            var lcEvent = new classLCEvent();

            // Read the name
            if (node.Attributes != null)
            {
                var nameAttribute = node.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_NAME);
                lcEvent.Name = nameAttribute.Value;
            }

            // Construct flags
            var value = node.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_OPTIMIZE_WITH);
            lcEvent.OptimizeWith = Convert.ToBoolean(value.Value);
            value = node.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_HAS_DISCREET_STATES);
            lcEvent.HasDiscreteStates = Convert.ToBoolean(value.Value);

            try
            {
                value = node.Attributes.GetNamedItem(classLCMethodFactory.CONST_IS_EVENT_INDETERMINANT);
                lcEvent.IsIndeterminant = Convert.ToBoolean(value.Value);
            }
            catch
            {
                Debug.WriteLine(
                    "Null Reference Exception due to backwards compatibility check in LCMethodReader");
                // This is to be backwards compatible.
            }

            // Start Time
            value = node.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_START);
            lcEvent.Start = Convert.ToDateTime(value.Value);

            // Hold Time
            TimeSpan span;
            value = node.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_HOLD_TIME);
            if (TimeSpan.TryParse(value.Value, out span))
            {
                lcEvent.HoldTime = span;
            }
            else
            {
                throw new classInvalidTimeSpanException("Could not read the hold time in the method.");
            }

            value = node.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_DURATION);
            if (TimeSpan.TryParse(value.Value, out span))
            {
                lcEvent.Duration = span;
            }
            else
            {
                throw new classInvalidTimeSpanException("Could not read the duration of the method.");
            }

            // Read the parameters
            var parameters = node.SelectSingleNode(classLCMethodFactory.CONST_XPATH_PARAMETERS);
            var parameterList = parameters.SelectNodes(classLCMethodFactory.CONST_XPATH_PARAMETER);

            // Create an array of expected parameters.
            var parameterArray = new object[parameterList.Count];
            var parameterNameArray = new string[parameterList.Count];
            var typeArray = new Type[parameterList.Count];


            // Use this sequentially to avoid ambiguity with iterations or out of order keying.
            for (var i = 0; i < parameterList.Count; i++)
            {
                var parameterNode = parameterList[i];

                var parameterValue = parameterNode.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_VALUE);
                var parameterType = parameterNode.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_TYPE);
                var parameterName = parameterNode.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_NAME);

                //
                // Create a parameter type, if it fails?! well...
                //
                Type constructedParameterType = null;
                try
                {
                    constructedParameterType = FindType(parameterType.Value);
                    //Type.GetType(parameterType.Value, true, true);
                }
                catch (FileNotFoundException fileEx)
                {
                    try
                    {
                        constructedParameterType = FindType(parameterType.Value);
                        if (constructedParameterType == null)
                        {
                            throw new Exception(
                                string.Format("Could not construct parameter for {0}.", parameterName.Value), fileEx);
                        }
                    }
                    catch (Exception newException)
                    {
                        constructedParameterType = null;
                        throw new Exception(
                            string.Format("Could not construct parameter for {0}.", parameterName.Value), newException);
                    }
                }
                catch (TypeLoadException ex)
                {
                    try
                    {
                        constructedParameterType = FindType(parameterType.Value);
                        if (constructedParameterType == null)
                        {
                            throw new Exception(
                                string.Format("Could not construct parameter for {0}.", parameterName.Value), ex);
                        }
                    }
                    catch (Exception newException)
                    {
                        constructedParameterType = null;
                        throw new Exception(
                            string.Format("Could not construct parameter for {0}.", parameterName.Value), newException);
                    }
                }
                catch (Exception ex)
                {
                    constructedParameterType = null;
                    throw new Exception(string.Format("Could not construct parameter for {0}.", parameterName.Value), ex);
                }

                // If it's an enumeration, then we'll have to treat it different from the rest of things
                if (constructedParameterType.IsEnum)
                {
                    parameterArray[i] = Enum.Parse(constructedParameterType, parameterValue.Value);
                    parameterNameArray[i] = parameterName.Value;
                    typeArray[i] = constructedParameterType;
                }
                else
                {
                    //
                    // Poor mans way of converting these parameter arguments.  But it works, however, its
                    // constraining for us.
                    //
                    switch (constructedParameterType.FullName)
                    {
                        case "System.Double":
                            parameterArray[i] = Convert.ToDouble(parameterValue.Value);
                            parameterNameArray[i] = parameterName.Value;
                            typeArray[i] = typeof (double);
                            break;
                        case "System.String":
                            parameterArray[i] = parameterValue.Value;
                            parameterNameArray[i] = parameterName.Value;
                            typeArray[i] = typeof (string);
                            break;
                        default:
                            parameterArray[i] = null;
                            parameterNameArray[i] = parameterName.Value;
                            typeArray[i] = typeof (classSampleData);
                            break;
                    }
                }
            }
            lcEvent.Parameters = parameterArray;
            lcEvent.ParameterNames = parameterNameArray;

            //
            // Device Initialization
            //
            value = node.SelectSingleNode(classLCMethodFactory.CONST_XPATH_DEVICE);
            var attribute = value.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_NAME);
            var deviceName = attribute.Value;
            attribute = value.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_TYPE);
            var deviceTypeName = attribute.Value;

            var devicetype = FindType(deviceTypeName);
            var device = classDeviceManager.Manager.FindDevice(deviceName, devicetype);
            if (device == null)
            {
                throw new classDeviceNotFoundException("Could not find the device " + deviceName + ".", deviceName);
            }
            lcEvent.Device = device;

            //
            // Method Info for invoking the device's method.
            //
            value = node.SelectSingleNode(classLCMethodFactory.CONST_XPATH_METHOD_INFO);
            attribute = value.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_NAME);
            var methodName = attribute.Value;
            MethodInfo method = null;
            try
            {
                //
                // There is no way to disambiguate overloaded methods...so we
                // have to see if the parameters match of the methods
                // that have the name provided.
                //
                var methods = devicetype.GetMethods();
                foreach (var info in methods)
                {
                    var parameterInfo = info.GetParameters();
                    if (info.Name == methodName && parameterInfo.Length == lcEvent.Parameters.Length)
                    {
                        var i = 0;
                        var found = true;
                        foreach (var pinfo in parameterInfo)
                        {
                            //if (pinfo.ParameterType.Equals(typeArray[i]) == false)
                            if ((pinfo.ParameterType.Name == typeArray[i].Name) == false)
                            {
                                found = false;
                                break;
                            }
                            i++;
                        }

                        if (found)
                        {
                            method = info;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not read the LC Method.  ", ex);
            }
            lcEvent.Method = method;

            //
            // Get the method attributes for this method as well.
            // Here we assume that there should only be ONE method attribute per method.
            // Otherwise this breaks specification, so it should be there. If the method was saved
            // then it should have also been persisted into the XML.  Otherwise...not a chance?
            //
            try
            {
                var methodAttributes = method.GetCustomAttributes(false);
                foreach (var attr in methodAttributes)
                {
                    var meth = attr as classLCMethodAttribute;
                    if (meth != null)
                    {
                        lcEvent.MethodAttribute = meth;
                        break;
                    }
                }
            }
            catch (Exception exOld)
            {
                var ex = new Exception("Could not read the LC-method event for device " + deviceName, exOld);
                throw ex;
            }
            return lcEvent;
        }

        /// <summary>
        /// Reads the method contained in the XML file path.
        /// </summary>
        /// <param name="filePath">Path of file that contains method.</param>
        /// <param name="errors"></param>
        /// <returns>Null if the path does not exist. New method object if successful.</returns>
        public classLCMethod ReadMethod(string filePath, List<Exception> errors)
        {
            return ReadMethod(filePath, false, errors);
        }

        /// <summary>
        /// Reads the method from the filepath.
        /// </summary>
        /// <param name="filePath">Path of the method file to read.</param>
        /// <param name="readActuals">Flag indicating whether to read the actual event information (if it exists).</param>
        /// <param name="errors"></param>
        /// <returns>LC-Method read from the file.</returns>
        public classLCMethod ReadMethod(string filePath, bool readActuals, List<Exception> errors)
        {
            if (File.Exists(filePath) == false)
                return null;

            //
            // Load the document,
            //     Catch XML errors and authorization errors.
            //     We have made sure the file exists.
            //
            var document = new XmlDocument();
            try
            {
                document.Load(filePath);
            }
            catch (XmlException ex)
            {
                throw new Exception("The LC-method file was corrupt.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception("You do not have authorization to open the method file.",
                    ex);
            }
            var root = document.SelectSingleNode(classLCMethodFactory.CONST_XPATH_METHOD);

            return ReadEventList(root, false, errors);
        }

        /// <summary>
        /// Reads the event list from the document provided.
        /// </summary>
        /// <param name="root">Root item in the document.</param>
        /// <param name="readActual">Flag indicating whether to read the actual data instead of the proposed data.</param>
        /// <param name="errors"></param>
        /// <returns>LC-Method containing all event information.</returns>
        private classLCMethod ReadEventList(XmlNode root, bool readActual, List<Exception> errors)
        {
            classLCMethod method = null;

            //
            // Get the name of the lc-method
            //

            try
            {
                var nameAttribute = root.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_NAME);
                method = new classLCMethod {
                    Name = nameAttribute.Value
                };

                nameAttribute = root.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_IS_SPECIAL);
                if (nameAttribute != null)
                {
                    method.IsSpecialMethod = bool.Parse(nameAttribute.Value);
                    if (method.IsSpecialMethod)
                    {
                        method.Column = -1;
                    }
                    else
                    {
                        nameAttribute = root.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_COLUMN_DATA);
                        method.Column = int.Parse(nameAttribute.Value);
                    }
                    nameAttribute = root.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_ALLOW_POST_OVERLAP);
                    method.AllowPostOverlap = bool.Parse(nameAttribute.Value);

                    nameAttribute = root.Attributes.GetNamedItem(classLCMethodFactory.CONST_XPATH_ALLOW_PRE_OVERLAP);
                    method.AllowPreOverlap = bool.Parse(nameAttribute.Value);
                }
            }
            catch
            {
                return null;
            }
            //
            // Now get the list and parse each item
            //
            var eventListNode = root.SelectNodes(classLCMethodFactory.CONST_XPATH_EVENTS);

            //
            // If the user really wanted to read the actual data, then read it instead
            // of reading the proposed method events. Actual data is stored after
            // a run in the XML file if requested.  This provides information about
            // the performance of the software and control.
            //
            if (readActual)
            {
                //
                // Make sure we have some kind of events
                //
                eventListNode = root.SelectNodes(classLCMethodFactory.CONST_XPATH_ACTUAL_METHOD);
                if (eventListNode == null || eventListNode.Count < 1)
                    return null;
            }

            if (eventListNode == null)
                return method;

            var i = 0;
            foreach (XmlNode node in eventListNode)
            {
                i++;
                try
                {
                    var lcEvent = ReadEventNode(node);
                    method.Events.Add(lcEvent);
                }
                catch (classDeviceNotFoundException ex)
                {
                    var error =
                        string.Format("The Device \"{0}\" was missing from the hardware manager at event {1}.",
                                      ex.DeviceName,
                                      i);
                    var newException = new Exception(error, ex);
                    classApplicationLogger.LogError(1, error, ex);
                    errors.Add(newException);
                }
            }
            return method;
        }
    }
}