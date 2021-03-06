﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using LcmsNetDataClasses;
using LcmsNetDataClasses.Data;

namespace LcmsNetDmsTools
{    
    public partial class controlDMSValidator : classDMSBaseControl
    {
        #region Members
        /// <summary>
        /// Sample data to interrogate for validity.
        /// </summary>
        private readonly classSampleData m_sample;

        /// <summary>
        /// Flag indicating that this Sample is ok.
        /// </summary>
        private bool m_isOK;
        #endregion

        public new event EventHandler<DMSValidatorEventArgs> EnterPressed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sample">Sample to validate and edit.</param>
        public controlDMSValidator(classSampleData sample)
        {
            InitializeComponent();

            mnum_requestNumber.Maximum = int.MaxValue;
            
            // 
            // Make sure the sample is valid so we dont get an exception later
            // when we try to edit it.
            // 
            if (sample == null)
                throw new Exception("The sample was null and cannot be displayed.");

            m_sample = sample;

            mlabel_sampleName.Text = sample.DmsData.DatasetName;

            mtextBox_experimentName.Text = m_sample.DmsData.Experiment;
            mtextbox_proposalID.Text     = m_sample.DmsData.ProposalID;
            mcomboBox_usageType.Text     = m_sample.DmsData.UsageType;
            mtextbox_user.Text           = m_sample.DmsData.UserList;
            mnum_requestNumber.Value     = Convert.ToDecimal(m_sample.DmsData.RequestID);

            mcomboBox_usageType.TextChanged        += mtextbox_usageType_TextChanged;
            mcomboBox_usageType.KeyUp              += KeyUpHandler;
            mtextbox_proposalID.TextChanged        += mtextbox_proposalID_TextChanged;
            mtextbox_proposalID.KeyUp              += KeyUpHandler;
            mtextbox_user.TextChanged              += mtextbox_user_TextChanged;
            mtextbox_user.KeyUp                    += KeyUpHandler;
            mnum_requestNumber.ValueChanged        += mnum_requestNumber_ValueChanged;
            mnum_requestNumber.KeyUp               += KeyUpHandler;
            mtextBox_experimentName.TextChanged    += mtextBox_experimentName_TextChanged;
            mtextBox_experimentName.KeyUp          += KeyUpHandler;

            UpdateUserInterface();

            
        }               
        
        void KeyUpHandler(object sender, KeyEventArgs e)
        {
            var c                = sender as Control;
            var isEnterType      = false;
            var modifier         = e.Modifiers;
            ComboBox box;
            NumericUpDown updown;
            Console.WriteLine(e.KeyCode.ToString());

            switch(e.KeyCode)
            {
                case Keys.Enter:
                    Console.WriteLine(@"Enter Pressed!");
                    LcmsNetDataClasses.Logging.classApplicationLogger.LogMessage(LcmsNetDataClasses.Logging.classApplicationLogger.CONST_STATUS_LEVEL_CRITICAL, "Enter pressed!");
                    isEnterType = true;
                    break;
                case Keys.Up:
                case Keys.VolumeUp:
                    modifier    = Keys.Shift;
                    isEnterType = true;

                    box = c as ComboBox;
                    if (box != null)
                    {
                        isEnterType = false;
                    }
                    else
                    {
                        updown = c as NumericUpDown;
                        if (updown != null)
                        {
                            isEnterType = false;
                        }
                    }
                    break;
                case Keys.Down:
                case Keys.VolumeDown:
                    modifier    = Keys.None;
                    isEnterType = true;

                    box = c as ComboBox;
                    if (box != null)
                    {
                        isEnterType = false;
                    }
                    else
                    {
                        updown = c as NumericUpDown;
                        if (updown != null)
                        {
                            isEnterType = false;
                        }                       
                    }
                    break;
                case Keys.Right:
                    if (c != null)
                        SelectNextControl(c, true, true, false, false);
                    break;
                case Keys.Left:
                    if (c != null)
                        SelectNextControl(c, false, true, false, false);
                    break;
            }

            if (isEnterType && c != null)
            {
                Console.WriteLine(@"isEnterType: " + c.Name);
                Console.WriteLine(EnterPressed != null);
                OnEnterPressed(this, new DMSValidatorEventArgs(c.Name, modifier));
            }
        }


