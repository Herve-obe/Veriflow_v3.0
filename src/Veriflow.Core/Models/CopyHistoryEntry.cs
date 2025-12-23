using System;

namespace Veriflow.Core.Models;

/// <summary>
/// Copy history entry for tracking offload operations
/// </summary>
public class CopyHistoryEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationAPath { get; set; } = string.Empty;
    public string DestinationBPath { get; set; } = string.Empty;
    public int FilesCount { get; set; }
    public long TotalBytes { get; set; }
    public TimeSpan Duration { get; set; }
    public string MhlPathA { get; set; } = string.Empty;
    public string MhlPathB { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Formatted file size
    /// </summary>
    public string FormattedSize => FormatBytes(TotalBytes);
    
    /// <summary>
    /// Formatted duration
    /// </summary>
    public string FormattedDuration => $"{Duration:mm\\:ss}";
    
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
