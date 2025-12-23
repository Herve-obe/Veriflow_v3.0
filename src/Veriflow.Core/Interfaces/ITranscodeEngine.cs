using System;
using System.Threading;
using System.Threading.Tasks;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Transcoding engine interface for video/audio conversion
/// </summary>
public interface ITranscodeEngine
{
    /// <summary>
    /// Transcode a media file
    /// </summary>
    Task<TranscodeResult> TranscodeAsync(
        string inputPath,
        string outputPath,
        TranscodePreset preset,
        IProgress<TranscodeProgress>? progress = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get available presets
    /// </summary>
    TranscodePreset[] GetAvailablePresets();
    
    /// <summary>
    /// Validate input file
    /// </summary>
    Task<bool> ValidateInputAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Estimate output file size
    /// </summary>
    Task<long> EstimateOutputSizeAsync(
        string inputPath,
        TranscodePreset preset,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Transcode preset configuration
/// </summary>
public class TranscodePreset
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoCodec { get; set; } = string.Empty;
    public string AudioCodec { get; set; } = string.Empty;
    public string Container { get; set; } = string.Empty;
    public int? VideoBitrate { get; set; }
    public int? AudioBitrate { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? FrameRate { get; set; }
    public string? CustomArgs { get; set; }
}

/// <summary>
/// Transcode progress information
/// </summary>
public class TranscodeProgress
{
    public double Percentage { get; set; }
    public TimeSpan Elapsed { get; set; }
    public TimeSpan Remaining { get; set; }
    public double Speed { get; set; } // Encoding speed (e.g., 2.5x)
    public long ProcessedBytes { get; set; }
    public string CurrentFrame { get; set; } = string.Empty;
}

/// <summary>
/// Transcode result
/// </summary>
public class TranscodeResult
{
    public bool Success { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public long OutputSize { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
}
