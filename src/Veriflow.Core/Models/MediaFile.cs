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
    public string? ChromaSubsampling { get; set; } // 4:2:0, 4:2:2, 4:4:4
    public string? ColorSpace { get; set; } // Rec.709, Rec.2020, DCI-P3, etc.
    public string? ScanType { get; set; } // Progressive, Interlaced
    public string? GopStructure { get; set; } // IBBP, Long GOP, All-I, etc.
    public double? VideoBitrate { get; set; } // Mbps
    
    // Audio properties
    public int? SampleRate { get; set; }
    public int? BitDepth { get; set; }
    public int? Channels { get; set; }
    public string? AudioCodec { get; set; }
    public double? AudioBitrate { get; set; } // kbps
    public string? BitrateMode { get; set; } // CBR, VBR
    
    // Audio metadata (BWF/iXML)
    public string? Project { get; set; }
    public string? Scene { get; set; }
    public string? Take { get; set; }
    public string? Tape { get; set; }
    public DateTime? RecordingDate { get; set; }
    public string? UserBits { get; set; }
    public List<AudioTrack>? AudioTracks { get; set; }
    
    // Metadata
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    // Timecode
    public string? Timecode { get; set; }
    public string? StartTimecode { get; set; }
    
    // Hash (for verification)
    public string? Hash { get; set; }
    public string? HashAlgorithm { get; set; }
    
    // Helper properties
    public bool IsVideo => Type == MediaType.Video;
    public bool IsAudio => Type == MediaType.Audio;
    
    // Computed properties
    public string ResolutionString => Width.HasValue && Height.HasValue ? $"{Width}x{Height}" : "Unknown";
}

public enum MediaType
{
    Unknown,
    Video,
    Audio,
    Image
}
