using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;
using Veriflow.UI.Views;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// Main window ViewModel
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISessionService _sessionService;
    private readonly IServiceProvider _serviceProvider;
    
    [ObservableProperty]
    private ProfileMode _currentProfile = ProfileMode.Video;
    
    [ObservableProperty]
    private string _currentPage = "OFFLOAD";
    
    [ObservableProperty]
    private string _windowTitle = "Veriflow 3.0";
    
    [ObservableProperty]
    private ViewModelBase? _currentPageViewModel;
    
    public MainWindowViewModel(ISessionService sessionService, IServiceProvider serviceProvider)
    {
        _sessionService = sessionService;
        _serviceProvider = serviceProvider;
        UpdateWindowTitle();
        
        // Load initial page (OFFLOAD)
        NavigateToPage("OFFLOAD");
    }
    
    [RelayCommand]
    private void ToggleProfile()
    {
        CurrentProfile = CurrentProfile == ProfileMode.Video ? ProfileMode.Audio : ProfileMode.Video;
        _sessionService.CurrentSession.CurrentProfile = CurrentProfile;
        _sessionService.MarkAsModified();
    }
    
    [RelayCommand]
    private void NavigateToPage(string? pageName)
    {
        if (string.IsNullOrEmpty(pageName)) return;
        
        CurrentPage = pageName;
        _sessionService.CurrentSession.CurrentPage = pageName;
        _sessionService.MarkAsModified();
        
        // Create appropriate ViewModel based on page name
        CurrentPageViewModel = pageName switch
        {
            "OFFLOAD" => _serviceProvider.GetRequiredService<OffloadViewModel>(),
            "MEDIA" => _serviceProvider.GetRequiredService<MediaViewModel>(),
            "PLAYER" => _serviceProvider.GetRequiredService<PlayerViewModel>(),
            "SYNC" => _serviceProvider.GetRequiredService<SyncViewModel>(),
            "TRANSCODE" => _serviceProvider.GetRequiredService<TranscodeViewModel>(),
            "REPORTS" => _serviceProvider.GetRequiredService<ReportsViewModel>(),
            _ => _serviceProvider.GetRequiredService<OffloadViewModel>()
        };
        
        StatusMessage = $"Navigated to {pageName}";
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
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task SaveSessionAsync()
    {
        await _sessionService.SaveSessionAsync();
        UpdateWindowTitle();
    }
    
    [RelayCommand]
    private void ShowAbout()
    {
        var aboutWindow = new AboutWindow();
#pragma warning disable CS8625
        aboutWindow.ShowDialog(null);
#pragma warning restore CS8625
    }
    
    private void UpdateWindowTitle()
    {
        var sessionName = _sessionService.CurrentSession.Name;
        var modified = _sessionService.CurrentSession.IsModified ? "*" : "";
        WindowTitle = $"Veriflow 3.0 - {sessionName}{modified}";
    }
}
