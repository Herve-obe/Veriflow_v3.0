using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Services;
using Veriflow.UI.ViewModels;

namespace Veriflow.UI;

/// <summary>
/// Dependency Injection configuration
/// </summary>
public static class ServiceConfiguration
{
    public static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Core Services
        services.AddSingleton<ISessionService, SessionService>();
        
        // TODO: Add other services as they are implemented
        // services.AddSingleton<IAudioEngine, MiniAudioEngine>();
        // services.AddSingleton<IMediaService, FFmpegMediaService>();
        
        // ViewModels - Main
        services.AddSingleton<MainWindowViewModel>();
        
        // ViewModels - Modules (Transient for fresh instances)
        services.AddTransient<OffloadViewModel>();
        services.AddTransient<MediaViewModel>();
        services.AddTransient<PlayerViewModel>();
        services.AddTransient<SyncViewModel>();
        services.AddTransient<TranscodeViewModel>();
        services.AddTransient<ReportsViewModel>();
        
        // Set ViewLocator
        var locator = new ViewLocator();
        
        return services.BuildServiceProvider();
    }
}
