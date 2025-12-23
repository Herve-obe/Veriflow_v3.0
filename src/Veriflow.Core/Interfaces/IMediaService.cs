using Veriflow.Core.Models;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Interface for media file operations
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Extract metadata from a media file
    /// </summary>
    Task<MediaFile> GetMediaInfoAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate a thumbnail for a video file
    /// </summary>
    Task<byte[]> GenerateThumbnailAsync(string filePath, TimeSpan position, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate waveform data for an audio file
    /// </summary>
    Task<float[]> GenerateWaveformAsync(string filePath, int width, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculate hash for a file
    /// </summary>
    Task<string> CalculateHashAsync(string filePath, string algorithm = "xxHash64", IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verify file hash
    /// </summary>
    Task<bool> VerifyHashAsync(string filePath, string expectedHash, string algorithm = "xxHash64", CancellationToken cancellationToken = default);
}
