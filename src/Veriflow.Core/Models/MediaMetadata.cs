using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Veriflow.Core.Models;

/// <summary>
/// Metadata information extracted from media files
/// </summary>
public class MediaMetadata
{
    // General Info
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
    
    // Format Info
    public string Format { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public long Bitrate { get; set; }
    
    // Video Info (if applicable)
    public bool HasVideo { get; set; }
    public string VideoCodec { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
    public string AspectRatio { get; set; } = string.Empty;
    public string ColorSpace { get; set; } = string.Empty;
    public int BitDepth { get; set; }
    
    // Audio Info (if applicable)
    public bool HasAudio { get; set; }
    public string AudioCodec { get; set; } = string.Empty;
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int AudioBitrate { get; set; }
    
    // Timecode Info
    public string Timecode { get; set; } = string.Empty;
    public string TimecodeFormat { get; set; } = string.Empty;
    
    // BWF/iXML Metadata (for professional audio)
    public Dictionary<string, string> CustomMetadata { get; set; } = new();
    
    // Thumbnail
    public byte[]? ThumbnailData { get; set; }
}
