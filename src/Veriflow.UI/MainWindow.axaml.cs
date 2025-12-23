using Avalonia.Controls;
using Avalonia.Input;
using Veriflow.UI.ViewModels;

namespace Veriflow.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        // Ctrl+Tab: Toggle profile (v1 feature)
        if (e.Key == Key.Tab && e.KeyModifiers == KeyModifiers.Control)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ToggleProfileCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}