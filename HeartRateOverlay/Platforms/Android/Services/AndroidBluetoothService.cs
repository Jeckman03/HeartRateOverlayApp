using HeartRateOverlay.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Android.Bluetooth;
using Android.Content;
using Java.Lang;

namespace HeartRateOverlay.Platforms.Android.Services
{
    public partial class AndroidBluetoothService : IBluetoothService
    {
        private BluetoothManager _bluetoothManager;
        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothGatt _bluetoothGatt;

        public event EventHandler<int> HeartRateUpdated;

        public AndroidBluetoothService()
        {
            _bluetoothManager = (BluetoothManager)Platform.AppContext.GetSystemService(Context.BluetoothService);
            _bluetoothAdapter = _bluetoothManager.Adapter;
        }

        public Task ConnectAsync()
        {
            // TODO: In the next step, we will add the code here to scan for your specific 
            // Heart Rate monitor and tell _bluetoothGatt to connect to it.
        }

        public void Disconnect()
        {
            _bluetoothGatt?.Disconnect();
            _bluetoothGatt?.Close();
            _bluetoothGatt = null;
        }

        private class GattCallBack : BluetoothGattCallback
        {
            private readonly AndroidBluetoothService _parentService;

            public GattCallBack(AndroidBluetoothService parentService)
            {
                _parentService = parentService;
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                base.OnCharacteristicChanged(gatt, characteristic);

                if (characteristic.Uuid.ToString().StartsWith("00002a37"))
                {
                    int heartRate = ExtractHeartRate(characteristic.GetValue());

                    _parentService.HeartRateUpdated?.Invoke(_parentService, heartRate);
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
