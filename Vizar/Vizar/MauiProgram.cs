using Microsoft.Extensions.Logging;

using Syncfusion.Blazor;

using Toolbelt.Blazor.Extensions.DependencyInjection;

using Vizar.Services;
using Vizar.Shared.Services;

using VizarLibrary.DataAccess;

namespace Vizar;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        Dapper.SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Secrets.SyncfusionLicense);

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the Vizar.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddSingleton<ISaveAndViewService, SaveAndViewService>();
        builder.Services.AddSingleton<IUpdateService, UpdateService>();
        builder.Services.AddSingleton<IDataStorageService, DataStorageService>();
        builder.Services.AddSingleton<IVibrationService, VibrationService>();
        builder.Services.AddSingleton<ISoundService, SoundService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSyncfusionBlazor();
        builder.Services.AddHotKeys2();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
