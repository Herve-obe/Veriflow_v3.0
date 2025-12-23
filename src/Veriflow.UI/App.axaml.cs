using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Veriflow.UI.ViewModels;

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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            
            // Configure Dependency Injection with MainWindow reference
            _serviceProvider = ServiceConfiguration.ConfigureServices(mainWindow);
            
            var mainViewModel = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_serviceProvider, _serviceProvider);
            mainWindow.DataContext = mainViewModel;
            
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}