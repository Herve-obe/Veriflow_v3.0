namespace Veriflow.Core.Models;

/// <summary>
/// Represents a media file (audio or video)
/// </summary>
public class MediaFile
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    
    // Media properties
    public MediaType Type { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Codec { get; set; }
    public string? Container { get; set; }
    
    // Video properties
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? FrameRate { get; set; }
    public string? AspectRatio { get; set; }
    
    // Audio properties
    public int? SampleRate { get; set; }
    public int? BitDepth { get; set; }
    public int? Channels { get; set; }
    public string? AudioCodec { get; set; }
    
    // Metadata
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    // Timecode
    public string? Timecode { get; set; }
    
    // Hash (for verification)
    public string? Hash { get; set; }
    public string? HashAlgorithm { get; set; }
}

public enum MediaType
{
    Unknown,
    Video,
    Audio,
    Image
}
