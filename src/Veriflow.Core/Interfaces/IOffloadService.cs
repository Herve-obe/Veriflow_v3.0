using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Veriflow.Core.Models;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Service for secure file offloading with dual-destination copying
/// </summary>
public interface IOffloadService
{
    /// <summary>
    /// Copy files from source to dual destinations with progress reporting
    /// </summary>
    Task<OffloadResult> OffloadAsync(
        string sourcePath,
        string destinationA,
        string destinationB,
        IProgress<OffloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verify existing offload by comparing hashes
    /// </summary>
    Task<VerifyResult> VerifyAsync(
        string sourcePath,
        string destinationA,
        string destinationB,
        IProgress<OffloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate MHL 1.1 hash file
    /// </summary>
    Task GenerateMhlAsync(string directoryPath, string outputPath);
    
    /// <summary>
    /// Get copy history entries
    /// </summary>
    Task<List<CopyHistoryEntry>> GetHistoryAsync();
    
    /// <summary>
    /// Add entry to copy history
    /// </summary>
    Task AddHistoryEntryAsync(CopyHistoryEntry entry);
    
    /// <summary>
    /// Clear all copy history
    /// </summary>
    Task ClearHistoryAsync();
}

/// <summary>
/// Progress information for offload operation
/// </summary>
public class OffloadProgress
{
    public string CurrentFile { get; set; } = string.Empty;
    public long BytesCopied { get; set; }
    public long TotalBytes { get; set; }
    public int FilesProcessed { get; set; }
    public int TotalFiles { get; set; }
    public double PercentComplete => TotalBytes > 0 ? (BytesCopied * 100.0 / TotalBytes) : 0;
    public string Status { get; set; } = string.Empty;
    
    // New properties for enhanced progress tracking
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public double TransferSpeed { get; set; } // bytes per second
    public string CurrentDestination { get; set; } = string.Empty; // "A", "B", or "Both"
}

/// <summary>
/// Result of offload operation
/// </summary>
public class OffloadResult
{
    public bool Success { get; set; }
    public int TotalFiles { get; set; }
    public int FilesProcessed { get; set; }
    public long BytesCopied { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public string? MhlPathA { get; set; }
    public string? MhlPathB { get; set; }
}

/// <summary>
/// Result of verify operation
/// </summary>
public class VerifyResult
{
    public bool Success { get; set; }
    public int FilesVerified { get; set; }
    public int MismatchCount { get; set; }
    public string[] MismatchedFiles { get; set; } = Array.Empty<string>();
    public string? ErrorMessage { get; set; }
}
