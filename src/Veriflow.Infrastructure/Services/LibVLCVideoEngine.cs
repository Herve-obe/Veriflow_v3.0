using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// LibVLC-based video player engine
/// Professional video playback with frame-accurate seeking
/// </summary>
public class LibVLCVideoEngine : IVideoEngine, IDisposable
{
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    private Media? _currentMedia;
    private bool _isPlaying;
    private bool _isLoaded;
    private float _playbackRate = 1.0f;
    private bool _disposed;
    
    public bool IsPlaying => _isPlaying;
    public bool IsLoaded => _isLoaded;
    public float PlaybackRate => _playbackRate;
    
    public event EventHandler<bool>? PlaybackStateChanged;
    public event EventHandler<long>? PositionChanged;
    
    public LibVLCVideoEngine()
    {
        InitializeLibVLC();
    }
    
    private void InitializeLibVLC()
    {
        try
        {
            Core.Initialize();
            
            _libVLC = new LibVLC(enableDebugLogs: false);
            _mediaPlayer = new MediaPlayer(_libVLC);
            
            // Subscribe to events
            _mediaPlayer.Playing += OnPlaying;
            _mediaPlayer.Paused += OnPaused;
            _mediaPlayer.Stopped += OnStopped;
            _mediaPlayer.EndReached += OnEndReached;
            _mediaPlayer.TimeChanged += OnTimeChanged;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize LibVLC: {ex.Message}", ex);
        }
    }
    
    public async Task LoadVideoAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Video file not found: {filePath}");
        
        await Task.Run(() =>
        {
            try
            {
                // Dispose previous media
                _currentMedia?.Dispose();
                
                // Create new media
                _currentMedia = new Media(_libVLC, filePath, FromType.FromPath);
                
                // Parse media to get metadata
                _currentMedia.Parse(MediaParseOptions.ParseNetwork);
                
                // Set media to player
                _mediaPlayer!.Media = _currentMedia;
                
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load video: {ex.Message}", ex);
            }
        }, cancellationToken);
    }
    
    public void UnloadVideo()
    {
        Stop();
        
        _currentMedia?.Dispose();
        _currentMedia = null;
        _mediaPlayer!.Media = null;
        
        _isLoaded = false;
    }
    
    public void Play()
    {
        if (_mediaPlayer == null || !_isLoaded) return;
        
        _mediaPlayer.Play();
        _isPlaying = true;
        PlaybackStateChanged?.Invoke(this, true);
    }
    
    public void Pause()
    {
        if (_mediaPlayer == null || !_isLoaded) return;
        
        _mediaPlayer.Pause();
        _isPlaying = false;
        PlaybackStateChanged?.Invoke(this, false);
    }
    
    public void Stop()
    {
        if (_mediaPlayer == null) return;
        
        _mediaPlayer.Stop();
        _isPlaying = false;
        PlaybackStateChanged?.Invoke(this, false);
    }
    
    public void Seek(long positionMs)
    {
        if (_mediaPlayer == null || !_isLoaded) return;
        
        _mediaPlayer.Time = positionMs;
        PositionChanged?.Invoke(this, positionMs);
    }
    
    public void StepForward()
    {
        if (_mediaPlayer == null || !_isLoaded) return;
        
        _mediaPlayer.NextFrame();
    }
    
    public void StepBackward()
    {
        if (_mediaPlayer == null || !_isLoaded) return;
        
        // LibVLC doesn't have native previous frame, so we seek back
        var frameRate = GetFrameRate();
        if (frameRate > 0)
        {
            var frameDuration = (long)(1000.0 / frameRate);
            var newPosition = Math.Max(0, _mediaPlayer.Time - frameDuration);
            Seek(newPosition);
        }
    }
    
    public void SetPlaybackRate(float rate)
    {
        if (_mediaPlayer == null) return;
        
        _playbackRate = Math.Clamp(rate, 0.25f, 4.0f);
        _mediaPlayer.SetRate(_playbackRate);
    }
    
    public void SetVolume(float volume)
    {
        if (_mediaPlayer == null) return;
        
        var volumePercent = (int)Math.Clamp(volume * 100, 0, 100);
        _mediaPlayer.Volume = volumePercent;
    }
    
    public long GetPosition()
    {
        return _mediaPlayer?.Time ?? 0;
    }
    
    public long GetDuration()
    {
        return _mediaPlayer?.Length ?? 0;
    }
    
    public long GetFrameNumber()
    {
        if (_mediaPlayer == null) return 0;
        
        var frameRate = GetFrameRate();
        if (frameRate > 0)
        {
            return (long)(_mediaPlayer.Time / 1000.0 * frameRate);
        }
        return 0;
    }
    
    public double GetFrameRate()
    {
        if (_mediaPlayer?.Media == null) return 0;
        
        // Try to get frame rate from media tracks
        foreach (var track in _mediaPlayer.Media.Tracks)
        {
            if (track.TrackType == TrackType.Video && track is MediaTrack videoTrack)
            {
                return videoTrack.Data.Video.FrameRate;
            }
        }
        
        return 25.0; // Default fallback
    }
    
    public int GetWidth()
    {
        if (_mediaPlayer?.Media == null) return 0;
        
        foreach (var track in _mediaPlayer.Media.Tracks)
        {
            if (track.TrackType == TrackType.Video && track is MediaTrack videoTrack)
            {
                return (int)videoTrack.Data.Video.Width;
            }
        }
        
        return 0;
    }
    
    public int GetHeight()
    {
        if (_mediaPlayer?.Media == null) return 0;
        
        foreach (var track in _mediaPlayer.Media.Tracks)
        {
            if (track.TrackType == TrackType.Video && track is MediaTrack videoTrack)
            {
                return (int)videoTrack.Data.Video.Height;
            }
        }
        
        return 0;
    }
    
    private void OnPlaying(object? sender, EventArgs e)
    {
        _isPlaying = true;
        PlaybackStateChanged?.Invoke(this, true);
    }
    
    private void OnPaused(object? sender, EventArgs e)
    {
        _isPlaying = false;
        PlaybackStateChanged?.Invoke(this, false);
    }
    
    private void OnStopped(object? sender, EventArgs e)
    {
        _isPlaying = false;
        PlaybackStateChanged?.Invoke(this, false);
    }
    
    private void OnEndReached(object? sender, EventArgs e)
    {
        _isPlaying = false;
        PlaybackStateChanged?.Invoke(this, false);
    }
    
    private void OnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        PositionChanged?.Invoke(this, e.Time);
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        Stop();
        
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Playing -= OnPlaying;
            _mediaPlayer.Paused -= OnPaused;
            _mediaPlayer.Stopped -= OnStopped;
            _mediaPlayer.EndReached -= OnEndReached;
            _mediaPlayer.TimeChanged -= OnTimeChanged;
            
            _mediaPlayer.Dispose();
            _mediaPlayer = null;
        }
        
        _currentMedia?.Dispose();
        _currentMedia = null;
        
        _libVLC?.Dispose();
        _libVLC = null;
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
