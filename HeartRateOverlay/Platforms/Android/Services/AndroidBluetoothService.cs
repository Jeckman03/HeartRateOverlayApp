using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Runtime;
using HeartRateOverlay.Abstractions;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Text;
using static AndroidX.RecyclerView.Widget.AsyncListUtil;

namespace HeartRateOverlay.Platforms.Android.Services
{
    public partial class AndroidBluetoothService : IBluetoothService
    {
        private BluetoothManager _bluetoothManager;
        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothGatt _bluetoothGatt;
        private BluetoothLeScanner _scanner;
        private HeartRateScanCallback _scanCallback;

        public event EventHandler<int> HeartRateUpdated;
        public event EventHandler<DiscoveredDevice> DeviceDiscovered;

        private Dictionary<string, BluetoothDevice> _foundDevices = new();

        public AndroidBluetoothService()
        {
            _bluetoothManager = (BluetoothManager)Platform.AppContext.GetSystemService(Context.BluetoothService);
            _bluetoothAdapter = _bluetoothManager.Adapter;
        }

        public void StartScanning()
        {
            if (_bluetoothAdapter == null || !_bluetoothAdapter.IsEnabled) return;

            _scanner = _bluetoothAdapter.BluetoothLeScanner;
            _scanCallback = new HeartRateScanCallback(this);
            _foundDevices.Clear();

            var hrUuid = global::Android.OS.ParcelUuid.FromString("0000180d-0000-1000-8000-00805f9b34fb");
            var filter = new global::Android.Bluetooth.LE.ScanFilter.Builder().SetServiceUuid(hrUuid).Build();

            _scanner.StartScan(
                new System.Collections.Generic.List<global::Android.Bluetooth.LE.ScanFilter> { filter },
                new global::Android.Bluetooth.LE.ScanSettings.Builder().SetScanMode(global::Android.Bluetooth.LE.ScanMode.LowLatency).Build(),
                _scanCallback);
        }

        public void StopScanning()
        {
            _scanner?.StartScan(_scanCallback);
        }

        public async Task ConnectToDeviceAsync(string deviceId)
        {
            StopScanning(); // Always stop scanning before connecting

            if (_foundDevices.TryGetValue(deviceId, out var device))
            {
                _bluetoothGatt = device.ConnectGatt(Microsoft.Maui.ApplicationModel.Platform.AppContext, false, new GattCallBack(this));
            }

            await Task.CompletedTask;

        }

        public void Disconnect()
        {
            _bluetoothGatt?.Disconnect();
            _bluetoothGatt?.Close();
            _bluetoothGatt = null;
        }

        private class HeartRateScanCallback : global::Android.Bluetooth.LE.ScanCallback
        {
            private readonly AndroidBluetoothService _parent;

            public HeartRateScanCallback(AndroidBluetoothService parent)
            {
                _parent = parent;
            }

            public override void OnScanResult(global::Android.Bluetooth.LE.ScanCallbackType callbackType, global::Android.Bluetooth.LE.ScanResult result)
            {
                base.OnScanResult(callbackType, result);

                var device = result.Device;
                var address = device.Address;

                // Only add it if we haven't seen it yet
                if (!_parent._foundDevices.ContainsKey(address))
                {
                    _parent._foundDevices[address] = device;

                    // Name might be null, provide a fallback
                    var name = string.IsNullOrEmpty(device.Name) ? "Unknown HR Monitor" : device.Name;

                    _parent.DeviceDiscovered?.Invoke(_parent, new DiscoveredDevice { Name = name, Id = address });
                }
            }
        }

        private class GattCallBack : BluetoothGattCallback
        {
            private readonly AndroidBluetoothService _parent;

            // Standard Bluetooth SIG UUIDs
            private readonly Java.Util.UUID _hrServiceUuid = Java.Util.UUID.FromString("0000180d-0000-1000-8000-00805f9b34fb");
            private readonly Java.Util.UUID _hrCharacteristicUuid = Java.Util.UUID.FromString("00002a37-0000-1000-8000-00805f9b34fb");
            private readonly Java.Util.UUID _clientConfigUuid = Java.Util.UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

            public GattCallBack(AndroidBluetoothService parent)
            {
                _parent = parent;
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
            {
                if (newState == ProfileState.Connected)
                {
                    // We connected! Now ask the device what services it has.
                    gatt.DiscoverServices();
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
            {
                if (status == GattStatus.Success)
                {
                    var service = gatt.GetService(_hrServiceUuid);
                    if (service != null)
                    {
                        var characteristic = service.GetCharacteristic(_hrCharacteristicUuid);
                        if (characteristic != null)
                        {
                            // Tell Android to listen to this characteristic
                            gatt.SetCharacteristicNotification(characteristic, true);

                            // Write to the device's descriptor to physically turn on the data stream
                            var descriptor = characteristic.GetDescriptor(_clientConfigUuid);
                            if (descriptor != null)
                            {
                                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                                gatt.WriteDescriptor(descriptor);
                            }
                        }
                    }
                }
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                if (characteristic.Uuid.Equals(_hrCharacteristicUuid))
                {
                    int heartRate = ExtractHeartRate(characteristic.GetValue());
                    _parent.HeartRateUpdated?.Invoke(_parent, heartRate);
                }
            }

            private int ExtractHeartRate(byte[] data)
            {
                if (data == null || data.Length == 0) return 0;
                byte flags = data[0];
                bool isFormat16Bit = (flags & 0x01) != 0;
                return isFormat16Bit ? (data[1] | (data[2] << 8)) : data[1];
            }
        }
    }
}
