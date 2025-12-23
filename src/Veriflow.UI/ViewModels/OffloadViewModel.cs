using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;

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
    [NotifyCanExecuteChangedFor(nameof(StartOffloadCommand))]
    private string _sourceFolder = string.Empty;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartOffloadCommand))]
    private string _destinationA = string.Empty;
    
    [ObservableProperty]
    private string _destinationB = string.Empty;
    
    [ObservableProperty]
    private string _verifyTargetFolder = string.Empty;
    
    [ObservableProperty]
    private bool _isOffloadMode = true; // true = OFFLOAD, false = VERIFY
    
    [ObservableProperty]
    private double _progress;
    
    [ObservableProperty]
    private double _progressA;
    
    [ObservableProperty]
    private double _progressB;
    
    [ObservableProperty]
    private string _currentFile = string.Empty;
    
    [ObservableProperty]
    private string _currentFileA = string.Empty;
    
    [ObservableProperty]
    private string _currentFileB = string.Empty;
    
    [ObservableProperty]
    private string _estimatedTimeRemaining = string.Empty;
    
    [ObservableProperty]
    private string _estimatedTimeRemainingA = string.Empty;
    
    [ObservableProperty]
    private string _estimatedTimeRemainingB = string.Empty;
    
    [ObservableProperty]
    private string _transferSpeed = string.Empty;
    
    [ObservableProperty]
    private string _transferSpeedA = string.Empty;
    
    [ObservableProperty]
    private string _transferSpeedB = string.Empty;
    
    [ObservableProperty]
    private string _logText = "Ready to offload...\n";
    
    [ObservableProperty]
    private ObservableCollection<CopyHistoryEntry> _history = new();
    
    [ObservableProperty]
    private bool _isHistoryVisible;
    
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
    private async Task SelectVerifyTargetAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Folder to Verify");
        if (!string.IsNullOrEmpty(folder))
        {
            VerifyTargetFolder = folder;
            AppendLog($"Verify Target: {folder}");
        }
    }
    
    [RelayCommand]
    private void ResetAll()
    {
        SourceFolder = string.Empty;
        DestinationA = string.Empty;
        DestinationB = string.Empty;
        VerifyTargetFolder = string.Empty;
        Progress = 0;
        ProgressA = 0;
        ProgressB = 0;
        CurrentFile = string.Empty;
        CurrentFileA = string.Empty;
        CurrentFileB = string.Empty;
        EstimatedTimeRemaining = string.Empty;
        EstimatedTimeRemainingA = string.Empty;
        EstimatedTimeRemainingB = string.Empty;
        TransferSpeed = string.Empty;
        TransferSpeedA = string.Empty;
        TransferSpeedB = string.Empty;
        AppendLog("All fields reset.");
    }
    
    [RelayCommand(CanExecute = nameof(CanStartOffload))]
    private async Task StartOffloadAsync()
    {
        // Validate: Source and DestinationA are required, DestinationB is optional
        if (string.IsNullOrEmpty(SourceFolder) || string.IsNullOrEmpty(DestinationA))
        {
            StatusMessage = "Please select Source and Destination A";
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
                    
                    // Update ETA and transfer speed
                    if (p.EstimatedTimeRemaining > TimeSpan.Zero)
                    {
                        EstimatedTimeRemaining = $"ETA: {p.EstimatedTimeRemaining:mm\\:ss}";
                    }
                    
                    if (p.TransferSpeed > 0)
                    {
                        TransferSpeed = $"{FormatBytes((long)p.TransferSpeed)}/s";
                    }
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
                    
                    // Show success dialog
                    var details = $"Files copied: {result.FilesProcessed}\n" +
                                 $"Total size: {FormatBytes(result.BytesCopied)}\n" +
                                 $"Duration: {result.Duration:mm\\:ss}\n" +
                                 $"MHL A: {result.MhlPathA}\n" +
                                 $"MHL B: {result.MhlPathB}";
                    await ShowCompletionDialogAsync("Offload Complete", "All files have been successfully copied and verified.", details, true);
                }
                else
                {
                    AppendLog($"\n✗ Offload failed: {result.ErrorMessage}");
                    StatusMessage = "Offload failed";
                    
                    // Show error dialog
                    await ShowCompletionDialogAsync("Offload Failed", "The offload operation encountered an error.", result.ErrorMessage ?? "Unknown error", false);
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
                    
                    // Show success dialog
                    var details = $"Files verified: {result.FilesVerified}\n" +
                                 $"All checksums match!";
                    await ShowCompletionDialogAsync("Verification Complete", "All files have been successfully verified.", details, true);
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
                    
                    // Show error dialog
                    var details = $"Files verified: {result.FilesVerified}\n" +
                                 $"Mismatches: {result.MismatchCount}\n\n" +
                                 string.Join("\n", result.MismatchedFiles);
                    await ShowCompletionDialogAsync("Verification Failed", "Some files failed verification.", details, false);
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
            
            // Refresh history after offload
            if (IsOffloadMode)
            {
                await LoadHistoryAsync();
            }
        }
    }
    
    [RelayCommand]
    private void CancelOffload()
    {
        _cancellationTokenSource?.Cancel();
    }
    
    private bool CanStartOffload()
    {
        if (IsOffloadMode)
        {
            return !string.IsNullOrEmpty(SourceFolder) && !string.IsNullOrEmpty(DestinationA);
        }
        else
        {
            return !string.IsNullOrEmpty(VerifyTargetFolder);
        }
    }
    
    [RelayCommand]
    private void ToggleMode()
    {
        IsOffloadMode = !IsOffloadMode;
        OnPropertyChanged(nameof(IsOffloadMode));
        StartOffloadCommand.NotifyCanExecuteChanged();
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
    
    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        var history = await _offloadService.GetHistoryAsync();
        History.Clear();
        foreach (var entry in history)
        {
            History.Add(entry);
        }
    }
    
    [RelayCommand]
    private void ToggleHistory()
    {
        IsHistoryVisible = !IsHistoryVisible;
    }
    
    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Clear History",
            "Are you sure you want to clear all copy history?");
        
        if (confirmed)
        {
            await _offloadService.ClearHistoryAsync();
            History.Clear();
            AppendLog("History cleared");
        }
    }
    
    private async Task ShowCompletionDialogAsync(string title, string subtitle, string details, bool isSuccess)
    {
        try
        {
            var dialog = new Views.OffloadCompletionDialog();
            
            if (isSuccess)
            {
                dialog.SetSuccess(title, subtitle, details);
            }
            else
            {
                dialog.SetError(title, subtitle, details);
            }
            
            // Show dialog - note: this requires access to the main window
            // For now, we'll use the dialog service's message box as a fallback
            await _dialogService.ShowMessageBoxAsync(title, $"{subtitle}\n\n{details}");
        }
        catch (Exception ex)
        {
            AppendLog($"Error showing completion dialog: {ex.Message}");
        }
    }
}
