using System;
using System.IO;
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
    
    // Computed property for profile toggle UI
    public bool IsVideoMode => CurrentProfile == ProfileMode.Video;
    
    [ObservableProperty]
    private string _currentPage = "OFFLOAD";
    
    [ObservableProperty]
    private string _windowTitle = "Veriflow 3.0";
    
    [ObservableProperty]
    private string _statusMessage = "Ready";
    
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
        StatusMessage = $"Switched to {CurrentProfile} mode";
        OnPropertyChanged(nameof(IsVideoMode));
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
        var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
        var filters = new[] { "Veriflow Session|*.vfsession", "All Files|*.*" };
        
        var files = await dialogService.ShowFilePickerAsync("Open Session", false, filters);
        if (files != null && files.Length > 0)
        {
            var filePath = files[0];
            await _sessionService.LoadSessionAsync(filePath);
            UpdateWindowTitle();
            StatusMessage = $"Session loaded: {Path.GetFileName(filePath)}";
        }
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
    
    [RelayCommand]
    private void Exit()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
    }
    
    [RelayCommand]
    private void OpenLogFolder()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Veriflow",
            "Logs"
        );
        
        // Create directory if it doesn't exist
        Directory.CreateDirectory(logPath);
        
        // Open in Windows Explorer
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = logPath,
                UseShellExecute = true,
                Verb = "open"
            });
            StatusMessage = "Log folder opened";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open log folder: {ex.Message}";
        }
    }
    
    private void UpdateWindowTitle()
    {
        var sessionName = _sessionService.CurrentSession.Name;
        var modified = _sessionService.CurrentSession.IsModified ? "*" : "";
        WindowTitle = $"Veriflow 3.0 - {sessionName}{modified}";
    }
}
