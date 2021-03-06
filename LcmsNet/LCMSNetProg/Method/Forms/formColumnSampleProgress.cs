﻿using System;
using System.Drawing;
using System.Windows.Forms;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Logging;
using LcmsNetDataClasses.Method;

namespace LcmsNet.Method.Forms
{
    public partial class formColumnSampleProgress : Form
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public formColumnSampleProgress()
        {
            InitializeComponent();

            mcontrol_sampleProgress.RenderAllEvents = false;
            mcontrol_sampleProgress.RenderCurrent = true;

            mcontrol_sampleProgressFull.RenderAllEvents = true;
            mcontrol_sampleProgressFull.RenderCurrent = false;
            mcontrol_sampleProgressFull.RenderDisplayWindow = true;
        }

        /// <summary>
        /// Updates the progress window with the sample data.
        /// </summary>
        /// <param name="sample"></param>
        public void UpdateSample(classSampleData sample)
        {
            mcontrol_sampleProgress.UpdateSample(sample);
            mcontrol_sampleProgressFull.UpdateSample(sample);
        }

        public void UpdateError(classSampleData sample, classLCEvent lcEvent)
        {
            mcontrol_sampleProgressFull.UpdateError(sample, lcEvent);
        }

        /// <summary>
        /// Updates preview lable minute text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnum_previewMinutes_ValueChanged(object sender, EventArgs e)
        {
            var previewMinutes = Convert.ToInt32(mnum_previewMinutes.Value);

            mlabel_previewMinutes.Text = previewMinutes + "-minute-preview";
            mcontrol_sampleProgress.PreviewMinutes = previewMinutes;
            mcontrol_sampleProgressFull.PreviewMinutes = mcontrol_sampleProgress.PreviewMinutes;
        }

        public event EventHandler<SampleProgressPreviewArgs> PreviewAvailable;

        private void m_previewTimer_Tick(object sender, EventArgs e)
        {
            mcontrol_sampleProgress.Refresh();
            mcontrol_sampleProgressFull.Refresh();

            if (PreviewAvailable != null)
            {
                try
                {
                    var map = new Bitmap(mcontrol_sampleProgressFull.Width,
                        mcontrol_sampleProgressFull.Height);
                    var gfx = Graphics.FromImage(map);
                    mcontrol_sampleProgressFull.RenderGraph(gfx);
                        //DrawToBitmap(map,new System.Drawing.Rectangle(0, 0, mcontrol_sampleProgressFull.Width, mcontrol_sampleProgressFull.Height));
                    PreviewAvailable(this, new SampleProgressPreviewArgs((Image) map.Clone()));
                    map.Dispose();
                    gfx.Dispose();
                }
                catch
                {
                    classApplicationLogger.LogError(0,
                        "Error attempting to update column sample progress.");
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void milliseconds_ValueChanged(object sender, EventArgs e)
        {
        }
    }

    public class SampleProgressPreviewArgs : EventArgs, IDisposable
    {
        public bool disposed;

        public SampleProgressPreviewArgs(Image image)
        {
            disposed = false;
            PreviewImage = image;
        }

        /// <summary>
        /// Gets the preview image for the sample progress
        /// </summary>
        public Image PreviewImage { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeOthers)
        {
            if (disposed)
            {
                return;
            }
            if (disposeOthers)
            {
                PreviewImage.Dispose();
                PreviewImage = null;
            }
            disposed = true;
        }
    }
}