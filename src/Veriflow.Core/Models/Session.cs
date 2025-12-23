namespace Veriflow.Core.Models;

/// <summary>
/// Represents a Veriflow session (.vfsession)
/// </summary>
public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Untitled Session";
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    
    // Current state
    public ProfileMode CurrentProfile { get; set; } = ProfileMode.Video;
    public string CurrentPage { get; set; } = "OFFLOAD";
    
    // Session data
    public List<MediaFile> LoadedMedia { get; set; } = new();
    public Dictionary<string, object> ModuleData { get; set; } = new();
    
    // File path
    public string? FilePath { get; set; }
    public bool IsModified { get; set; }
}

public enum ProfileMode
{
    Video,
    Audio
}
