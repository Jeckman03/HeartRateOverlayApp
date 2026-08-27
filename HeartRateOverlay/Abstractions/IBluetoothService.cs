using System;
using System.Collections.Generic;
using System.Text;

namespace HeartRateOverlay.Abstractions
{
    public interface IBluetoothService
    {
        Task ConnectToDeviceAsync(string deviceId);
        void Disconnect();
        void StartScanning();
        void StopScanning();

        event EventHandler<int> HeartRateUpdated;
        event EventHandler<DiscoveredDevice> DeviceDiscovered;

    }
}
