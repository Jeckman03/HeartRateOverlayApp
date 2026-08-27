using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HeartRateOverlay.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using HeartRateOverlay.Models;

namespace HeartRateOverlay.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IOverlayService _overlayService;

        public ObservableCollection<DiscoveredDevice> Devices { get; } = new();
        public ObservableCollection<string> Positions { get; } = new() { "Top Right", "Top Left", "Bottom Right", "Bottom Left" };
        public ObservableCollection<string> Colors { get; } = new() { "Red", "Green", "Blue", "White", "Purple", "Grey" };

        public ObservableCollection<InstructionStep> Instructions { get; } = new()
        {
            new InstructionStep { Icon = "🔓", Title = "1. Permissions", Description = "Tap the gear icon and ensure Bluetooth and 'Display Over Other Apps' permissions are granted." },
            new InstructionStep { Icon = "🫀", Title = "2. Gear Up", Description = "Make sure your heart rate device is turned on and ready to pair." },
            new InstructionStep { Icon = "📡", Title = "3. Lock On", Description = "Tap 'Scan', wait for your heart rate monitor to appear in the list, and tap it to connect." },
            new InstructionStep { Icon = "👍", Title = "4. Activate Overlay", Description = "Hit 'Toggle Overlay'and you are all set!" },
            new InstructionStep { Icon = "ℹ️", Title = "5. How To Cast", Description = "When casting your phone, use the main cast button in the top drop-down menu instead of the app's built-in cast button, as some apps will not work otherwise." }
        };

        [ObservableProperty]
        private int _currentHeartRate;

        [ObservableProperty]
        private bool _isOverlayActive;

        [ObservableProperty]
        private string _selectedPosition;

        [ObservableProperty]
        private string _selectedColor;


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

            _bluetoothService.DeviceDiscovered += (sender, device) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!Devices.Any(d => d.Id == device.Id))
                    {
                        Devices.Add(device);
                    }
                });
            };

            SelectedPosition = Preferences.Default.Get("OverlayPosition", "Top Right");
            SelectedColor = Preferences.Default.Get("OverlayColor", "Red");
        }

        partial void OnSelectedPositionChanged(string value)
        {
            Preferences.Default.Set("OverlayPosition", value);
        }

        partial void OnSelectedColorChanged(string value)
        {
            Preferences.Default.Set("OverlayColor", value);
        }

        [RelayCommand]
        private void ScanForDevices()
        {
            Devices.Clear();
            _bluetoothService.StartScanning();
        }

        [RelayCommand]
        private async Task ConnectToDeviceAsync(DiscoveredDevice selectedDevice)
        {
            if (selectedDevice == null) return;

            await _bluetoothService.ConnectToDeviceAsync(selectedDevice.Id);
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

        [RelayCommand]
        private async Task RequestBluetoothPermissionsAsync()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();

            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Bluetooth>();
            }
        }

        [RelayCommand]
        private void RequestOverlayPermission()
        {
            _overlayService.CheckOverlayPermission();
        }
    }
}
