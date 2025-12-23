using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;

namespace Veriflow.Core.Services;

/// <summary>
/// Service for secure file offloading with dual-destination copying and MHL verification
/// </summary>
public class OffloadService : IOffloadService
{
    private const int BufferSize = 1024 * 1024; // 1MB buffer for file copying
    private const int MaxConcurrentCopies = 4; // Limit concurrent file operations
    private static readonly string HistoryFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Veriflow",
        "copy_history.json"
    );
    
    public async Task<OffloadResult> OffloadAsync(
        string sourcePath,
        string destinationA,
        string destinationB,
        IProgress<OffloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new OffloadResult();
        
        try
        {
            // Validate paths
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"Source path not found: {sourcePath}");
            
            Directory.CreateDirectory(destinationA);
            Directory.CreateDirectory(destinationB);
            
            // Get all files to copy
            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            var totalBytes = files.Sum(f => new FileInfo(f).Length);
            long bytesCopied = 0;
            int filesProcessed = 0;
            
            result.TotalFiles = files.Length;
            
            // Parallel file copying with semaphore throttling
            var semaphore = new SemaphoreSlim(MaxConcurrentCopies);
            var copyTasks = files.Select(async (sourceFile, index) =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var relativePath = Path.GetRelativePath(sourcePath, sourceFile);
                    var destFileA = Path.Combine(destinationA, relativePath);
                    var destFileB = Path.Combine(destinationB, relativePath);
                    
                    // Create destination directories
                    Directory.CreateDirectory(Path.GetDirectoryName(destFileA)!);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFileB)!);
                    
                    // Copy to both destinations in parallel
                    await Task.WhenAll(
                        CopyFileWithHashAsync(sourceFile, destFileA, cancellationToken),
                        CopyFileWithHashAsync(sourceFile, destFileB, cancellationToken)
                    );
                    
                    // Thread-safe progress update
                    var fileSize = new FileInfo(sourceFile).Length;
                    var currentBytesCopied = Interlocked.Add(ref bytesCopied, fileSize);
                    var currentFilesProcessed = Interlocked.Increment(ref filesProcessed);
                    
                    // Calculate ETA and transfer speed
                    var elapsed = stopwatch.Elapsed.TotalSeconds;
                    var transferSpeed = elapsed > 0 ? currentBytesCopied / elapsed : 0;
                    var bytesRemaining = totalBytes - currentBytesCopied;
                    var eta = transferSpeed > 0 ? TimeSpan.FromSeconds(bytesRemaining / transferSpeed) : TimeSpan.Zero;
                    
                    // Report progress
                    progress?.Report(new OffloadProgress
                    {
                        CurrentFile = relativePath,
                        BytesCopied = currentBytesCopied,
                        TotalBytes = totalBytes,
                        FilesProcessed = currentFilesProcessed,
                        TotalFiles = files.Length,
                        Status = $"Copying {relativePath}... ({currentFilesProcessed}/{files.Length})",
                        EstimatedTimeRemaining = eta,
                        TransferSpeed = transferSpeed,
                        CurrentDestination = "Both"
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            });
            
            await Task.WhenAll(copyTasks);
            
            result.BytesCopied = bytesCopied;
            result.FilesProcessed = filesProcessed;
            
            // Generate MHL files
            progress?.Report(new OffloadProgress
            {
                CurrentFile = "Generating MHL...",
                BytesCopied = bytesCopied,
                TotalBytes = totalBytes,
                FilesProcessed = files.Length,
                TotalFiles = files.Length,
                Status = "Generating MHL verification files..."
            });
            
            var mhlPathA = Path.Combine(destinationA, $"veriflow_{DateTime.Now:yyyyMMdd_HHmmss}.mhl");
            var mhlPathB = Path.Combine(destinationB, $"veriflow_{DateTime.Now:yyyyMMdd_HHmmss}.mhl");
            
            await GenerateMhlAsync(destinationA, mhlPathA);
            await GenerateMhlAsync(destinationB, mhlPathB);
            
            result.MhlPathA = mhlPathA;
            result.MhlPathB = mhlPathB;
            result.Success = true;
            
            progress?.Report(new OffloadProgress
            {
                CurrentFile = "Complete",
                BytesCopied = bytesCopied,
                TotalBytes = totalBytes,
                FilesProcessed = files.Length,
                TotalFiles = files.Length,
                Status = "Offload complete!"
            });
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }
        
        // Add to history if successful
        if (result.Success)
        {
            var historyEntry = new CopyHistoryEntry
            {
                Timestamp = DateTime.Now,
                SourcePath = sourcePath,
                DestinationAPath = destinationA,
                DestinationBPath = destinationB,
                FilesCount = result.FilesProcessed,
                TotalBytes = result.BytesCopied,
                Duration = result.Duration,
                MhlPathA = result.MhlPathA ?? string.Empty,
                MhlPathB = result.MhlPathB ?? string.Empty,
                Success = true
            };
            
            await AddHistoryEntryAsync(historyEntry);
        }
        
        return result;
    }
    
    public async Task<VerifyResult> VerifyAsync(
        string sourcePath,
        string destinationA,
        string destinationB,
        IProgress<OffloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new VerifyResult();
        var mismatches = new List<string>();
        
        try
        {
            // Find MHL files
            var mhlFilesA = Directory.GetFiles(destinationA, "*.mhl");
            var mhlFilesB = Directory.GetFiles(destinationB, "*.mhl");
            
            if (mhlFilesA.Length == 0 || mhlFilesB.Length == 0)
            {
                result.Success = false;
                result.ErrorMessage = "MHL files not found in one or both destinations";
                return result;
            }
            
            // Use the most recent MHL file
            var mhlFileA = mhlFilesA.OrderByDescending(f => File.GetCreationTime(f)).First();
            var mhlFileB = mhlFilesB.OrderByDescending(f => File.GetCreationTime(f)).First();
            
            // Parse MHL and verify hashes
            var hashesA = ParseMhlFile(mhlFileA);
            var hashesB = ParseMhlFile(mhlFileB);
            
            int filesVerified = 0;
            int totalFiles = hashesA.Count;
            
            foreach (var kvp in hashesA)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var filePath = kvp.Key;
                var expectedHash = kvp.Value;
                
                progress?.Report(new OffloadProgress
                {
                    CurrentFile = filePath,
                    FilesProcessed = filesVerified,
                    TotalFiles = totalFiles,
                    Status = $"Verifying {filePath}..."
                });
                
                // Verify file exists and hash matches in destination A
                var fullPathA = Path.Combine(destinationA, filePath);
                if (!File.Exists(fullPathA))
                {
                    mismatches.Add($"{filePath} (missing in A)");
                    continue;
                }
                
                var actualHashA = await ComputeXxHash64Async(fullPathA, cancellationToken);
                if (actualHashA != expectedHash)
                {
                    mismatches.Add($"{filePath} (hash mismatch in A)");
                }
                
                // Verify file exists and hash matches in destination B
                var fullPathB = Path.Combine(destinationB, filePath);
                if (!File.Exists(fullPathB))
                {
                    mismatches.Add($"{filePath} (missing in B)");
                    continue;
                }
                
                var actualHashB = await ComputeXxHash64Async(fullPathB, cancellationToken);
                if (actualHashB != expectedHash)
                {
                    mismatches.Add($"{filePath} (hash mismatch in B)");
                }
                
                filesVerified++;
            }
            
            result.FilesVerified = filesVerified;
            result.MismatchCount = mismatches.Count;
            result.MismatchedFiles = mismatches.ToArray();
            result.Success = mismatches.Count == 0;
            
            progress?.Report(new OffloadProgress
            {
                CurrentFile = "Complete",
                FilesProcessed = filesVerified,
                TotalFiles = totalFiles,
                Status = result.Success ? "Verification successful!" : $"Verification failed: {mismatches.Count} mismatches"
            });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }
        
        return result;
    }
    
    public async Task GenerateMhlAsync(string directoryPath, string outputPath)
    {
        var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".mhl", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        
        var hashList = new XElement("hashlist",
            new XAttribute("version", "1.1"),
            new XElement("creatorinfo",
                new XElement("name", "Veriflow 3.0"),
                new XElement("version", "3.0.0"),
                new XElement("username", Environment.UserName),
                new XElement("hostname", Environment.MachineName),
                new XElement("tool", "Veriflow OFFLOAD Module")
            )
        );
        
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(directoryPath, file);
            var fileInfo = new FileInfo(file);
            var hash = await ComputeXxHash64Async(file, CancellationToken.None);
            
            var hashElement = new XElement("hash",
                new XElement("file", relativePath.Replace("\\", "/")),
                new XElement("size", fileInfo.Length),
                new XElement("lastmodificationdate", fileInfo.LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                new XElement("xxhash64", hash)
            );
            
            hashList.Add(hashElement);
        }
        
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            hashList
        );
        
        await File.WriteAllTextAsync(outputPath, doc.ToString(), Encoding.UTF8);
    }
    
    private async Task CopyFileWithHashAsync(string sourceFile, string destFile, CancellationToken cancellationToken)
    {
        using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);
        using var destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);
        
        await sourceStream.CopyToAsync(destStream, BufferSize, cancellationToken);
        
        // Preserve timestamps
        File.SetCreationTime(destFile, File.GetCreationTime(sourceFile));
        File.SetLastWriteTime(destFile, File.GetLastWriteTime(sourceFile));
    }
    
    private async Task<string> ComputeXxHash64Async(string filePath, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);
        var hash = new XxHash64();
        
        var buffer = new byte[BufferSize];
        int bytesRead;
        
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            hash.Append(buffer.AsSpan(0, bytesRead));
        }
        
        var hashBytes = hash.GetHashAndReset();
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
    
    private Dictionary<string, string> ParseMhlFile(string mhlPath)
    {
        var hashes = new Dictionary<string, string>();
        var doc = XDocument.Load(mhlPath);
        
        foreach (var hashElement in doc.Descendants("hash"))
        {
            var file = hashElement.Element("file")?.Value;
            var xxhash = hashElement.Element("xxhash64")?.Value;
            
            if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(xxhash))
            {
                hashes[file.Replace("/", "\\")] = xxhash;
            }
        }
        
        return hashes;
    }
    
    public async Task<List<CopyHistoryEntry>> GetHistoryAsync()
    {
        try
        {
            if (!File.Exists(HistoryFilePath))
                return new List<CopyHistoryEntry>();
            
            var json = await File.ReadAllTextAsync(HistoryFilePath);
            var history = JsonSerializer.Deserialize<List<CopyHistoryEntry>>(json);
            return history ?? new List<CopyHistoryEntry>();
        }
        catch
        {
            return new List<CopyHistoryEntry>();
        }
    }
    
    public async Task AddHistoryEntryAsync(CopyHistoryEntry entry)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(HistoryFilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            
            var history = await GetHistoryAsync();
            history.Insert(0, entry); // Add to beginning (most recent first)
            
            // Keep only last 100 entries
            if (history.Count > 100)
                history = history.Take(100).ToList();
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(history, options);
            await File.WriteAllTextAsync(HistoryFilePath, json);
        }
        catch (Exception ex)
        {
            CrashLogger.LogException(ex, "AddHistoryEntryAsync");
        }
    }
    
    public async Task ClearHistoryAsync()
    {
        try
        {
            if (File.Exists(HistoryFilePath))
                File.Delete(HistoryFilePath);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            CrashLogger.LogException(ex, "ClearHistoryAsync");
        }
    }
}
