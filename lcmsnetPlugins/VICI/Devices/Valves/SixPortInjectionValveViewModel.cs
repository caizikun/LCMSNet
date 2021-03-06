﻿using System.Reactive;
using FluidicsSDK.Devices;
using LcmsNetDataClasses.Devices;
using LcmsNetSDK;

namespace LcmsNet.Devices.Valves
{
    public class SixPortInjectionValveViewModel : ValveVICI2PosViewModel
    {
        public SixPortInjectionValveViewModel()
        {
            SetInjectionVolumeCommand = ReactiveUI.ReactiveCommand.Create(() => SetInjectionVolume());
        }

        private void SetInjectionVolume()
        {
            var injector = Device as ISixPortInjectionValve;
            if (injector != null)
                injector.InjectionVolume = InjectionVolume;
        }

        protected override void RegisterDevice(IDevice device)
        {
            base.RegisterDevice(device);

            var injector = Device as ISixPortInjectionValve;
            if (injector != null)
                InjectionVolume = injector.InjectionVolume;
        }

        private double injectionVolume = 0;

        public double InjectionVolume
        {
            get { return injectionVolume; }
            set { this.RaiseAndSetIfChanged(ref injectionVolume, value); }
        }

        public ReactiveUI.ReactiveCommand<Unit, Unit> SetInjectionVolumeCommand { get; private set; }
    }
}
