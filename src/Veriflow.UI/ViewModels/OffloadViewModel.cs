using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for OFFLOAD page (F1)
/// Secure dual-destination file copying with MHL verification
/// </summary>
public partial class OffloadViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _sourceFolder = string.Empty;
    
    [ObservableProperty]
    private string _destinationA = string.Empty;
    
    [ObservableProperty]
    private string _destinationB = string.Empty;
    
    [ObservableProperty]
    private bool _isOffloadMode = true; // true = OFFLOAD, false = VERIFY
    
    [ObservableProperty]
    private double _progress;
    
    [ObservableProperty]
    private string _currentFile = string.Empty;
    
    public OffloadViewModel()
    {
        StatusMessage = "Ready to offload";
    }
    
    [RelayCommand]
    private void SelectSourceFolder()
    {
        // TODO: Implement folder picker
        StatusMessage = "Select source folder";
    }
    
    [RelayCommand]
    private void SelectDestinationA()
    {
        // TODO: Implement folder picker
        StatusMessage = "Select destination A";
    }
    
    [RelayCommand]
    private void SelectDestinationB()
    {
        // TODO: Implement folder picker
        StatusMessage = "Select destination B";
    }
    
    [RelayCommand]
    private async Task StartOffloadAsync()
    {
        IsBusy = true;
        StatusMessage = "Offloading...";
        
        try
        {
            // TODO: Implement offload logic
            await Task.Delay(100); // Placeholder
        }
        finally
        {
            IsBusy = false;
            StatusMessage = "Offload complete";
        }
    }
    
    [RelayCommand]
    private void ToggleMode()
    {
        IsOffloadMode = !IsOffloadMode;
        StatusMessage = IsOffloadMode ? "OFFLOAD mode" : "VERIFY mode";
    }
}
