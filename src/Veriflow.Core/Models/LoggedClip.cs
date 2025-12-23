using System;

namespace Veriflow.Core.Models;

/// <summary>
/// Logged clip entry for Player module
/// </summary>
public class LoggedClip
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Scene { get; set; } = string.Empty;
    public string Take { get; set; } = string.Empty;
    public string TimecodeIn { get; set; } = string.Empty;
    public string TimecodeOut { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int Rating { get; set; } // 0-5 stars
    public bool IsGood { get; set; } = true;
    public DateTime LoggedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Markers within the clip (timecode positions)
    /// </summary>
    public List<string> Markers { get; set; } = new();
}
