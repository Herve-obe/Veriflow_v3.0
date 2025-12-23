using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for TRANSCODE page (F5)
/// Professional video/audio transcoding with queue management
/// </summary>
public partial class TranscodeViewModel : ViewModelBase
{
    private readonly ITranscodeEngine _transcodeEngine;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _cancellationTokenSource;
    
    [ObservableProperty]
    private ObservableCollection<TranscodeJob> _queue = new();
    
    [ObservableProperty]
    private ObservableCollection<TranscodePreset> _presets = new();
    
    [ObservableProperty]
    private TranscodePreset? _selectedPreset;
    
    [ObservableProperty]
    private TranscodeJob? _currentJob;
    
    [ObservableProperty]
    private bool _isProcessing;
    
    public TranscodeViewModel(ITranscodeEngine transcodeEngine, IDialogService dialogService)
    {
        _transcodeEngine = transcodeEngine;
        _dialogService = dialogService;
        
        // Load presets
        var availablePresets = _transcodeEngine.GetAvailablePresets();
        foreach (var preset in availablePresets)
        {
            Presets.Add(preset);
        }
        
        SelectedPreset = Presets.FirstOrDefault();
        StatusMessage = "Ready to transcode";
    }
    
    [RelayCommand]
    private async Task AddFilesToQueueAsync()
    {
        var files = await _dialogService.ShowFilePickerAsync("Select Media Files", true);
        if (files == null || files.Length == 0) return;
        
        if (SelectedPreset == null)
        {
            StatusMessage = "Please select a preset first";
            return;
        }
        
        foreach (var file in files)
        {
            var outputPath = GenerateOutputPath(file, SelectedPreset);
            
            var job = new TranscodeJob
            {
                InputPath = file,
                OutputPath = outputPath,
                PresetId = SelectedPreset.Id,
                PresetName = SelectedPreset.Name,
                Status = TranscodeStatus.Queued,
                InputSize = new System.IO.FileInfo(file).Length
            };
            
            Queue.Add(job);
        }
        
        StatusMessage = $"Added {files.Length} file(s) to queue";
    }
    
    [RelayCommand]
    private async Task StartQueueAsync()
    {
        if (Queue.Count == 0)
        {
            StatusMessage = "Queue is empty";
            return;
        }
        
        if (IsProcessing)
        {
            StatusMessage = "Already processing";
            return;
        }
        
        IsProcessing = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            var queuedJobs = Queue.Where(j => j.Status == TranscodeStatus.Queued).ToList();
            
            foreach (var job in queuedJobs)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;
                
                await ProcessJobAsync(job, _cancellationTokenSource.Token);
            }
            
            StatusMessage = "Queue processing complete";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            CurrentJob = null;
        }
    }
    
    [RelayCommand]
    private void StopQueue()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "Stopping queue...";
    }
    
    [RelayCommand]
    private void RemoveJob(TranscodeJob? job)
    {
        if (job != null && job.Status != TranscodeStatus.Processing)
        {
            Queue.Remove(job);
            StatusMessage = "Job removed from queue";
        }
    }
    
    [RelayCommand]
    private void ClearCompleted()
    {
        var completed = Queue.Where(j => j.Status == TranscodeStatus.Completed).ToList();
        foreach (var job in completed)
        {
            Queue.Remove(job);
        }
        StatusMessage = $"Cleared {completed.Count} completed job(s)";
    }
    
    [RelayCommand]
    private void ClearAll()
    {
        var nonProcessing = Queue.Where(j => j.Status != TranscodeStatus.Processing).ToList();
        foreach (var job in nonProcessing)
        {
            Queue.Remove(job);
        }
        StatusMessage = "Queue cleared";
    }
    
    private async Task ProcessJobAsync(TranscodeJob job, CancellationToken cancellationToken)
    {
        CurrentJob = job;
        job.Status = TranscodeStatus.Processing;
        job.StartedDate = DateTime.Now;
        job.Progress = 0;
        
        StatusMessage = $"Transcoding: {System.IO.Path.GetFileName(job.InputPath)}";
        
        try
        {
            var preset = Presets.FirstOrDefault(p => p.Id == job.PresetId);
            if (preset == null)
            {
                job.Status = TranscodeStatus.Failed;
                job.ErrorMessage = "Preset not found";
                return;
            }
            
            var progress = new Progress<TranscodeProgress>(p =>
            {
                job.Progress = p.Percentage;
                StatusMessage = $"Transcoding: {p.Percentage:F1}% - Speed: {p.Speed:F2}x - Remaining: {p.Remaining:hh\\:mm\\:ss}";
            });
            
            var result = await _transcodeEngine.TranscodeAsync(
                job.InputPath,
                job.OutputPath,
                preset,
                progress,
                cancellationToken
            );
            
            job.CompletedDate = DateTime.Now;
            job.Duration = job.CompletedDate - job.StartedDate;
            
            if (result.Success)
            {
                job.Status = TranscodeStatus.Completed;
                job.OutputSize = result.OutputSize;
                job.Progress = 100;
                StatusMessage = $"Completed: {System.IO.Path.GetFileName(job.InputPath)}";
            }
            else
            {
                job.Status = TranscodeStatus.Failed;
                job.ErrorMessage = result.ErrorMessage;
                StatusMessage = $"Failed: {result.ErrorMessage}";
            }
        }
        catch (OperationCanceledException)
        {
            job.Status = TranscodeStatus.Cancelled;
            StatusMessage = "Job cancelled";
        }
        catch (Exception ex)
        {
            job.Status = TranscodeStatus.Failed;
            job.ErrorMessage = ex.Message;
            StatusMessage = $"Error: {ex.Message}";
        }
    }
    
    private string GenerateOutputPath(string inputPath, TranscodePreset preset)
    {
        var directory = System.IO.Path.GetDirectoryName(inputPath) ?? "";
        var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(inputPath);
        var extension = preset.Container;
        
        return System.IO.Path.Combine(directory, $"{fileNameWithoutExt}_transcoded.{extension}");
    }
}
