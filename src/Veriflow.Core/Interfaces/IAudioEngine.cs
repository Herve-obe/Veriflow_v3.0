using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Audio engine interface for multi-track playback
/// </summary>
public interface IAudioEngine
{
    /// <summary>
    /// Load an audio file into a track
    /// </summary>
    Task LoadTrackAsync(int trackIndex, string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unload a track
    /// </summary>
    void UnloadTrack(int trackIndex);
    
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
    /// Seek to position in seconds
    /// </summary>
    void Seek(double positionSeconds);
    
    /// <summary>
    /// Set track volume (0.0 to 1.0)
    /// </summary>
    void SetTrackVolume(int trackIndex, float volume);
    
    /// <summary>
    /// Set track pan (-1.0 to 1.0)
    /// </summary>
    void SetTrackPan(int trackIndex, float pan);
    
    /// <summary>
    /// Mute/unmute track
    /// </summary>
    void SetTrackMute(int trackIndex, bool muted);
    
    /// <summary>
    /// Solo track
    /// </summary>
    void SetTrackSolo(int trackIndex, bool solo);
    
    /// <summary>
    /// Get current playback position in seconds
    /// </summary>
    double GetPosition();
    
    /// <summary>
    /// Get total duration in seconds
    /// </summary>
    double GetDuration();
    
    /// <summary>
    /// Get current peak levels for a track (L/R)
    /// </summary>
    (float left, float right) GetTrackPeaks(int trackIndex);
    
    /// <summary>
    /// Get master output peaks (L/R)
    /// </summary>
    (float left, float right) GetMasterPeaks();
    
    /// <summary>
    /// Is currently playing
    /// </summary>
    bool IsPlaying { get; }
    
    /// <summary>
    /// Maximum number of tracks
    /// </summary>
    int MaxTracks { get; }
    
    /// <summary>
    /// Sample rate
    /// </summary>
    int SampleRate { get; }
    
    /// <summary>
    /// Dispose resources
    /// </summary>
    void Dispose();
}
