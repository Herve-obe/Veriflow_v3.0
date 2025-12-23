using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// Main window ViewModel
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISessionService _sessionService;
    
    [ObservableProperty]
    private ProfileMode _currentProfile = ProfileMode.Video;
    
    [ObservableProperty]
    private string _currentPage = "OFFLOAD";
    
    [ObservableProperty]
    private string _windowTitle = "Veriflow 3.0";
    
    public MainWindowViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
        UpdateWindowTitle();
    }
    
    [RelayCommand]
    private void ToggleProfile()
    {
        CurrentProfile = CurrentProfile == ProfileMode.Video ? ProfileMode.Audio : ProfileMode.Video;
        _sessionService.CurrentSession.CurrentProfile = CurrentProfile;
        _sessionService.MarkAsModified();
    }
    
    [RelayCommand]
    private void NavigateToPage(string pageName)
    {
        CurrentPage = pageName;
        _sessionService.CurrentSession.CurrentPage = pageName;
        _sessionService.MarkAsModified();
    }
    
    [RelayCommand]
    private void NewSession()
    {
        _sessionService.NewSession();
        UpdateWindowTitle();
    }
    
    [RelayCommand]
    private async Task OpenSessionAsync()
    {
        // TODO: Show file picker dialog
        // await _sessionService.LoadSessionAsync(filePath);
        UpdateWindowTitle();
    }
    
    [RelayCommand]
    private async Task SaveSessionAsync()
    {
        await _sessionService.SaveSessionAsync();
        UpdateWindowTitle();
    }
    
    private void UpdateWindowTitle()
    {
        var sessionName = _sessionService.CurrentSession.Name;
        var modified = _sessionService.CurrentSession.IsModified ? "*" : "";
        WindowTitle = $"Veriflow 3.0 - {sessionName}{modified}";
    }
}
