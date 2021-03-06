﻿using System;
using FluidicsSDK;
using FluidicsSDK.ModelCheckers;
using FluidicsSimulator;
using LcmsNet.Devices.Fluidics.ViewModels;
using ReactiveUI;

namespace LcmsNet.Simulator.ViewModels
{
    public class SimConfigurationViewModel : ReactiveObject
    {
        /// <summary>
        /// Calling this constructor is only for the windows WPF designer.
        /// </summary>
        [Obsolete("For WPF Design time use only.", true)]
        public SimConfigurationViewModel() : this(null)
        {
            instance = this;
        }

        /// <summary>
        /// The real constructor
        /// </summary>
        /// <param name="anything">really, anything; only there to differentiate from the public no-parameter constructor</param>
        private SimConfigurationViewModel(object anything)
        {
            mod = FluidicsModeratorWpf.Moderator;
            mod.ModelChanged += ModelChangedHandler;
            Elapsed = "+00:00:00";

            FluidicsSimulator.FluidicsSimulator.GetInstance.EventSimulated += EventSimulated_Handler;

            var sinkCheck = new NoSinksModelCheck();
            sinkCheck.IsEnabled = false;

            var sourceCheck = new MultipleSourcesModelCheck();
            sourceCheck.IsEnabled = false;

            var cycleCheck = new FluidicsCycleCheck();
            cycleCheck.IsEnabled = false;

            var testCheck = new TestModelCheck();
            testCheck.IsEnabled = false;

            FluidicsSimulator.FluidicsSimulator.GetInstance.AddModelCheck(sinkCheck);
            FluidicsSimulator.FluidicsSimulator.GetInstance.AddModelCheck(sourceCheck);
            FluidicsSimulator.FluidicsSimulator.GetInstance.AddModelCheck(cycleCheck);
            FluidicsSimulator.FluidicsSimulator.GetInstance.AddModelCheck(testCheck);
            //fluidicsControlVm = new FluidicsControlViewModel();
        }

        //private FluidicsControlViewModel fluidicsControlVm;
        private static SimConfigurationViewModel instance;
        private readonly FluidicsModeratorWpf mod;
        private readonly FluidicsControlViewModel fluidicsControlVm = new FluidicsControlViewModel();
        private string elapsed;

        public FluidicsControlViewModel FluidicsControlVm => fluidicsControlVm;

        public static SimConfigurationViewModel GetInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SimConfigurationViewModel(null);
                }
                return instance;
            }
        }

        public string Elapsed
        {
            get { return elapsed; }
            private set { this.RaiseAndSetIfChanged(ref elapsed, value); }
        }

        public event EventHandler RefreshImage;

        private void EventSimulated_Handler(object sender, SimulatedEventArgs e)
        {
            Elapsed = "+" + e.SimulatedTimeElapsed.ToString(@"%d\.hh\:mm\:ss");
        }

        public void UpdateImage()
        {
            //fluidicsControlVm.UpdateImage();
            RefreshImage?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// event handler for when a fluidics model changes.
        /// </summary>
        private void ModelChangedHandler()
        {
            ChangeHandler(this, new EventArgs());
        }

        private void ChangeHandler(object sender, EventArgs e)
        {
            //fluidicsControlVm.UpdateImage();
            RefreshImage?.Invoke(this, e);
        }
    }
}
