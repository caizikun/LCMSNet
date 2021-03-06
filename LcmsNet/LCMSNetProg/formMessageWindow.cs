﻿using System;
using System.Drawing;
using System.Windows.Forms;
using LcmsNetDataClasses.Logging;
using LcmsNetSDK;

namespace LcmsNet
{
    /// <summary>
    /// Form that displays errors and messages to the user.
    /// </summary>
    public partial class formMessageWindow : Form
    {
        public delegate void DelegateShowErrors(int level, classErrorLoggerArgs args);

        bool m_errorLabelSelected;
        bool m_messageLabelSelected;

        /// <summary>
        /// Constructor.
        /// </summary>
        public formMessageWindow()
        {
            InitializeComponent();
            m_messageLevel = classApplicationLogger.CONST_STATUS_LEVEL_USER;
            m_errorLevel = classApplicationLogger.CONST_STATUS_LEVEL_USER;
            m_lockMessages = new object();
            m_lockErrors = new object();
            SelectErrorTab();
        }

        /// <summary>
        /// Fired when an error is found by the system.
        /// </summary>
        public event EventHandler ErrorPresent;

        /// <summary>
        /// Fired when the user says the errors are cleared.
        /// </summary>
        public event EventHandler ErrorCleared;

        /// <summary>
        /// Formats the input string message with a date and time string.
        /// </summary>
        /// <param name="message">Message to format.</param>
        /// <returns>Formatted string message to display.</returns>
        private string FormatMessage(string message)
        {
            return string.Format("{0} {1}: {2}",
                DateTime.UtcNow.Subtract(TimeKeeper.Instance.TimeZone.BaseUtcOffset).ToLongDateString(),
                DateTime.UtcNow.Subtract(TimeKeeper.Instance.TimeZone.BaseUtcOffset).TimeOfDay,
                message);
        }

        /// <summary>
        /// Displays the error message to the screen.
        /// </summary>
        /// <param name="level">Filter for displaying messages.</param>
        /// <param name="message">Message to show user.</param>
        public void ShowMessage(int level, classMessageLoggerArgs message)
        {
            if (level <= m_messageLevel && message != null)
            {
                InsertMessage(FormatMessage(message.Message));
                //mlistBox_messages.Items.Insert(0, FormatMessage(message.Message));
                //mlistBox_messages.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Updates message window using a delegate to avoid cross-thread problems
        /// </summary>
        /// <param name="message"></param>
        private void InsertMessage(string message)
        {
            if (mlistBox_messages.InvokeRequired)
            {
                var d = new delegateInsertMessage(InsertMessage);
                try
                {
                    mlistBox_messages.Invoke(d, message);
                }
                catch (Exception ex)
                {
                    // An exception can occur if one thread is trying to post a message while the main application is closing
                    Console.WriteLine("Ignoring exception in InsertMessage: " + ex.Message);
                }

            }
            else
            {
                lock (m_lockMessages)
                {
                    mlistBox_messages.Items.Insert(0, message);
                    mlistBox_messages.SelectedIndex = 0;
                }
            }
        }

        private void ShowErrorsDelegated(int level, classErrorLoggerArgs args)
        {
            if (level <= m_errorLevel && args != null)
            {
                ErrorPresent?.Invoke(this, new EventArgs());
                var exceptions = "";
                lock (m_lockErrors)
                {
                    if (args.Exception != null)
                    {
                        m_errorMessages.Text = FormatMessage(args.Exception.StackTrace) + "\n" + m_errorMessages.Text;

                        var ex = args.Exception;
                        while (ex != null)
                        {
                            exceptions += ex.Message + "\n";
                            ex = ex.InnerException;
                        }

                        m_errorMessages.Text = FormatMessage(exceptions) + "\n" + m_errorMessages.Text;
                    }

                    m_errorMessages.Text = FormatMessage(args.Message) + "\n" + m_errorMessages.Text;
                }
            }
        }

        /// <summary>
        /// Displays error messages to the user.
        /// </summary>
        /// <param name="level">Level of errors to display.</param>
        /// <param name="error">Error to display.</param>
        public void ShowErrors(int level, classErrorLoggerArgs error)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DelegateShowErrors(ShowErrorsDelegated), level, error);
            }
            else
            {
                ShowErrorsDelegated(level, error);
            }
        }

