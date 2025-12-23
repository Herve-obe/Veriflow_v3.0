using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for PLAYER page (F3)
/// Multi-track audio playback with waveforms and VU meters
/// </summary>
public partial class PlayerViewModel : ViewModelBase, IDisposable
{
    private readonly IAudioEngine _audioEngine;
    private readonly IDialogService _dialogService;
    private readonly IMediaService _mediaService;
    private readonly DispatcherTimer _updateTimer;
    private CancellationTokenSource? _cancellationTokenSource;
    
    [ObservableProperty]
    private ObservableCollection<AudioTrackViewModel> _tracks = new();
    
    [ObservableProperty]
    private double _position;
    
    [ObservableProperty]
    private double _duration;
    
    [ObservableProperty]
    private string _formattedPosition = "00:00:00.000";
    
    [ObservableProperty]
    private string _formattedDuration = "00:00:00.000";
    
    [ObservableProperty]
    private bool _isPlaying;
    
    [ObservableProperty]
    private float _masterPeakLeft;
    
    [ObservableProperty]
    private float _masterPeakRight;
    
    [ObservableProperty]
    private float _masterVolume = 1.0f;
    
    [ObservableProperty]
    private float _playbackRate = 1.0f;
    
    [ObservableProperty]
    private ObservableCollection<LoggedClip> _loggedClips = new();
    
    [ObservableProperty]
    private string _currentFilePath = string.Empty;
    
    public PlayerViewModel(IAudioEngine audioEngine, IDialogService dialogService, IMediaService mediaService)
    {
        _audioEngine = audioEngine;
        _dialogService = dialogService;
        _mediaService = mediaService;
        
        StatusMessage = "Ready to play audio";
        
        // Initialize tracks
        for (int i = 0; i < _audioEngine.MaxTracks; i++)
        {
            Tracks.Add(new AudioTrackViewModel(i, _audioEngine));
        }
        
        // Setup update timer for position and VU meters
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20Hz update rate
        };
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();
    }
    
    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (_audioEngine.IsPlaying)
        {
            Position = _audioEngine.GetPosition();
            FormattedPosition = FormatTime(Position);
            
            // Update VU meters
            var masterPeaks = _audioEngine.GetMasterPeaks();
            MasterPeakLeft = masterPeaks.left;
            MasterPeakRight = masterPeaks.right;
            
            // Update track peaks
            foreach (var track in Tracks)
            {
                track.UpdatePeaks();
            }
            
            // Check if playback finished
            if (Position >= Duration)
            {
                Stop();
            }
        }
    }
    
    [RelayCommand]
    private async Task LoadTrackAsync(int trackIndex)
    {
        var files = await _dialogService.ShowFilePickerAsync("Select Audio File", false);
        if (files == null || files.Length == 0) return;
        
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            StatusMessage = $"Loading {files[0]}...";
            
            await _audioEngine.LoadTrackAsync(trackIndex, files[0], _cancellationTokenSource.Token);
            
            var track = Tracks[trackIndex];
            track.FileName = System.IO.Path.GetFileName(files[0]);
            track.IsLoaded = true;
            
            // Store current file path for logging
            CurrentFilePath = files[0];
            
            // Update duration
            Duration = _audioEngine.GetDuration();
            FormattedDuration = FormatTime(Duration);
            
            StatusMessage = $"Loaded: {track.FileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private void UnloadTrack(int trackIndex)
    {
        _audioEngine.UnloadTrack(trackIndex);
        
        var track = Tracks[trackIndex];
        track.FileName = string.Empty;
        track.IsLoaded = false;
        track.Volume = 1.0f;
        track.Pan = 0f;
        track.IsMuted = false;
        track.IsSolo = false;
        
        Duration = _audioEngine.GetDuration();
        FormattedDuration = FormatTime(Duration);
        
        StatusMessage = $"Track {trackIndex + 1} unloaded";
    }
    
    [RelayCommand]
    private void Play()
    {
        _audioEngine.Play();
        IsPlaying = true;
        StatusMessage = "Playing";
    }
    
    [RelayCommand]
    private void Pause()
    {
        _audioEngine.Pause();
        IsPlaying = false;
        StatusMessage = "Paused";
    }
    
    [RelayCommand]
    private void Stop()
    {
        _audioEngine.Stop();
        IsPlaying = false;
        Position = 0;
        FormattedPosition = "00:00:00.000";
        StatusMessage = "Stopped";
    }
    
    [RelayCommand]
    private void Seek(double position)
    {
        _audioEngine.Seek(position);
        Position = position;
        FormattedPosition = FormatTime(position);
    }
    
    [RelayCommand]
    private void PlayPauseToggle()
    {
        if (IsPlaying)
            Pause();
        else
            Play();
    }
    
    [RelayCommand]
    private void ShuttleBackward()
    {
        // J key - Shuttle backward (cycle through speeds: -2x, -4x, -8x, pause)
        if (PlaybackRate > 0)
            PlaybackRate = -2.0f;
        else if (PlaybackRate > -8.0f)
            PlaybackRate *= 2;
        else
            PlaybackRate = 0;
        
        if (PlaybackRate == 0)
            Pause();
        else
        {
            // Note: OpenAL doesn't support negative playback rates
            // This is a placeholder - actual implementation would need custom audio processing
            StatusMessage = $"Shuttle: {PlaybackRate:F1}x (not yet implemented)";
        }
    }
    
    [RelayCommand]
    private void ShuttleForward()
    {
        // L key - Shuttle forward (cycle through speeds: 2x, 4x, 8x, 1x)
        if (PlaybackRate < 0)
            PlaybackRate = 2.0f;
        else if (PlaybackRate < 8.0f)
            PlaybackRate *= 2;
        else
            PlaybackRate = 1.0f;
        
        Play();
        StatusMessage = $"Shuttle: {PlaybackRate:F1}x (not yet implemented)";
    }
    
    [RelayCommand]
    private void StepForward()
    {
        // Right arrow - Step forward (not implemented for audio, would need frame-based seeking)
        StatusMessage = "Step forward (not implemented for audio)";
    }
    
    [RelayCommand]
    private void StepBackward()
    {
        // Left arrow - Step backward (not implemented for audio, would need frame-based seeking)
        StatusMessage = "Step backward (not implemented for audio)";
    }
    
    private static string FormatTime(double seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);
        return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
    }
    
    [RelayCommand]
    private void LogCurrentClip()
    {
        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            StatusMessage = "No file loaded to log";
            return;
        }
        
        var clip = new LoggedClip
        {
            FilePath = CurrentFilePath,
            FileName = System.IO.Path.GetFileName(CurrentFilePath),
            TimecodeIn = FormattedPosition,
            TimecodeOut = FormattedDuration,
            Duration = FormatTime(Duration),
            LoggedAt = DateTime.Now,
            IsGood = true
        };
        
        LoggedClips.Add(clip);
        StatusMessage = $"Logged clip: {clip.FileName}";
    }
    
    [RelayCommand]
    private void RemoveLoggedClip(LoggedClip? clip)
    {
        if (clip != null && LoggedClips.Contains(clip))
        {
            LoggedClips.Remove(clip);
            StatusMessage = $"Removed clip: {clip.FileName}";
        }
    }
    
    [RelayCommand]
    private void ClearLoggedClips()
    {
        LoggedClips.Clear();
        StatusMessage = "Cleared all logged clips";
    }
    
    public void Dispose()
    {
        _updateTimer?.Stop();
        _cancellationTokenSource?.Cancel();
        _audioEngine?.Dispose();
    }
}

