using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HeartRateOverlay.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace HeartRateOverlay.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IOverlayService _overlayService;

        [ObservableProperty]
        private int _currentHeartRate;

        [ObservableProperty]
        private bool _isOverlayActive;


        public MainViewModel(IBluetoothService bluetoothService, IOverlayService overlayService)
        {
            _bluetoothService = bluetoothService;
            _overlayService = overlayService;

            // Listen for heart rate updates and push them to the overlay
            _bluetoothService.HeartRateUpdated += (sender, bpm) =>
            {
                CurrentHeartRate = bpm;
                if (IsOverlayActive)
                {
                    _overlayService.UpdateHeartRate(bpm);
                }
            };
        }

        [RelayCommand]
        private async Task ConnectSensorAsync()
        {
            await _bluetoothService.ConnectAsync();
        }

        [RelayCommand]
        private void ToggleOverlay()
        {
            if (IsOverlayActive)
            {
                _overlayService.StopOverlay();
                IsOverlayActive = false;
            }
            else
            {
                _overlayService.StartOverlay();
                IsOverlayActive = true;
            }
        }

        private async Task<bool> CheckAnsRequestBluetoothPermission()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();

            if (status == PermissionStatus.Granted)
                return true;

            status = await Permissions.RequestAsync<Permissions.Bluetooth>();

            return status == PermissionStatus.Granted;
        }
    }
}
