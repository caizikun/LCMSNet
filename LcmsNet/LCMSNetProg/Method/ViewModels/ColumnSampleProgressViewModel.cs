﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Logging;
using LcmsNetDataClasses.Method;
using ReactiveUI;

namespace LcmsNet.Method.ViewModels
{
    public class ColumnSampleProgressViewModel : ReactiveObject
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColumnSampleProgressViewModel()
        {
            SampleProgress = new SampleProgressViewModel();
            SampleProgressFull = new SampleProgressViewModel();

            SampleProgress.RenderAllEvents = false;
            SampleProgress.RenderCurrent = true;

            SampleProgressFull.RenderAllEvents = true;
            SampleProgressFull.RenderCurrent = false;
            SampleProgressFull.RenderDisplayWindow = true;

            this.WhenAnyValue(x => x.Minutes).Subscribe(x => this.PreviewMinutesUpdated());

            previewUpdateTimer = new Timer(UpdateDrawings, this, 1000, 1000);
        }

        private string previewLabelText = "30-minute-preview";
        private int minutes = 30;
        private int seconds = 1;
        private int milliseconds = 1;
        private Timer previewUpdateTimer;
        private SampleProgressViewModel sampleProgress;
        private SampleProgressViewModel sampleProgressFull;

        public string PreviewLabelText
        {
            get { return previewLabelText; }
            set { this.RaiseAndSetIfChanged(ref previewLabelText, value); }
        }

        public int Minutes
        {
            get { return minutes; }
            set { this.RaiseAndSetIfChanged(ref minutes, value); }
        }

        public int Seconds
        {
            get { return seconds; }
            set { this.RaiseAndSetIfChanged(ref seconds, value); }
        }

        public int Milliseconds
        {
            get { return milliseconds; }
            set { this.RaiseAndSetIfChanged(ref milliseconds, value); }
        }

        public SampleProgressViewModel SampleProgress
        {
            get { return sampleProgress; }
            set { this.RaiseAndSetIfChanged(ref sampleProgress, value); }
        }

        public SampleProgressViewModel SampleProgressFull
        {
            get { return sampleProgressFull; }
            set { this.RaiseAndSetIfChanged(ref sampleProgressFull, value); }
        }

        /// <summary>
        /// Updates the progress window with the sample data.
        /// </summary>
        /// <param name="sample"></param>
        public void UpdateSample(classSampleData sample)
        {
            SampleProgress.UpdateSample(sample);
            SampleProgressFull.UpdateSample(sample);
        }

        public void UpdateError(classSampleData sample, classLCEvent lcEvent)
        {
            SampleProgressFull.UpdateError(sample, lcEvent);
        }

        /// <summary>
        /// Updates preview label minute text.
        /// </summary>
        private void PreviewMinutesUpdated()
        {
            PreviewLabelText = Minutes + "-minute-preview";
            SampleProgress.PreviewMinutes = Minutes;
            SampleProgressFull.PreviewMinutes = Minutes;
        }

        public event EventHandler<SampleProgressPreviewArgs> PreviewAvailable;

        private void UpdateDrawings(object sender)
        {
            SampleProgress.Refresh();
            SampleProgressFull.Refresh();

            if (PreviewAvailable != null)
            {
                try
                {
                    var drawVisual = new DrawingVisual();
                    var drawContext = drawVisual.RenderOpen();
                    sampleProgressFull.RenderGraph(drawContext, new Rect(0, 0, 800, 200));
                    drawContext.Close();
                    var rtb = new RenderTargetBitmap(800, 200, 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(drawVisual);
                    rtb.Freeze();

                    PreviewAvailable(this, new SampleProgressPreviewArgs(rtb.ToImage(), rtb.ToBitmapImage()));
                }
                catch
                {
                    classApplicationLogger.LogError(0, "Error attempting to update column sample progress.");
                }
            }
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

        public SampleProgressPreviewArgs(Image image, BitmapImage bmpImage)
        {
            disposed = false;
            PreviewImage = image;
            PreviewImageWpf = bmpImage;
        }

        /// <summary>
        /// Gets the preview image for the sample progress
        /// </summary>
        public Image PreviewImage { get; private set; }

        /// <summary>
        /// Gets the preview image for the sample progress
        /// </summary>
        public BitmapImage PreviewImageWpf { get; private set; }

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
