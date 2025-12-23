using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;
using Veriflow.Core.Services;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for SYNC page (F4)
/// Audio/video synchronization with waveform correlation
/// </summary>
public partial class SyncViewModel : ViewModelBase
{
    private readonly ISyncEngine _syncEngine;
    private readonly IMediaService _mediaService;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _cancellationTokenSource;
    
    [ObservableProperty]
    private ObservableCollection<SyncPoolItem> _videoPool = new();
    
    [ObservableProperty]
    private ObservableCollection<SyncPoolItem> _audioPool = new();
    
    [ObservableProperty]
    private ObservableCollection<SyncPair> _syncPairs = new();
    
    [ObservableProperty]
    private SyncPoolItem? _selectedVideo;
    
    [ObservableProperty]
    private SyncPoolItem? _selectedAudio;
    
    [ObservableProperty]
    private SyncPair? _selectedPair;
    
    [ObservableProperty]
    private double _syncProgress;
    
    [ObservableProperty]
    private string _syncStatus = "Ready";
    
    public SyncViewModel(ISyncEngine syncEngine, IMediaService mediaService, IDialogService dialogService)
    {
        _syncEngine = syncEngine;
        _mediaService = mediaService;
        _dialogService = dialogService;
        
        StatusMessage = "Add video and audio files to sync";
    }
    
