using System;
using System.Collections.Generic;
using System.Text;

namespace HeartRateOverlay.Abstractions
{
    public interface IOverlayService
    {
        void StartOverlay();
        void StopOverlay();
        void UpdateHeartRate(int bpm);
        void CheckOverlayPermission();

    }
}
