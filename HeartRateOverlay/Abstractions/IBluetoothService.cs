using System;
using System.Collections.Generic;
using System.Text;

namespace HeartRateOverlay.Abstractions
{
    public interface IBluetoothService
    {
        Task ConnectAsync();
        void Disconnect();
        event EventHandler<int> HeartRateUpdated;

    }
}
