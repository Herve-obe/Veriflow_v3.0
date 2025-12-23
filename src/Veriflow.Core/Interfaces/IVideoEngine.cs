using System;
using System.Threading;
using System.Threading.Tasks;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Video player engine interface for professional video playback
/// </summary>
public interface IVideoEngine
{
    /// <summary>
    /// Load a video file
    /// </summary>
    Task LoadVideoAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unload current video
    /// </summary>
    void UnloadVideo();
    
    /// <summary>
    /// Start playback
    /// </summary>
    void Play();
    
    /// <summary>
    /// Pause playback
    /// </summary>
    void Pause();
    
    /// <summary>
    /// Stop playback
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Seek to position in milliseconds
    /// </summary>
    void Seek(long positionMs);
    
    /// <summary>
    /// Step forward one frame
    /// </summary>
    void StepForward();
    
    /// <summary>
    /// Step backward one frame
    /// </summary>
    void StepBackward();
    
    /// <summary>
    /// Set playback rate (0.25x to 4.0x)
    /// </summary>
    void SetPlaybackRate(float rate);
    
    /// <summary>
    /// Set volume (0.0 to 1.0)
    /// </summary>
    void SetVolume(float volume);
    
    /// <summary>
    /// Get current playback position in milliseconds
    /// </summary>
    long GetPosition();
    
    /// <summary>
    /// Get total duration in milliseconds
    /// </summary>
    long GetDuration();
    
    /// <summary>
    /// Get current frame number
    /// </summary>
    long GetFrameNumber();
    
    /// <summary>
    /// Get frame rate (fps)
    /// </summary>
    double GetFrameRate();
    
    /// <summary>
    /// Get video width
    /// </summary>
    int GetWidth();
    
    /// <summary>
    /// Get video height
    /// </summary>
    int GetHeight();
    
    /// <summary>
    /// Is currently playing
    /// </summary>
    bool IsPlaying { get; }
    
    /// <summary>
    /// Is video loaded
    /// </summary>
    bool IsLoaded { get; }
    
    /// <summary>
    /// Current playback rate
    /// </summary>
    float PlaybackRate { get; }
    
    /// <summary>
    /// Event raised when playback state changes
    /// </summary>
    event EventHandler<bool>? PlaybackStateChanged;
    
    /// <summary>
    /// Event raised when position changes
    /// </summary>
    event EventHandler<long>? PositionChanged;
    
    /// <summary>
    /// Dispose resources
    /// </summary>
    void Dispose();
}
