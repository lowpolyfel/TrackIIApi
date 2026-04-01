using Microsoft.Extensions.Logging;
using Trackii.AdminApp.Services;
using Trackii.AdminApp.ViewModels;
using Trackii.AdminApp.Views;

namespace Trackii.AdminApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(ApiConstants.ApiBaseUrl)
        });

        builder.Services.AddSingleton<ITrackiiAdminApiClient, TrackiiAdminApiClient>();

        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<RouteViewModel>();
        builder.Services.AddTransient<SummaryViewModel>();

        builder.Services.AddTransient<ScanPage>();
        builder.Services.AddTransient<RoutePage>();
        builder.Services.AddTransient<SummaryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
