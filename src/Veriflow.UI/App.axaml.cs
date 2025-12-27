using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Veriflow.UI.ViewModels;
using Veriflow.Core.Services;
using System;
using System.Threading.Tasks;

namespace Veriflow.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Initialize crash logging first
        CrashLogger.Initialize();
        CrashLogger.LogInfo("Veriflow 3.0 starting...");
        
        // Setup unhandled exception handlers
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            CrashLogger.LogException(ex ?? new Exception("Unknown exception"), "Unhandled AppDomain Exception");
        };
        
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            CrashLogger.LogException(e.Exception, "Unobserved Task Exception");
            e.SetObserved();
        };
        
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Create a temporary window for initial service configuration
            var tempWindow = new Avalonia.Controls.Window();
            
            // Configure services
            _serviceProvider = ServiceConfiguration.ConfigureServices(tempWindow);
            
            // Get the window settings service
            var windowSettingsService = _serviceProvider.GetRequiredService<Services.IWindowSettingsService>();
            
            // Create the actual main window with dependencies
            var mainWindow = new MainWindow(windowSettingsService);
            
            // Get MainWindowViewModel and set as DataContext
            var mainViewModel = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_serviceProvider, _serviceProvider);
            mainWindow.DataContext = mainViewModel;
            
            // Set the main window
            desktop.MainWindow = mainWindow;
            
            // Shutdown handler
            desktop.ShutdownRequested += (s, e) =>
            {
                CrashLogger.LogInfo("Veriflow 3.0 shutting down...");
                CrashLogger.Shutdown();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}