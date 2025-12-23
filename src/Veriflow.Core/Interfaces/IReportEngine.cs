using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Report generation engine interface
/// </summary>
public interface IReportEngine
{
    /// <summary>
    /// Generate Camera Report PDF (Video profile)
    /// </summary>
    Task<string> GenerateCameraReportAsync(
        ReportData reportData,
        string outputPath,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate Sound Report PDF (Audio profile)
    /// </summary>
    Task<string> GenerateSoundReportAsync(
        ReportData reportData,
        string outputPath,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Preview report without saving
    /// </summary>
    Task<byte[]> PreviewReportAsync(
        ReportData reportData,
        ReportType type,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Report data model
/// </summary>
public class ReportData
{
    // Session Info
    public string ProjectName { get; set; } = string.Empty;
    public string ProductionCompany { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public string Location { get; set; } = string.Empty;
    
    // Camera Report Specific
    public string Director { get; set; } = string.Empty;
    public string DOP { get; set; } = string.Empty;
    public string CameraOperator { get; set; } = string.Empty;
    public string DataManager { get; set; } = string.Empty;
    public string CameraModel { get; set; } = string.Empty;
    public string LensInfo { get; set; } = string.Empty;
    
    // Sound Report Specific
    public string SoundRecordist { get; set; } = string.Empty;
    public string BoomOperator { get; set; } = string.Empty;
    public string RecorderModel { get; set; } = string.Empty;
    public string MicrophoneInfo { get; set; } = string.Empty;
    public string TimecodeRate { get; set; } = string.Empty;
    public string BitDepth { get; set; } = string.Empty;
    
    // Clips/Takes
    public List<ReportClip> Clips { get; set; } = new();
    
    // Notes
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Report clip/take entry
/// </summary>
public class ReportClip
{
    public string Scene { get; set; } = string.Empty;
    public string Take { get; set; } = string.Empty;
    public string Timecode { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string FrameRate { get; set; } = string.Empty;
    public string Codec { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsGood { get; set; }
}

/// <summary>
/// Report type enumeration
/// </summary>
public enum ReportType
{
    Camera,
    Sound
}
