using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for OFFLOAD page (F1)
/// Secure dual-destination file copying with MHL verification
/// </summary>
public partial class OffloadViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IOffloadService _offloadService;
    private CancellationTokenSource? _cancellationTokenSource;
    
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
    
    [ObservableProperty]
    private string _logText = "Ready to offload...\n";
    
    [ObservableProperty]
    private string _sourceFolder = string.Empty;
    
    [ObservableProperty]
    private string _destinationA = string.Empty;
    
    [ObservableProperty]
    private string _destinationB = string.Empty;
    
    public OffloadViewModel(IDialogService dialogService, IOffloadService offloadService)
    {
        _dialogService = dialogService;
        _offloadService = offloadService;
        StatusMessage = "Ready to offload";
    }
    
    [RelayCommand]
    private async Task SelectSourceFolderAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Source Folder");
        if (!string.IsNullOrEmpty(folder))
        {
            SourceFolder = folder;
            AppendLog($"Source: {folder}");
        }
    }
    
    [RelayCommand]
    private async Task SelectDestinationAAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Destination A");
        if (!string.IsNullOrEmpty(folder))
        {
            DestinationA = folder;
            AppendLog($"Destination A: {folder}");
        }
    }
    
    [RelayCommand]
    private async Task SelectDestinationBAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Destination B");
        if (!string.IsNullOrEmpty(folder))
        {
            DestinationB = folder;
            AppendLog($"Destination B: {folder}");
        }
    }
    
    [RelayCommand]
    private async Task StartOffloadAsync()
    {
        if (string.IsNullOrEmpty(SourceFolder) || string.IsNullOrEmpty(DestinationA) || string.IsNullOrEmpty(DestinationB))
        {
            StatusMessage = "Please select all folders";
            return;
        }
        
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            if (IsOffloadMode)
            {
                AppendLog("\n=== Starting OFFLOAD ===");
                StatusMessage = "Offloading...";
                
                var progressReporter = new Progress<OffloadProgress>(p =>
                {
                    Progress = p.PercentComplete;
                    CurrentFile = p.CurrentFile;
                    StatusMessage = p.Status;
                });
                
                var result = await _offloadService.OffloadAsync(
                    SourceFolder,
                    DestinationA,
                    DestinationB,
                    progressReporter,
                    _cancellationTokenSource.Token);
                
                if (result.Success)
                {
                    AppendLog($"\n✓ Offload complete!");
                    AppendLog($"  Files: {result.FilesProcessed}");
                    AppendLog($"  Size: {FormatBytes(result.BytesCopied)}");
                    AppendLog($"  Time: {result.Duration:mm\\:ss}");
                    AppendLog($"  MHL A: {result.MhlPathA}");
                    AppendLog($"  MHL B: {result.MhlPathB}");
                    StatusMessage = "Offload complete!";
                }
                else
                {
                    AppendLog($"\n✗ Offload failed: {result.ErrorMessage}");
                    StatusMessage = "Offload failed";
                }
            }
            else
            {
                AppendLog("\n=== Starting VERIFY ===");
                StatusMessage = "Verifying...";
                
                var progressReporter = new Progress<OffloadProgress>(p =>
                {
                    Progress = p.PercentComplete;
                    CurrentFile = p.CurrentFile;
                    StatusMessage = p.Status;
                });
                
                var result = await _offloadService.VerifyAsync(
                    SourceFolder,
                    DestinationA,
                    DestinationB,
                    progressReporter,
                    _cancellationTokenSource.Token);
                
                if (result.Success)
                {
                    AppendLog($"\n✓ Verification complete!");
                    AppendLog($"  Files verified: {result.FilesVerified}");
                    AppendLog($"  All hashes match!");
                    StatusMessage = "Verification successful!";
                }
                else
                {
                    AppendLog($"\n✗ Verification failed!");
                    AppendLog($"  Files verified: {result.FilesVerified}");
                    AppendLog($"  Mismatches: {result.MismatchCount}");
                    foreach (var mismatch in result.MismatchedFiles)
                    {
                        AppendLog($"    - {mismatch}");
                    }
                    StatusMessage = "Verification failed";
                }
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog("\n✗ Operation cancelled");
            StatusMessage = "Cancelled";
        }
        catch (Exception ex)
        {
            AppendLog($"\n✗ Error: {ex.Message}");
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsBusy = false;
            Progress = 0;
            CurrentFile = string.Empty;
        }
    }
    
    [RelayCommand]
    private void CancelOffload()
    {
        _cancellationTokenSource?.Cancel();
    }
    
    [RelayCommand]
    private void ToggleMode()
    {
        IsOffloadMode = !IsOffloadMode;
        StatusMessage = IsOffloadMode ? "OFFLOAD mode" : "VERIFY mode";
        AppendLog($"\nMode: {(IsOffloadMode ? "OFFLOAD" : "VERIFY")}");
    }
    
    public void AppendLog(string message)
    {
        LogText += message + "\n";
    }
    
    [RelayCommand]
    private void ClearLog()
    {
        LogText = "Log cleared.\n";
    }
    
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