        private void mbutton_acknowledgeErrors_Click(object sender, EventArgs e)
        {
            m_errorMessages.Clear();
            ErrorCleared?.Invoke(this, new EventArgs());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lock (m_lockMessages)
            {
                mlistBox_messages.Items.Clear();
            }
        }

        private void mlabel_errors_Click(object sender, EventArgs e)
        {
            SelectErrorTab();
        }

        private void mlabel_messages_Click(object sender, EventArgs e)
        {
            SelectMessageTab();
        }

        private void SelectErrorTab()
        {
            customTabControl1.SelectedTab = mtab_errors;
            //mlabel_errors.BackColor             = System.Drawing.Color.White;
            mlabel_errors.ForeColor = Color.Black;
            mpanel_errorIndicator.BackColor = Color.DarkGray;

            //mlabel_messages.BackColor           = System.Drawing.Color.LightGray;
            mlabel_messages.ForeColor = Color.Gray;
            mpanel_messageIndicator.BackColor = Color.White;
            m_errorLabelSelected = true;
            m_messageLabelSelected = false;
        }

        private void SelectMessageTab()
        {
            customTabControl1.SelectedTab = mtab_messages;
            //mlabel_errors.BackColor             = System.Drawing.Color.LightGray;
            mlabel_errors.ForeColor = Color.Gray;
            mpanel_errorIndicator.BackColor = Color.White;

            //mlabel_messages.BackColor           = System.Drawing.Color.White;
            mlabel_messages.ForeColor = Color.Black;
            mpanel_messageIndicator.BackColor = Color.DarkGray;
            m_messageLabelSelected = true;
            m_errorLabelSelected = false;
        }


        private void mlabel_errors_MouseLeave(object sender, EventArgs e)
        {
            if (!m_errorLabelSelected)
            {
                mlabel_errors.ForeColor = Color.Gray;
                mpanel_errorIndicator.BackColor = Color.White;
            }
        }

        private void mlabel_errors_MouseEnter(object sender, EventArgs e)
        {
            if (!m_errorLabelSelected)
            {
                mlabel_errors.ForeColor = Color.LightGray;
                mpanel_errorIndicator.BackColor = Color.DarkGray;
            }
        }

        private void mlabel_messages_MouseLeave(object sender, EventArgs e)
        {
            if (!m_messageLabelSelected)
            {
                mlabel_messages.ForeColor = Color.Gray;
                mpanel_messageIndicator.BackColor = Color.White;
            }
        }

        private void mlabel_messages_MouseEnter(object sender, EventArgs e)
        {
            if (!m_messageLabelSelected)
            {
                mlabel_messages.ForeColor = Color.LightGray;
                mpanel_messageIndicator.BackColor = Color.DarkGray;
            }
        }

        #region "Delegates"

        /// <summary>
        /// Delegate used for updating message window without cross-thread problems
        /// </summary>
        /// <param name="message"></param>
        private delegate void delegateInsertMessage(string message);

        #endregion

        #region Members

        /// <summary>
        /// Level of messages to show.  Messages greater than are ignored.
        /// </summary>
        private int m_messageLevel;

        /// <summary>
        /// Level of errors to show.  Errors greater than are ignored.
        /// </summary>
        private int m_errorLevel;

        private readonly object m_lockMessages;
        private readonly object m_lockErrors;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the message filter level.  Messages greater than level are not shown.
        /// </summary>
        public int MessageLevel
        {
            get { return m_messageLevel; }
            set { m_messageLevel = value; }
        }

        /// <summary>
        /// Gets or sets the error filter level.  Errors greater than level are not shown.
        /// </summary>
        public int ErrorLevel
        {
            get { return m_errorLevel; }
            set { m_errorLevel = value; }
        }

        #endregion
    }
}