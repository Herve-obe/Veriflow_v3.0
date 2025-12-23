using CommunityToolkit.Mvvm.ComponentModel;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// Base class for all ViewModels
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
}
