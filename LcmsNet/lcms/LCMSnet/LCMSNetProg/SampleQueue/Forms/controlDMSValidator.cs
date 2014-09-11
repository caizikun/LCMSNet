﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using LcmsNetDataClasses;

namespace LcmsNet.SampleQueue.Forms
{
    public partial class controlDMSValidator : UserControl
    {
        #region Members
        /// <summary>
        /// Sample data to interrogate for validity.
        /// </summary>
        private classSampleData mobj_sample;
        /// <summary>
        /// Flag indicating that this Sample is ok.
        /// </summary>
        private bool mbool_isOK;
        #endregion

        public event EventHandler<DMSValidatorEventArgs> EnterPressed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sample">Sample to validate and edit.</param>
        public controlDMSValidator(classSampleData sample)
        {
            InitializeComponent();

            mnum_requestNumber.Maximum = int.MaxValue;
            
            /// 
            /// Make sure the sample is valid so we dont get an exception later
            /// when we try to edit it.
            /// 
            if (sample == null)
                throw new Exception("The sample was null and cannot be displayed.");

            mobj_sample = sample;

            mlabel_sampleName.Text = sample.DmsData.DatasetName;    

            mtextBox_experimentName.Text = mobj_sample.DmsData.Experiment;
            mtextbox_proposalID.Text     = mobj_sample.DmsData.ProposalID;
            mcomboBox_usageType.Text     = mobj_sample.DmsData.UsageType;
            mtextbox_user.Text           = mobj_sample.DmsData.UserList;
            mnum_requestNumber.Value     = Convert.ToDecimal(mobj_sample.DmsData.RequestID);

            this.mcomboBox_usageType.TextChanged        += new System.EventHandler(this.mtextbox_usageType_TextChanged);            
            this.mcomboBox_usageType.KeyUp              += new KeyEventHandler(KeyUpHandler);
            this.mtextbox_proposalID.TextChanged        += new System.EventHandler(this.mtextbox_proposalID_TextChanged);
            this.mtextbox_proposalID.KeyUp              += new KeyEventHandler(KeyUpHandler);
            this.mtextbox_user.TextChanged              += new System.EventHandler(this.mtextbox_user_TextChanged);
            this.mtextbox_user.KeyUp                    += new KeyEventHandler(KeyUpHandler);            
            this.mnum_requestNumber.ValueChanged        += new System.EventHandler(this.mnum_requestNumber_ValueChanged);
            this.mnum_requestNumber.KeyUp               += new KeyEventHandler(KeyUpHandler);
            this.mtextBox_experimentName.TextChanged    += new System.EventHandler(this.mtextBox_experimentName_TextChanged);
            this.mtextBox_experimentName.KeyUp          += new KeyEventHandler(KeyUpHandler);

            UpdateUserInterface();

            
        }
        public void SetFocusOn(DMSValidatorEventArgs args)
        {
            foreach(Control c in Controls)
            {
                if (c.Name == args.Name)
                {
                    if (c.Enabled == false)
                    {
                        if (EnterPressed != null)
                        {
                            EnterPressed(this, args);
                        }
                    }
                    c.Focus();
                    return;
                }
            }
        }
        public int ID
        {
            get;
            set;
        }
        
        void KeyUpHandler(object sender, KeyEventArgs e)
        {
            Control c           = sender as Control;
            bool    isEnterType = false;
            Keys modifier       = e.Modifiers;
            ComboBox box        = null;
            NumericUpDown updown = null;

            switch(e.KeyCode)
            {
                case Keys.Enter:
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
                    SelectNextControl(c, true, true, false,false);
                    break;
                case Keys.Left:
                    SelectNextControl(c, false, true, false, false);
                    break;
            }

            if (isEnterType && c != null)
            {
                if (this.EnterPressed != null)
                {
                    EnterPressed(this, new DMSValidatorEventArgs(c.Name, modifier));
                }
            }
        }


