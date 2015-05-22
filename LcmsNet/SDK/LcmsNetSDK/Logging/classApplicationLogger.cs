﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LcmsNetDataClasses.Logging
{
    public class ThreadPoolStateObject
    {
        public ThreadPoolStateObject(int messageLevel, object args)
        {
            MessageLevel = messageLevel;
            EventArgs = args;
        }

        /// <summary>
        /// Message importance level (0 is most important, 5 is least important)
        /// </summary>
        public int MessageLevel { get; set; }
        public object EventArgs { get; set; }
    }

    /// <summary>
    /// Class that marshalls messages from different components to different logging and streaming capabilities.
    /// </summary>
    public class classApplicationLogger: ILogger
    {
        /// <summary>
        /// Critical and should always be logged.
        /// </summary>
        public const int CONST_STATUS_LEVEL_CRITICAL = 0;
        /// <summary>
        /// More detailed error levels.
        /// </summary>
        public const int CONST_STATUS_LEVEL_DETAILED = 1;
        /// <summary>
        /// Less likely to need to be logged.
        /// </summary>
        public const int CONST_STATUS_LEVEL_USER = 2;

        /// <summary>
        /// Delegate method handler defining how a message event will be called.
        /// </summary>
        /// <param name="messageLevel"></param>
        /// <param name="args"></param>
        public delegate void DelegateMessageHandler(int messageLevel, classMessageLoggerArgs args);
        
        /// <summary>
        /// Delegate method handler defining how an error event will be called.
        /// </summary>
        /// <param name="errorLevel"></param>
        /// <param name="args"></param>
        public delegate void DelegateErrorHandler(int errorLevel, classErrorLoggerArgs args);
        
        /// <summary>
        /// Found when the application finds a message.
        /// </summary>
        public static event DelegateMessageHandler Message;
        
        /// <summary>
        /// Fired when the application has an error.
        /// </summary>
        public static event DelegateErrorHandler Error;

        /// <summary>
        /// Error message importance level (0 is most important, 5 is least important)
        /// </summary>
        private static int mint_errorLevel = 0;

        /// <summary>
        /// Status message importance level (0 is most important, 5 is least important)
        /// </summary>
        private static int mint_messageLevel = 0;

        /// <summary>
        /// Gets or sets the error level to log.
        /// </summary>
        public static int ErrorLevel
        {
            get
            {
                return mint_errorLevel;
            }
            set
            {
                mint_errorLevel = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the message level to log.  
        /// </summary>
        public static int MessageLevel
        {
            get
            {
                return mint_messageLevel;
            }
            set
            {
                mint_messageLevel = value;
            }
        }

        #region Error Methods
        /// <summary>
        /// Logs an error to the listening error output streams.
        /// </summary>
        /// <param name="errorLevel">Level of the error message so more verbose errors can be filtered.</param>
        /// <param name="message">Error message to report.</param>
        /// <param name="ex">An associated exception</param>
        /*public static void LogError(int errorLevel, string message, Exception ex, classSampleData sample)
        {
            if (errorLevel <= mint_errorLevel)
                if (Error != null)
                    Error(errorLevel, new classErrorLoggerArgs(message, ex, sample));
        }*/
        /// <summary>
        /// Logs an error to the listening error output streams.
        /// </summary>
        /// <param name="errorLevel">Level of the error message so more verbose errors can be filtered.</param>
        /// <param name="message">Error message to report.</param>
        /// <param name="ex">An associated exception</param>
        /*public static void LogError(int errorLevel, string message, Exception ex)
        {
            if (errorLevel <= mint_errorLevel)
                if (Error != null)
                    Error(errorLevel, new classErrorLoggerArgs(message, ex));
        }*/
        /// <summary>
        /// Logs an error to the listening error output streams.
        /// </summary>
        /// <param name="errorLevel">Level of the error message so more verbose errors can be filtered.</param>
        /// <param name="message">Error message to report.</param>
        public static void LogError(int errorLevel, string message, Exception ex = null, classSampleData sample = null)
        {
            /*if (errorLevel <= mint_errorLevel)
                if (Error != null)
                    Error(errorLevel, new classErrorLoggerArgs(message, sample));*/
            classErrorLoggerArgs args;
            if (sample != null)
            {
                args = new classErrorLoggerArgs(message, sample);
            }
            else
            {
                args = new classErrorLoggerArgs(message);
            }
            if(ex != null)
            {
                args.Exception = ex;
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(RaiseErrorEvent), new ThreadPoolStateObject(errorLevel, args));
            //RaiseErrorEvent(new ThreadPoolStateObject(errorLevel, args));
        }  
        #endregion

        #region Messages 

        /// <summary>
        /// Logs a message to the listening message output streams.
        /// </summary>
        /// <param name="messageLevel"></param>
        /// <param name="message"></param>
        /// <param name="sample"></param>
        public static void LogMessage(int messageLevel, string message, classSampleData sample = null)
        {            
            classMessageLoggerArgs args;
            if(sample != null)
            {
                args = new classMessageLoggerArgs(message, sample);
            }
            else
            {
                args = new classMessageLoggerArgs(message);
            }         
            ThreadPool.QueueUserWorkItem(new WaitCallback(RaiseMessageEvent), new ThreadPoolStateObject(messageLevel, args));
            //RaiseMessageEvent(new ThreadPoolStateObject(messageLevel, args));
        }

        /// <summary>
        /// Raises the error event in a Threadpool thread to avoid interrupting other functions with I/O.
        /// </summary>
        /// <param name="errorInfo"></param>
        public static void RaiseErrorEvent(object errorInfo)
        {
            ThreadPoolStateObject info = errorInfo as ThreadPoolStateObject;

            if(info.MessageLevel <= mint_errorLevel && Error != null)
            {
                Error(info.MessageLevel, info.EventArgs as classErrorLoggerArgs);
            }

        }

        /// <summary>
        /// Raises the message event in a Threadpool thread to avoid interrupting other functions with I/O.
        /// </summary>
        /// <param name="errorInfo"></param>
        private static void RaiseMessageEvent(object messageInfo)
        {
            ThreadPoolStateObject info = messageInfo as ThreadPoolStateObject;

            if (info.MessageLevel <= mint_messageLevel && Message!= null)
            {
                Message(info.MessageLevel, info.EventArgs as classMessageLoggerArgs);
            }
        }
        #endregion

        #region ILogger Members

        void ILogger.LogError(int errorLevel, string message, Exception ex, classSampleData sample)
        {
            classApplicationLogger.LogError(errorLevel, message, ex, sample);
        }

        void ILogger.LogError(int errorLevel, string message, Exception ex)
        {
            classApplicationLogger.LogError(errorLevel, message, ex);
        }

        void ILogger.LogError(int errorLevel, string message, classSampleData sample)
        {
            classApplicationLogger.LogError(errorLevel, message, null, sample);
        }

        void ILogger.LogError(int errorLevel, string message)
        {
            classApplicationLogger.LogError(errorLevel, message);
        }

        void ILogger.LogMessage(int messageLevel, string message)
        {
            classApplicationLogger.LogMessage(messageLevel, message);
        }

        void ILogger.LogMessage(int messageLevel, string message, classSampleData sample)
        {
            classApplicationLogger.LogMessage(messageLevel, message, sample);
        }

        #endregion
    }   
}
