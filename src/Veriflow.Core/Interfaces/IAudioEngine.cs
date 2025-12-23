using Veriflow.Core.Models;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Interface for audio engine operations
/// </summary>
public interface IAudioEngine
{
    /// <summary>
    /// Initialize the audio engine
    /// </summary>
    void Initialize(int sampleRate = 48000, int channels = 2);
    
    /// <summary>
    /// Load an audio track
    /// </summary>
    Task<AudioTrack> LoadTrackAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add a track to the playback mixer
    /// </summary>
    void AddTrack(AudioTrack track);
    
    /// <summary>
    /// Remove a track from the playback mixer
    /// </summary>
    void RemoveTrack(Guid trackId);
    
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
    /// Seek to a specific sample position
    /// </summary>
    void Seek(long samplePosition);
    
    /// <summary>
    /// Get current playback position in samples
    /// </summary>
    long GetPosition();
    
    /// <summary>
    /// Get peak level for a specific track
    /// </summary>
    float GetPeakLevel(Guid trackId);
    
    /// <summary>
    /// Get RMS level for a specific track
    /// </summary>
    float GetRMSLevel(Guid trackId);
    
    /// <summary>
    /// Dispose resources
    /// </summary>
    void Dispose();
}