        #region Methods
        /// <summary>
        /// Checks the sample and updates the user interface accordingly.
        /// </summary>
        private void UpdateUserInterface()
        {
            if (mobj_sample == null)
                return;

            Dictionary<bool, Color> drawingBackgroundColors = new Dictionary<bool, Color>();
            Dictionary<bool, Color> drawingForegroundColors = new Dictionary<bool, Color>();

            drawingBackgroundColors.Add(false, Color.Red);
            drawingBackgroundColors.Add(true, Color.White);            
            drawingForegroundColors.Add(false, Color.White);
            drawingForegroundColors.Add(true, Color.Black);

            mbool_isOK = true;

            if (mobj_sample.DmsData.RequestID > 0)
            {
                mtextBox_experimentName.Enabled     = false;
                mtextBox_experimentName.BackColor   = Color.LightGray;
                mtextbox_proposalID.Enabled         = false;
                mtextbox_proposalID.BackColor       = Color.LightGray;
                mcomboBox_usageType.Enabled         = false;
                mcomboBox_usageType.BackColor       = Color.LightGray;
                mtextbox_user.Enabled               = false;
                mtextbox_user.BackColor             = Color.LightGray;
                mpictureBox_glyph.Image             = global::LcmsNet.Properties.Resources.AllIsGood;
                


            }
            else
            {                
                mtextBox_experimentName.Enabled = true;
                mtextbox_proposalID.Enabled     = true;
                mcomboBox_usageType.Enabled     = true;
                mtextbox_user.Enabled           = true;

                bool sampleOK = classDMSSampleValidator.IsEMSLProposalIDValid(mobj_sample);
                mtextbox_proposalID.BackColor = drawingBackgroundColors[sampleOK];
                mtextbox_proposalID.ForeColor = drawingForegroundColors[sampleOK];
                mbool_isOK = mbool_isOK & sampleOK;

                sampleOK = classDMSSampleValidator.IsEMSLUsageTypeValid(mobj_sample);
                mcomboBox_usageType.BackColor = drawingBackgroundColors[sampleOK];
                mcomboBox_usageType.ForeColor = drawingForegroundColors[sampleOK];
                mbool_isOK = mbool_isOK & sampleOK;

                sampleOK = classDMSSampleValidator.IsEMSLUserValid(mobj_sample);
                mtextbox_user.BackColor = drawingBackgroundColors[sampleOK];
                mtextbox_user.ForeColor = drawingForegroundColors[sampleOK];
                mbool_isOK = mbool_isOK & sampleOK;

                sampleOK = classDMSSampleValidator.IsExperimentNameValid(mobj_sample);
                mtextBox_experimentName.BackColor = drawingBackgroundColors[sampleOK];
                mtextBox_experimentName.ForeColor = drawingForegroundColors[sampleOK];
                mbool_isOK = mbool_isOK & sampleOK;

                /// 
                /// Make it look nice for the user so they can tell what is going on
                /// Here we just pick an image to display next to the sample to draw their 
                /// attention to it.
                /// 
                if (mbool_isOK == false)
                {
                    mpictureBox_glyph.Image = global::LcmsNet.Properties.Resources.ButtonDeleteRed;
                }
                else
                {
                    mpictureBox_glyph.Image = global::LcmsNet.Properties.Resources.AllIsGood;
                }
            }

        }
        #endregion 

        #region Properties
        /// <summary>
        /// Gets to the flag indicating if this sample is valid or not.
        /// </summary>
        public bool IsSampleValid
        {
            get
            {
                return mbool_isOK;
            }
        }
        #endregion 

        #region Form Event Handlers 
        private void mnum_requestNumber_ValueChanged(object sender, EventArgs e)
        {
            mobj_sample.DmsData.RequestID = Convert.ToInt32(mnum_requestNumber.Value);
            UpdateUserInterface();
        }

        private void mtextbox_usageType_TextChanged(object sender, EventArgs e)
        {
            mobj_sample.DmsData.UsageType = mcomboBox_usageType.Text;
            UpdateUserInterface();
        }

        private void mtextbox_proposalID_TextChanged(object sender, EventArgs e)
        {
            mobj_sample.DmsData.ProposalID = mtextbox_proposalID.Text;
            UpdateUserInterface();
        }

        private void mtextbox_user_TextChanged(object sender, EventArgs e)
        {
            mobj_sample.DmsData.UserList = mtextbox_user.Text;
            UpdateUserInterface();
        }

        private void mtextBox_experimentName_TextChanged(object sender, EventArgs e)
        {
            mobj_sample.DmsData.Experiment = mtextBox_experimentName.Text;
            UpdateUserInterface();
        }
        #endregion

    }

    public class DMSValidatorEventArgs: EventArgs
    {
        public DMSValidatorEventArgs(string name, Keys modifier)
        {
            Name        = name;
            Modifiers   = modifier;
        }
        public string Name
        {
            get;
            private set;
        }
        public Keys Modifiers
        {
            get;
            private set;
        }
    }
}
