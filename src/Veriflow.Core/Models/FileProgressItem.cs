namespace Veriflow.Core.Models;

/// <summary>
/// Represents progress information for a single file during offload or verify operations
/// </summary>
public class FileProgressItem
{
    /// <summary>
    /// Name of the file being processed
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// xxHash64 checksum value
    /// </summary>
    public string XxHash64 { get; set; } = string.Empty;
    
    /// <summary>
    /// Status of the file operation (Success, Failed, Processing)
    /// </summary>
    public FileStatus Status { get; set; } = FileStatus.Processing;
    
    /// <summary>
    /// Optional error message if status is Failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Status of a file operation
/// </summary>
public enum FileStatus
{
    /// <summary>
    /// File is currently being processed
    /// </summary>
    Processing,
    
    /// <summary>
    /// File operation completed successfully
    /// </summary>
    Success,
    
    /// <summary>
    /// File operation failed
    /// </summary>
    Failed
}