/// <summary>
/// ViewModel for individual audio track
/// </summary>
public partial class AudioTrackViewModel : ObservableObject
{
    private readonly int _trackIndex;
    private readonly IAudioEngine _audioEngine;
    
    [ObservableProperty]
    private string _fileName = string.Empty;
    
    [ObservableProperty]
    private bool _isLoaded;
    
    [ObservableProperty]
    private float _volume = 1.0f;
    
    [ObservableProperty]
    private float _pan = 0f;
    
    [ObservableProperty]
    private bool _isMuted;
    
    [ObservableProperty]
    private bool _isSolo;
    
    [ObservableProperty]
    private float _peakLeft;
    
    [ObservableProperty]
    private float _peakRight;
    
    public int TrackNumber => _trackIndex + 1;
    
    public AudioTrackViewModel(int trackIndex, IAudioEngine audioEngine)
    {
        _trackIndex = trackIndex;
        _audioEngine = audioEngine;
    }
    
    partial void OnVolumeChanged(float value)
    {
        _audioEngine.SetTrackVolume(_trackIndex, value);
    }
    
    partial void OnPanChanged(float value)
    {
        _audioEngine.SetTrackPan(_trackIndex, value);
    }
    
    partial void OnIsMutedChanged(bool value)
    {
        _audioEngine.SetTrackMute(_trackIndex, value);
    }
    
    partial void OnIsSoloChanged(bool value)
    {
        _audioEngine.SetTrackSolo(_trackIndex, value);
    }
    
    public void UpdatePeaks()
    {
        var peaks = _audioEngine.GetTrackPeaks(_trackIndex);
        PeakLeft = peaks.left;
        PeakRight = peaks.right;
    }
}
