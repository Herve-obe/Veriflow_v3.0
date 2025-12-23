using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veriflow.Core.Interfaces;

namespace Veriflow.Core.Services;

/// <summary>
/// Service for generating EDL (Edit Decision List) and ALE (Avid Log Exchange) files
/// </summary>
public class EdlAleService
{
    /// <summary>
    /// Generate CMX 3600 EDL file
    /// </summary>
    public async Task<string> GenerateEdlAsync(
        List<ReportClip> clips,
        string outputPath,
        string title = "VERIFLOW EXPORT",
        double frameRate = 25.0,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        
        // EDL Header
        sb.AppendLine($"TITLE: {title}");
        sb.AppendLine($"FCM: NON-DROP FRAME");
        sb.AppendLine();
        
        // EDL Events
        int eventNumber = 1;
        foreach (var clip in clips)
        {
            // Event line: 001  AX       V     C        00:00:00:00 00:00:05:00 00:00:00:00 00:00:05:00
            // Format: EventNum  Reel  Track  Trans  SourceIn  SourceOut  RecordIn  RecordOut
            
            var reel = Path.GetFileNameWithoutExtension(clip.FileName).ToUpper();
            if (reel.Length > 8) reel = reel.Substring(0, 8); // Max 8 chars for reel name
            
            sb.AppendLine($"{eventNumber:D3}  {reel,-8} V     C        {clip.Timecode,-11} {GetEndTimecode(clip.Timecode, clip.Duration, frameRate),-11} {clip.Timecode,-11} {GetEndTimecode(clip.Timecode, clip.Duration, frameRate),-11}");
            
            // Comment lines
            if (!string.IsNullOrEmpty(clip.Scene))
                sb.AppendLine($"* FROM CLIP NAME: {clip.FileName}");
            if (!string.IsNullOrEmpty(clip.Notes))
                sb.AppendLine($"* COMMENT: {clip.Notes}");
            
            sb.AppendLine();
            eventNumber++;
        }
        
        await File.WriteAllTextAsync(outputPath, sb.ToString(), cancellationToken);
        return outputPath;
    }
    
    /// <summary>
    /// Generate Avid ALE file
    /// </summary>
    public async Task<string> GenerateAleAsync(
        List<ReportClip> clips,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        
        // ALE Header
        sb.AppendLine("Heading");
        sb.AppendLine("FIELD_DELIM\tTABS");
        sb.AppendLine("VIDEO_FORMAT\t1080p");
        sb.AppendLine("AUDIO_FORMAT\t48kHz");
        sb.AppendLine("FPS\t25");
        sb.AppendLine();
        
        // Column Headers
        sb.AppendLine("Column");
        sb.AppendLine("Name\tTape\tScene\tTake\tStart\tEnd\tDuration\tFPS\tResolution\tCodec\tNotes");
        sb.AppendLine();
        
        // Data
        sb.AppendLine("Data");
        foreach (var clip in clips)
        {
            var name = Path.GetFileNameWithoutExtension(clip.FileName);
            var tape = clip.FileName;
            var scene = clip.Scene;
            var take = clip.Take;
            var start = clip.Timecode;
            var end = clip.Timecode; // Would need calculation
            var duration = clip.Duration;
            var fps = clip.FrameRate;
            var resolution = clip.Resolution;
            var codec = clip.Codec;
            var notes = clip.Notes?.Replace("\t", " ").Replace("\n", " "); // Remove tabs/newlines
            
            sb.AppendLine($"{name}\t{tape}\t{scene}\t{take}\t{start}\t{end}\t{duration}\t{fps}\t{resolution}\t{codec}\t{notes}");
        }
        
        await File.WriteAllTextAsync(outputPath, sb.ToString(), cancellationToken);
        return outputPath;
    }
    
    private string GetEndTimecode(string startTimecode, string duration, double frameRate)
    {
        // Parse start timecode (HH:MM:SS:FF)
        var startParts = startTimecode.Split(':');
        if (startParts.Length != 4) return startTimecode;
        
        int startHours = int.Parse(startParts[0]);
        int startMinutes = int.Parse(startParts[1]);
        int startSeconds = int.Parse(startParts[2]);
        int startFrames = int.Parse(startParts[3]);
        
        // Parse duration (HH:MM:SS:FF or seconds)
        int durationFrames = 0;
        if (duration.Contains(':'))
        {
            var durParts = duration.Split(':');
            if (durParts.Length == 4)
            {
                durationFrames = int.Parse(durParts[0]) * 3600 * (int)frameRate +
                                int.Parse(durParts[1]) * 60 * (int)frameRate +
                                int.Parse(durParts[2]) * (int)frameRate +
                                int.Parse(durParts[3]);
            }
        }
        else
        {
            // Duration in seconds
            if (double.TryParse(duration, out var seconds))
            {
                durationFrames = (int)(seconds * frameRate);
            }
        }
        
        // Convert start to total frames
        int totalStartFrames = startHours * 3600 * (int)frameRate +
                              startMinutes * 60 * (int)frameRate +
                              startSeconds * (int)frameRate +
                              startFrames;
        
        // Add duration
        int totalEndFrames = totalStartFrames + durationFrames;
        
        // Convert back to timecode
        int endHours = totalEndFrames / (3600 * (int)frameRate);
        int remainder = totalEndFrames % (3600 * (int)frameRate);
        int endMinutes = remainder / (60 * (int)frameRate);
        remainder = remainder % (60 * (int)frameRate);
        int endSeconds = remainder / (int)frameRate;
        int endFrames = remainder % (int)frameRate;
        
        return $"{endHours:D2}:{endMinutes:D2}:{endSeconds:D2}:{endFrames:D2}";
    }
}
