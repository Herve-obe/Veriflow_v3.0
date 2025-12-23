using System;

namespace Veriflow.Core.Models;

/// <summary>
/// Represents a transcode job in the queue
/// </summary>
public class TranscodeJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string PresetId { get; set; } = string.Empty;
    public string PresetName { get; set; } = string.Empty;
    public TranscodeStatus Status { get; set; } = TranscodeStatus.Queued;
    public double Progress { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public TimeSpan? Duration { get; set; }
    public long InputSize { get; set; }
    public long OutputSize { get; set; }
}

/// <summary>
/// Transcode job status
/// </summary>
public enum TranscodeStatus
{
    Queued,
    Processing,
    Completed,
    Failed,
    Cancelled
}
