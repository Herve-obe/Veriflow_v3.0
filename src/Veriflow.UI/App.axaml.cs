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
        // Configure Dependency Injection
        _serviceProvider = ServiceConfiguration.ConfigureServices();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}