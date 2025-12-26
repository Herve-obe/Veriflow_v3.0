using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    [NotifyCanExecuteChangedFor(nameof(StartOffloadCommand))]
    private string _verifyTargetA = string.Empty;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartOffloadCommand))]
    private string _verifyTargetB = string.Empty;
    
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
    private string _logText = "Ready.\n";
    
    [ObservableProperty]
    private ObservableCollection<CopyHistoryEntry> _history = new();
    
    [ObservableProperty]
    private bool _isHistoryVisible;
    
    // File Progress Collections for DataGrid
    public ObservableCollection<FileProgressItem> OffloadFileProgress { get; } = new();
    public ObservableCollection<FileProgressItem> VerifyFileProgress { get; } = new();
    
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
    private async Task SelectVerifyTargetAAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Verify Target #1");
        if (!string.IsNullOrEmpty(folder))
        {
            VerifyTargetA = folder;
            AppendLog($"Verify Target A: {folder}");
        }
    }
    
    [RelayCommand]
    private async Task SelectVerifyTargetBAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Verify Target #2");
        if (!string.IsNullOrEmpty(folder))
        {
            VerifyTargetB = folder;
            AppendLog($"Verify Target B: {folder}");
        }
    }
    
    [RelayCommand]
    private void ClearSource()
    {
        SourceFolder = string.Empty;
        AppendLog("Source cleared");
    }
    
    [RelayCommand]
    private void ClearDestinationA()
    {
        DestinationA = string.Empty;
        AppendLog("Destination A cleared");
    }
    
    [RelayCommand]
    private void ClearDestinationB()
    {
        DestinationB = string.Empty;
        AppendLog("Destination B cleared");
    }
    
    [RelayCommand]
    private void ClearVerifyTargetA()
    {
        VerifyTargetA = string.Empty;
        AppendLog("Verify target A cleared");
    }
    
    [RelayCommand]
    private void ClearVerifyTargetB()
    {
        VerifyTargetB = string.Empty;
        AppendLog("Verify target B cleared");
    }
    
    [RelayCommand]
    private void ResetAll()
    {
        // Reset both modes completely
        ResetOffloadMode();
        ResetVerifyMode();
    }
    
    [RelayCommand]
    private void ResetOffloadMode()
    {
        SourceFolder = string.Empty;
        DestinationA = string.Empty;
        DestinationB = string.Empty;
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
        
        // Clear Offload DataGrid
        OffloadFileProgress.Clear();
        
        AppendLog("Offload mode reset.");
    }
    
    [RelayCommand]
    private void ResetVerifyMode()
    {
        VerifyTargetA = string.Empty;
        VerifyTargetB = string.Empty;
        
        // Clear Verify DataGrid
        VerifyFileProgress.Clear();
        
        AppendLog("Verify mode reset.");
    }
    
    [RelayCommand(CanExecute = nameof(CanStartOffload))]
    private async Task StartOffloadAsync()
    {
        AppendLog("=== START BUTTON CLICKED ===");
        
        // Validate: Source and DestinationA are required, DestinationB is optional
        if (string.IsNullOrEmpty(SourceFolder) || string.IsNullOrEmpty(DestinationA))
        {
            StatusMessage = "Please select Source and Destination A";
            return;
        }
        
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        // Show progress popup window
        Views.OffloadProgressWindow? progressWindow = null;
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            
            if (mainWindow != null)
            {
                progressWindow = new Views.OffloadProgressWindow
                {
                    DataContext = this
                };
                progressWindow.Show(mainWindow);
            }
        });
        
        try
        {
            if (IsOffloadMode)
            {
                AppendLog("\n=== Starting OFFLOAD ===");
                StatusMessage = "Offloading...";
                
                // STEP 1: Clear previous progress
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    OffloadFileProgress.Clear();
                });
                
                // STEP 2: Scan source folder and populate DataGrid
                AppendLog($"Scanning source folder: {SourceFolder}");
                var files = Directory.GetFiles(SourceFolder, "*", SearchOption.AllDirectories);
                AppendLog($"Found {files.Length} files to copy");
                
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    foreach (var file in files)
                    {
                        var relativePath = Path.GetRelativePath(SourceFolder, file);
                        OffloadFileProgress.Add(new FileProgressItem
                        {
                            FileName = relativePath,
                            Hash = "Waiting...",
                            Status = 0 // Pending
                        });
                    }
                });
                
                // STEP 3: Create progress reporter that updates DataGrid
                var progressReporter = new Progress<OffloadProgress>(p =>
                {
                    Progress = p.PercentComplete;
                    CurrentFile = p.CurrentFile;
                    StatusMessage = p.Status;
                    
                    if (p.EstimatedTimeRemaining > TimeSpan.Zero)
                    {
                        EstimatedTimeRemaining = $"ETA: {p.EstimatedTimeRemaining:mm\\:ss}";
                    }
                    
                    if (p.TransferSpeed > 0)
                    {
                        TransferSpeed = $"{FormatBytes((long)p.TransferSpeed)}/s";
                    }
                    
                    // Update DataGrid item status when file completes
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        var item = OffloadFileProgress.FirstOrDefault(x => x.FileName == p.CurrentFile);
                        if (item != null && p.FilesProcessed > 0)
                        {
                            item.Hash = "Calculated"; // Will be updated with actual hash later
                            item.Status = 1; // Success (we'll assume success unless error occurs)
                        }
                    });
                });
                
                var result = await _offloadService.OffloadAsync(
                    SourceFolder,
                    DestinationA,
                    DestinationB,
                    progressReporter,
                    _cancellationTokenSource.Token);
                
                AppendLog($">>> OffloadAsync returned, Success={result.Success}");
                
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
                
                int totalFilesVerified = 0;
                int totalMismatches = 0;
                var allMismatchedFiles = new List<string>();
                
                // Verify Target A if provided
                if (!string.IsNullOrEmpty(VerifyTargetA))
                {
                    AppendLog($"\nVerifying Target A: {VerifyTargetA}");
                    
                    var progressReporter = new Progress<OffloadProgress>(p =>
                    {
                        Progress = p.PercentComplete;
                        CurrentFile = p.CurrentFile;
                        StatusMessage = $"Verifying A: {p.Status}";
                    });
                    
                    var resultA = await _offloadService.VerifyAsync(
                        VerifyTargetA,
                        progressReporter,
                        _cancellationTokenSource.Token);
                    
                    totalFilesVerified += resultA.FilesVerified;
                    totalMismatches += resultA.MismatchCount;
                    allMismatchedFiles.AddRange(resultA.MismatchedFiles.Select(f => $"[A] {f}"));
                    
                    AppendLog($"Target A: {resultA.FilesVerified} files verified, {resultA.MismatchCount} mismatches");
                }
                
                // Verify Target B if provided
                if (!string.IsNullOrEmpty(VerifyTargetB))
                {
                    AppendLog($"\nVerifying Target B: {VerifyTargetB}");
                    
                    var progressReporter = new Progress<OffloadProgress>(p =>
                    {
                        Progress = p.PercentComplete;
                        CurrentFile = p.CurrentFile;
                        StatusMessage = $"Verifying B: {p.Status}";
                    });
                    
                    var resultB = await _offloadService.VerifyAsync(
                        VerifyTargetB,
                        progressReporter,
                        _cancellationTokenSource.Token);
                    
                    totalFilesVerified += resultB.FilesVerified;
                    totalMismatches += resultB.MismatchCount;
                    allMismatchedFiles.AddRange(resultB.MismatchedFiles.Select(f => $"[B] {f}"));
                    
                    AppendLog($"Target B: {resultB.FilesVerified} files verified, {resultB.MismatchCount} mismatches");
                }
                
                // Show combined results
                if (totalMismatches == 0 && totalFilesVerified > 0)
                {
                    AppendLog($"\n✓ Verification complete!");
                    AppendLog($"  Total files verified: {totalFilesVerified}");
                    AppendLog($"  All hashes match!");
                    StatusMessage = "Verification successful!";
                    
                    var details = $"Total files verified: {totalFilesVerified}\n" +
                                 $"All checksums match!";
                    await ShowCompletionDialogAsync("Verification Complete", "All files have been successfully verified.", details, true);
                }
                else if (totalFilesVerified == 0)
                {
                    AppendLog($"\n✗ No files verified!");
                    StatusMessage = "No files found";
                    
                    await ShowCompletionDialogAsync("Verification Failed", "No files were found to verify.", "Please check that the folders contain MHL files.", false);
                }
                else
                {
                    AppendLog($"\n✗ Verification failed!");
                    AppendLog($"  Total files verified: {totalFilesVerified}");
                    AppendLog($"  Mismatches: {totalMismatches}");
                    foreach (var mismatch in allMismatchedFiles)
                    {
                        AppendLog($"    - {mismatch}");
                    }
                    StatusMessage = "Verification failed";
                    
                    var details = $"Total files verified: {totalFilesVerified}\n" +
                                 $"Mismatches: {totalMismatches}\n\n" +
                                 string.Join("\n", allMismatchedFiles);
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
            
            // Close progress window
            if (progressWindow != null)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    progressWindow.Close();
                });
            }
            
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
            // Offload mode: Source and Destination A required
            return !string.IsNullOrEmpty(SourceFolder) && !string.IsNullOrEmpty(DestinationA);
        }
        else
        {
            // Verify mode: At least ONE target required
            return !string.IsNullOrEmpty(VerifyTargetA) || !string.IsNullOrEmpty(VerifyTargetB);
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

/// <summary>
/// Model for file progress items in DataGrid
/// </summary>
public partial class FileProgressItem : ObservableObject
{
    [ObservableProperty]
    private string _fileName = string.Empty;
    
    [ObservableProperty]
    private string _hash = string.Empty;
    
    [ObservableProperty]
    private int _status = 0; // 0 = Pending, 1 = Success, 2 = Error
    
    /// <summary>
    /// Returns icon geometry based on status
    /// </summary>
    public string StatusIconData
    {
        get
        {
            return Status switch
            {
                1 => "M9,20.42L2.79,14.21L5.62,11.38L9,14.77L18.88,4.88L21.71,7.71L9,20.42Z", // Check icon
                2 => "M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z", // X icon
                _ => "" // Empty for pending
            };
        }
    }
    
    /// <summary>
    /// Returns color based on status
    /// </summary>
    public string StatusColor
    {
        get
        {
            return Status switch
            {
                1 => "#00FF00", // Lime Green for success
                2 => "#FF0000", // Red for error
                _ => "Transparent" // Transparent for pending
            };
        }
    }
}
