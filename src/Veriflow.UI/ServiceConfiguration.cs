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
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        
        return services.BuildServiceProvider();
    }
}
