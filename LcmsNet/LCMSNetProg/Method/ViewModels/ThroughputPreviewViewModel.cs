﻿using System.Collections.Generic;
using LcmsNetDataClasses;
using LcmsNetDataClasses.Method;

namespace LcmsNet.Method.ViewModels
{
    /// <summary>
    /// Form that displays the throughput for the given samples.
    /// </summary>
    public class ThroughputPreviewViewModel : LCMethodTimelineViewModel
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ThroughputPreviewViewModel()
        {
        }

        /// <summary>
        /// Displays the alignment for the samples.
        /// </summary>
        /// <param name="samples"></param>
        public void ShowAlignmentForSamples(List<classSampleData> samples)
        {
            // Show the samples
            UpdateSampleMethods(samples);

            // Align the samples
            var optimizer = new classLCMethodOptimizer();
            optimizer.UpdateRequired += optimizer_UpdateRequired;
            optimizer.AlignSamples(samples);

            // Display end product
            if (optimizer.Methods != null)
                UpdateSampleMethods(samples);
        }

        /// <summary>
        /// Renders the alignment as it occurs.
        /// </summary>
        /// <param name="sender"></param>
        void optimizer_UpdateRequired(classLCMethodOptimizer sender)
        {
            RenderMethods(sender.Methods);
        }

        /// <summary>
        /// Renders the sample methods.
        /// </summary>
        void UpdateSampleMethods(List<classSampleData> samples)
        {
            var methods = new List<classLCMethod>();
            foreach (var sample in samples)
            {
                if (sample?.LCMethod == null)
                    continue;

                // Clone this, so we don't have multiple copies competing for the same times, methods, etc.
                sample.CloneLCMethod();
                methods.Add(sample.LCMethod);
            }

            if (methods.Count > 0)
                RenderMethods(methods);
        }

        /// <summary>
        /// Renders all of the methods.
        /// </summary>
        /// <param name="methods"></param>
        private void RenderMethods(List<classLCMethod> methods)
        {
            RenderLCMethod(methods);
        }
    }
}
