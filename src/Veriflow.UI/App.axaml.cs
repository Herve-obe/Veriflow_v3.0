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
            var mainWindow = new MainWindow();
            
            // Configure Dependency Injection with MainWindow reference
            _serviceProvider = ServiceConfiguration.ConfigureServices(mainWindow);
            
            var mainViewModel = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_serviceProvider, _serviceProvider);
            mainWindow.DataContext = mainViewModel;
            
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