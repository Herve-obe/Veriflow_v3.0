using System;

namespace Veriflow.Core.Models;

/// <summary>
/// Represents a media item in sync pool
/// </summary>
public class SyncPoolItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public MediaType Type { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Timecode { get; set; }
    public double FrameRate { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? SampleRate { get; set; }
    public int? Channels { get; set; }
    public bool IsSelected { get; set; }
    public string? ThumbnailPath { get; set; }
}

/// <summary>
/// Represents a synchronized pair
/// </summary>
public class SyncPair
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public SyncPoolItem? VideoItem { get; set; }
    public SyncPoolItem? AudioItem { get; set; }
    public TimeSpan Offset { get; set; }
    public double Confidence { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
