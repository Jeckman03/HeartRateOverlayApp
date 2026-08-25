using Android.Provider;
using Android.Content;
using HeartRateOverlay.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace HeartRateOverlay.Platforms.Android.Services
{
    public partial class AndroidOverlayService : IOverlayService
    {
        public void StartOverlay()
        {
            throw new NotImplementedException();
        }

        public void StopOverlay()
        {
            throw new NotImplementedException();
        }

        public void UpdateHeartRate(int bpm)
        {
            throw new NotImplementedException();
        }

        public void CheckOverlayPermission()
        {
            var context = Platform.AppContext;

            if (!Settings.CanDrawOverlays(context))
            {
                var intent = new Intent(Settings.ActionManageOverlayPermission);

                intent.SetData(global::Android.Net.Uri.Parse("package:" + context.PackageName));

                intent.AddFlags(ActivityFlags.NewTask);

                context.StartActivity(intent);
            }
        }
    }
}