    [RelayCommand]
    private async Task AddVideoFilesAsync()
    {
        var files = await _dialogService.ShowFilePickerAsync("Select Video Files", true);
        if (files == null || files.Length == 0) return;
        
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            foreach (var file in files)
            {
                StatusMessage = $"Loading {System.IO.Path.GetFileName(file)}...";
                
                var mediaInfo = await _mediaService.GetMediaInfoAsync(file, _cancellationTokenSource.Token);
                
                var poolItem = new SyncPoolItem
                {
                    FilePath = file,
                    FileName = System.IO.Path.GetFileName(file),
                    Type = MediaType.Video,
                    Duration = mediaInfo.Duration,
                    FrameRate = mediaInfo.FrameRate ?? 25.0,
                    Width = mediaInfo.Width,
                    Height = mediaInfo.Height,
                    Timecode = mediaInfo.Timecode
                };
                
                VideoPool.Add(poolItem);
            }
            
            StatusMessage = $"Added {files.Length} video file(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task AddAudioFilesAsync()
    {
        var files = await _dialogService.ShowFilePickerAsync("Select Audio Files", true);
        if (files == null || files.Length == 0) return;
        
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            foreach (var file in files)
            {
                StatusMessage = $"Loading {System.IO.Path.GetFileName(file)}...";
                
                var mediaInfo = await _mediaService.GetMediaInfoAsync(file, _cancellationTokenSource.Token);
                
                var poolItem = new SyncPoolItem
                {
                    FilePath = file,
                    FileName = System.IO.Path.GetFileName(file),
                    Type = MediaType.Audio,
                    Duration = mediaInfo.Duration,
                    SampleRate = mediaInfo.SampleRate,
                    Channels = mediaInfo.Channels,
                    Timecode = mediaInfo.Timecode
                };
                
                AudioPool.Add(poolItem);
            }
            
            StatusMessage = $"Added {files.Length} audio file(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task AutoSyncAsync()
    {
        await SynchronizeSelectedAsync();
    }
    
    [RelayCommand]
    private async Task SyncByWaveformAsync()
    {
        await SynchronizeSelectedAsync();
    }
    
    [RelayCommand]
    private async Task SynchronizeSelectedAsync()
    {
        if (SelectedVideo == null || SelectedAudio == null)
        {
            StatusMessage = "Select both video and audio files";
            return;
        }
        
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        SyncProgress = 0;
        
        try
        {
            SyncStatus = "Analyzing waveforms...";
            SyncProgress = 25;
            
            var result = await _syncEngine.SynchronizeAsync(
                SelectedVideo.FilePath,
                SelectedAudio.FilePath,
                _cancellationTokenSource.Token
            );
            
            SyncProgress = 75;
            
            if (result.Success)
            {
                var pair = new SyncPair
                {
                    VideoItem = SelectedVideo,
                    AudioItem = SelectedAudio,
                    Offset = result.Offset,
                    Confidence = result.Confidence,
                    IsVerified = result.Confidence > 80
                };
                
                SyncPairs.Add(pair);
                
                SyncProgress = 100;
                SyncStatus = $"Synchronized! Offset: {result.Offset.TotalSeconds:F3}s, Confidence: {result.Confidence:F1}%";
                StatusMessage = $"Sync complete: {result.FrameOffset} frames @ {result.FrameRate:F2}fps";
            }
            else
            {
                SyncStatus = $"Sync failed: {result.ErrorMessage}";
                StatusMessage = "Synchronization failed";
            }
        }
        catch (Exception ex)
        {
            SyncStatus = $"Error: {ex.Message}";
            StatusMessage = "Synchronization error";
        }
        finally
        {
            IsBusy = false;
            SyncProgress = 0;
        }
    }
    
    [RelayCommand]
    private async Task AutoSyncAllAsync()
    {
        if (VideoPool.Count == 0 || AudioPool.Count == 0)
        {
            StatusMessage = "Add video and audio files first";
            return;
        }
        
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            // REDUNDANCY PROTECTION: Filter out already-synced videos (v1 feature)
            var unsyncedVideos = VideoPool
                .Where(v => !SyncPairs.Any(p => p.VideoItem?.FilePath == v.FilePath))
                .ToList();
            
            if (unsyncedVideos.Count == 0)
            {
                StatusMessage = "All videos are already synchronized";
                SyncStatus = "No unmatched videos to sync";
                IsBusy = false;
                return;
            }
            
            var totalPairs = unsyncedVideos.Count * AudioPool.Count;
            var currentPair = 0;
            var syncedPairs = new System.Collections.Concurrent.ConcurrentBag<SyncPair>();
            
            // Create pairs only for unsynced videos
            var pairs = unsyncedVideos.SelectMany(v => AudioPool.Select(a => (Video: v, Audio: a))).ToList();
            
            StatusMessage = $"Syncing {unsyncedVideos.Count} unmatched videos (skipping {VideoPool.Count - unsyncedVideos.Count} already synced)";
            SyncStatus = $"Processing {unsyncedVideos.Count} unmatched videos...";
            
            // Parallel sync processing with max concurrency
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4,
                CancellationToken = _cancellationTokenSource.Token
            };
            
            await Parallel.ForEachAsync(pairs, options, async (pair, ct) =>
            {
                var pairIndex = Interlocked.Increment(ref currentPair);
                SyncProgress = (pairIndex * 100.0) / totalPairs;
                SyncStatus = $"Syncing {pair.Video.FileName} with {pair.Audio.FileName}...";
                
                var result = await _syncEngine.SynchronizeAsync(
                    pair.Video.FilePath,
                    pair.Audio.FilePath,
                    ct
                );
                
                if (result.Success && result.Confidence > 70)
                {
                    var syncPair = new SyncPair
                    {
                        VideoItem = pair.Video,
                        AudioItem = pair.Audio,
                        Offset = result.Offset,
                        Confidence = result.Confidence,
                        IsVerified = result.Confidence > 80
                    };
                    
                    syncedPairs.Add(syncPair);
                }
            });
            
            // Add results to UI collection on main thread
            foreach (var pair in syncedPairs)
            {
                SyncPairs.Add(pair);
            }
            
            SyncStatus = $"Auto-sync complete: {SyncPairs.Count} pairs found";
            StatusMessage = $"Found {SyncPairs.Count} synchronized pairs";
        }
        catch (Exception ex)
        {
            SyncStatus = $"Error: {ex.Message}";
            StatusMessage = "Auto-sync error";
            CrashLogger.LogException(ex, "AutoSyncAllAsync");
        }
        finally
        {
            IsBusy = false;
            SyncProgress = 0;
        }
    }
    
    [RelayCommand]
    private void RemoveVideo(SyncPoolItem? item)
    {
        if (item != null)
            VideoPool.Remove(item);
    }
    
    [RelayCommand]
    private void RemoveAudio(SyncPoolItem? item)
    {
        if (item != null)
            AudioPool.Remove(item);
    }
    
    [RelayCommand]
    private void RemovePair(SyncPair? pair)
    {
        if (pair != null)
            SyncPairs.Remove(pair);
    }
    
    [RelayCommand]
    private void ClearAll()
    {
        VideoPool.Clear();
        AudioPool.Clear();
        SyncPairs.Clear();
        SelectedVideo = null;
        SelectedAudio = null;
        SelectedPair = null;
        StatusMessage = "Pools cleared";
    }
}
