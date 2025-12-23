using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Services;
using Veriflow.Infrastructure.Services;
using Veriflow.UI.Services;
using Veriflow.UI.ViewModels;

namespace Veriflow.UI;

/// <summary>
/// Dependency Injection configuration
/// </summary>
public static class ServiceConfiguration
{
    public static ServiceProvider ConfigureServices(Window mainWindow)
    {
        var services = new ServiceCollection();
        
        // Core Services
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IOffloadService, OffloadService>();
        services.AddSingleton<IMediaService, FFmpegMediaService>();
        services.AddSingleton<IAudioEngine, NAudioEngine>();
        services.AddSingleton<IVideoEngine, LibVLCVideoEngine>();
        services.AddSingleton<ISyncEngine, FFmpegSyncEngine>();
        services.AddSingleton<ITranscodeEngine, FFmpegTranscodeEngine>();
        services.AddSingleton<IReportEngine, QuestPDFReportEngine>();
        
        // UI Services
        services.AddSingleton<IDialogService>(sp => new AvaloniaDialogService(mainWindow));
        
        // TODO: Add other services as they are implemented
        // services.AddSingleton<IAudioEngine, MiniAudioEngine>();
        
        // ViewModels - Main
        services.AddSingleton<MainWindowViewModel>();
        
        // ViewModels - Modules (Transient for fresh instances)
        services.AddTransient<OffloadViewModel>();
        services.AddTransient<MediaViewModel>();
        services.AddTransient<PlayerViewModel>();
        services.AddTransient<SyncViewModel>();
        services.AddTransient<TranscodeViewModel>();
        services.AddTransient<ReportsViewModel>();
        
        return services.BuildServiceProvider();
    }
}

