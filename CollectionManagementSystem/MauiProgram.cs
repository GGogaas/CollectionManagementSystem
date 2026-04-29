using CollectionManagementSystem.Services;
using CollectionManagementSystem.ViewModels;
using CollectionManagementSystem.Views;
using Microsoft.Extensions.Logging;

namespace CollectionManagementSystem;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<FileStorageService>();

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<CollectionViewModel>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<AddCollectionViewModel>();
        builder.Services.AddTransient<CollectionSummaryViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CollectionPage>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<AddCollectionPage>();
        builder.Services.AddTransient<CollectionSummaryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
