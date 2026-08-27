using CommunityToolkit.Maui;
using HeartRateOverlay.Abstractions;
using HeartRateOverlay.ViewModels;
using Microsoft.Extensions.Logging;

namespace HeartRateOverlay
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();

            // Registering Platform-Specific Services
#if ANDROID
            builder.Services.AddSingleton<IBluetoothService, Platforms.Android.Services.AndroidBluetoothService>();
            builder.Services.AddSingleton<IOverlayService, Platforms.Android.Services.AndroidOverlayService>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
