using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Synchronization engine interface for audio/video alignment
/// </summary>
public interface ISyncEngine
{
    /// <summary>
    /// Synchronize audio and video files using waveform correlation
    /// </summary>
    Task<SyncResult> SynchronizeAsync(
        string videoFilePath, 
        string audioFilePath, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detect timecode from video file
    /// </summary>
    Task<string?> DetectTimecodeAsync(
        string videoFilePath, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculate offset between two audio files
    /// </summary>
    Task<TimeSpan> CalculateAudioOffsetAsync(
        string audioFile1, 
        string audioFile2, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verify synchronization accuracy
    /// </summary>
    Task<double> VerifySyncAccuracyAsync(
        string videoFilePath, 
        string audioFilePath, 
        TimeSpan offset,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Synchronization result
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public TimeSpan Offset { get; set; }
    public double Confidence { get; set; }
    public string? VideoTimecode { get; set; }
    public string? AudioTimecode { get; set; }
    public string? ErrorMessage { get; set; }
    public int FrameOffset { get; set; }
    public double FrameRate { get; set; }
}
