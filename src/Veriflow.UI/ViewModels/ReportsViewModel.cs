using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for REPORTS page (F6)
/// Camera and Sound Report generation
/// </summary>
public partial class ReportsViewModel : ViewModelBase
{
    private readonly IReportEngine _reportEngine;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private CancellationTokenSource? _cancellationTokenSource;
    
    [ObservableProperty]
    private ReportData _reportData = new();
    
    [ObservableProperty]
    private ObservableCollection<ReportClip> _clips = new();
    
    [ObservableProperty]
    private ReportType _reportType = ReportType.Camera;
    
    [ObservableProperty]
    private bool _isGenerating;
    
    public ReportsViewModel(
        IReportEngine reportEngine,
        IDialogService dialogService,
        ISessionService sessionService)
    {
        _reportEngine = reportEngine;
        _dialogService = dialogService;
        _sessionService = sessionService;
        
        InitializeReportData();
        StatusMessage = "Ready to generate reports";
    }
    
    private void InitializeReportData()
    {
        // Load session data if available
        var session = _sessionService.GetCurrentSession();
        if (session != null)
        {
            ReportData.ProjectName = session.ProjectName;
            ReportData.Date = session.CreatedDate;
        }
        
        // Add sample clips for demonstration
        Clips.Add(new ReportClip
        {
            Scene = "001",
            Take = "A",
            Timecode = "01:00:00:00",
            Duration = "00:02:30",
            FileName = "A001_C001_001.mov",
            Resolution = "1920x1080",
            FrameRate = "24",
            Codec = "ProRes 422",
            IsGood = true
        });
    }
    
    [RelayCommand]
    private async Task GenerateCameraReportAsync()
    {
        if (string.IsNullOrWhiteSpace(ReportData.ProjectName))
        {
            StatusMessage = "Please enter project name";
            return;
        }
        
        var outputPath = await _dialogService.ShowSaveFileDialogAsync(
            "Save Camera Report",
            $"CameraReport_{DateTime.Now:yyyyMMdd}.pdf",
            "PDF Files|*.pdf"
        );
        
        if (string.IsNullOrEmpty(outputPath)) return;
        
        IsGenerating = true;
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            StatusMessage = "Generating Camera Report...";
            
            // Update report data with clips
            ReportData.Clips = Clips.ToList();
            
            var result = await _reportEngine.GenerateCameraReportAsync(
                ReportData,
                outputPath,
                _cancellationTokenSource.Token
            );
            
            StatusMessage = $"Camera Report generated: {System.IO.Path.GetFileName(result)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsGenerating = false;
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task GenerateSoundReportAsync()
    {
        if (string.IsNullOrWhiteSpace(ReportData.ProjectName))
        {
            StatusMessage = "Please enter project name";
            return;
        }
        
        var outputPath = await _dialogService.ShowSaveFileDialogAsync(
            "Save Sound Report",
            $"SoundReport_{DateTime.Now:yyyyMMdd}.pdf",
            "PDF Files|*.pdf"
        );
        
        if (string.IsNullOrEmpty(outputPath)) return;
        
        IsGenerating = true;
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            StatusMessage = "Generating Sound Report...";
            
            // Update report data with clips
            ReportData.Clips = Clips.ToList();
            
            var result = await _reportEngine.GenerateSoundReportAsync(
                ReportData,
                outputPath,
                _cancellationTokenSource.Token
            );
            
            StatusMessage = $"Sound Report generated: {System.IO.Path.GetFileName(result)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsGenerating = false;
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private void AddClip()
    {
        var newClip = new ReportClip
        {
            Scene = $"{Clips.Count + 1:000}",
            Take = "A",
            Timecode = "01:00:00:00",
            Duration = "00:00:00",
            FileName = "New_Clip.mov",
            Resolution = "1920x1080",
            FrameRate = "24",
            IsGood = true
        };
        
        Clips.Add(newClip);
        StatusMessage = "Clip added";
    }
    
    [RelayCommand]
    private void RemoveClip(ReportClip? clip)
    {
        if (clip != null)
        {
            Clips.Remove(clip);
            StatusMessage = "Clip removed";
        }
    }
    
    [RelayCommand]
    private void ClearClips()
    {
        Clips.Clear();
        StatusMessage = "All clips cleared";
    }
    
    [RelayCommand]
    private async Task ImportClipsFromSessionAsync()
    {
        // TODO: Import clips from current session
        StatusMessage = "Import clips from session";
        await Task.CompletedTask;
    }
}