        #region Methods
        /// <summary>
        /// Checks the sample and updates the user interface accordingly.
        /// </summary>
        private void UpdateUserInterface()
        {
            if (m_sample == null)
                return;

            var drawingBackgroundColors = new Dictionary<bool, Color>();
            var drawingForegroundColors = new Dictionary<bool, Color>();

            drawingBackgroundColors.Add(false, Color.Red);
            drawingBackgroundColors.Add(true, Color.White);
            drawingForegroundColors.Add(false, Color.White);
            drawingForegroundColors.Add(true, Color.Black);

            m_isOK = true;

            if (m_sample.DmsData.RequestID > 0)
            {
                mtextBox_experimentName.Enabled     = false;
                mtextBox_experimentName.BackColor   = Color.LightGray;
                mtextbox_proposalID.Enabled         = false;
                mtextbox_proposalID.BackColor       = Color.LightGray;
                mcomboBox_usageType.Enabled         = false;
                mcomboBox_usageType.BackColor       = Color.LightGray;
                mtextbox_user.Enabled               = false;
                mtextbox_user.BackColor             = Color.LightGray;
                mpictureBox_glyph.Image             = Properties.Resources.AllIsGood;
                


            }
            else
            {                
                mtextBox_experimentName.Enabled = true;
                mtextbox_proposalID.Enabled     = true;
                mcomboBox_usageType.Enabled     = true;
                mtextbox_user.Enabled           = true;

                var sampleOK = classDMSSampleValidator.IsEMSLProposalIDValid(m_sample);
                mtextbox_proposalID.BackColor = drawingBackgroundColors[sampleOK];
                mtextbox_proposalID.ForeColor = drawingForegroundColors[sampleOK];
                m_isOK = m_isOK & sampleOK;

                sampleOK = classDMSSampleValidator.IsEMSLUsageTypeValid(m_sample);
                mcomboBox_usageType.BackColor = drawingBackgroundColors[sampleOK];
                mcomboBox_usageType.ForeColor = drawingForegroundColors[sampleOK];
                m_isOK = m_isOK & sampleOK;

                sampleOK = classDMSSampleValidator.IsEMSLUserValid(m_sample);
                mtextbox_user.BackColor = drawingBackgroundColors[sampleOK];
                mtextbox_user.ForeColor = drawingForegroundColors[sampleOK];
                m_isOK = m_isOK & sampleOK;

                sampleOK = classDMSSampleValidator.IsExperimentNameValid(m_sample);
                mtextBox_experimentName.BackColor = drawingBackgroundColors[sampleOK];
                mtextBox_experimentName.ForeColor = drawingForegroundColors[sampleOK];
                m_isOK = m_isOK & sampleOK;

                // 
                // Make it look nice for the user so they can tell what is going on
                // Here we just pick an image to display next to the sample to draw their
                // attention to it.
                // 
                if (m_isOK == false)
                {
                    mpictureBox_glyph.Image = global::LcmsNetDmsTools.Properties.Resources.ButtonDeleteRed;
                }
                else
                {
                    mpictureBox_glyph.Image = global::LcmsNetDmsTools.Properties.Resources.AllIsGood;
                }
            }

        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets to the flag indicating if this sample is valid or not.
        /// </summary>
        public override bool IsSampleValid => m_isOK;

        #endregion

        #region Form Event Handlers
        private void mnum_requestNumber_ValueChanged(object sender, EventArgs e)
        {
            m_sample.DmsData.RequestID = Convert.ToInt32(mnum_requestNumber.Value);
            UpdateUserInterface();
        }

        private void mtextbox_usageType_TextChanged(object sender, EventArgs e)
        {
            m_sample.DmsData.UsageType = mcomboBox_usageType.Text;
            UpdateUserInterface();
        }

        private void mtextbox_proposalID_TextChanged(object sender, EventArgs e)
        {
            m_sample.DmsData.ProposalID = mtextbox_proposalID.Text;
            UpdateUserInterface();
        }

        private void mtextbox_user_TextChanged(object sender, EventArgs e)
        {
            m_sample.DmsData.UserList = mtextbox_user.Text;
            UpdateUserInterface();
        }

        private void mtextBox_experimentName_TextChanged(object sender, EventArgs e)
        {
            m_sample.DmsData.Experiment = mtextBox_experimentName.Text;
            UpdateUserInterface();
        }
        #endregion

    }   
}
