namespace Veriflow.Core.Models;

/// <summary>
/// Represents an audio track in multitrack playback
/// </summary>
public class AudioTrack
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FilePath { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    // Playback properties
    public float Volume { get; set; } = 1.0f; // 0.0 to 1.0
    public float Pan { get; set; } = 0.0f; // -1.0 (left) to 1.0 (right)
    public bool IsMuted { get; set; }
    public bool IsSolo { get; set; }
    
    // Audio properties
    public int SampleRate { get; set; }
    public int BitDepth { get; set; }
    public int Channels { get; set; }
    public TimeSpan Duration { get; set; }
    
    // Waveform data (for visualization)
    public float[]? WaveformData { get; set; }
    
    // Playback position
    public long CurrentSample { get; set; }
}
