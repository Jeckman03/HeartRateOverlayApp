using Android.Provider;
using Android.Content;
using HeartRateOverlay.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Android.Views;
using Android.Widget;
using Android.Runtime;
using Android.Graphics;
using Color = Android.Graphics.Color;

namespace HeartRateOverlay.Platforms.Android.Services
{
    public partial class AndroidOverlayService : IOverlayService
    {
        private IWindowManager _windowManager;
        private global::Android.Views.View _floatingView;
        private TextView _heartRateText;

        public AndroidOverlayService()
        {
            var context = Platform.AppContext;
            _windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        }

        public void StartOverlay()
        {
            if (_floatingView != null) return;

            var context = Platform.AppContext;

            // 1. READ THE SAVED SETTINGS
            string savedColor = Microsoft.Maui.Storage.Preferences.Default.Get("OverlayColor", "Red");
            string savedPosition = Microsoft.Maui.Storage.Preferences.Default.Get("OverlayPosition", "Top Right");
            string savedSize = Preferences.Default.Get("OverlaySize", "Medium");

            // 2. MAP THE COLOR
            var textColor = savedColor switch
            {
                "Green" => global::Android.Graphics.Color.ParseColor("#39FF14"),
                "Blue" => global::Android.Graphics.Color.ParseColor("#00CCFF"),
                "Purple" => global::Android.Graphics.Color.Purple,
                "Grey" => global::Android.Graphics.Color.Gray,
                "White" => global::Android.Graphics.Color.White,
                _ => global::Android.Graphics.Color.Red
            };

            // 3. MAP THE POSITION
            var gravity = savedPosition switch
            {
                "Top Left" => GravityFlags.Top | GravityFlags.Left,
                "Bottom Right" => GravityFlags.Bottom | GravityFlags.Right,
                "Bottom Left" => GravityFlags.Bottom | GravityFlags.Left,
                _ => GravityFlags.Top | GravityFlags.Right
            };

            // MAP THE SIZE
            float baseSize = savedSize switch
            {
                "Small" => 24f,
                "Large" => 48f,
                "Extra Large" => 64f,
                _ => 36f
            };

            // 4. BUILD THE NEW LAYOUT (Heart + Text)
            var layout = new global::Android.Widget.LinearLayout(context)
            {
                Orientation = global::Android.Widget.Orientation.Horizontal
            };
            layout.SetBackgroundColor(global::Android.Graphics.Color.Argb(180, 0, 0, 0));
            layout.SetPadding(30, 30, 30, 30);

            // Create the Heart Icon
            var heartIcon = new global::Android.Widget.TextView(context)
            {
                Text = "❤️",
                TextSize = baseSize - 2f
            };
            heartIcon.SetPadding(0, 0, 15, 0); // Add some space between heart and text

            // Create the BPM Text
            _heartRateText = new global::Android.Widget.TextView(context)
            {
                Text = "-- BPM",
                TextSize = baseSize
            };
            _heartRateText.SetTextColor(textColor); // Apply user's color!

            // Add them both to the container
            layout.AddView(heartIcon);
            layout.AddView(_heartRateText);
            _floatingView = layout;

            // 5. ANIMATE THE HEART
            var pulseAnimation = new global::Android.Views.Animations.ScaleAnimation(
                0.8f, 1.0f, // Start and End X scale
                0.8f, 1.0f, // Start and End Y scale
                global::Android.Views.Animations.Dimension.RelativeToSelf, 0.5f, // Pivot X center
                global::Android.Views.Animations.Dimension.RelativeToSelf, 0.5f  // Pivot Y center
            )
            {
                Duration = 400, // Speed of the pulse
                RepeatCount = -1, // -1 is the native Android integer for Infinite
                RepeatMode = global::Android.Views.Animations.RepeatMode.Reverse
            };
            heartIcon.StartAnimation(pulseAnimation);

            // 6. DRAW THE WINDOW
            var layoutParams = new WindowManagerLayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent,
                WindowManagerTypes.ApplicationOverlay,
                WindowManagerFlags.NotFocusable,
                Format.Translucent
            );

            layoutParams.Gravity = gravity; // Apply user's position!
            layoutParams.X = 0;
            layoutParams.Y = 100;

            _windowManager.AddView(_floatingView, layoutParams);
        }

        public void StopOverlay()
        {
            if ( _floatingView != null )
            {
                _windowManager.RemoveView(_floatingView);
                _floatingView = null;
                _heartRateText = null;
            }
        }

        public void UpdateHeartRate(int bpm)
        {
            if (_heartRateText != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _heartRateText.Text = $"{bpm} BPM";
                });
            }
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
