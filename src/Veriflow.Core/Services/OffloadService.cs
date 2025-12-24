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
    private const int BufferSize = 4 * 1024 * 1024; // 4MB buffer for better throughput
    private const int MaxConcurrentCopies = 2; // Reduced for stability (prevents freezes)
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
            
            // Only create Destination B if provided (optional)
            if (!string.IsNullOrEmpty(destinationB))
            {
                Directory.CreateDirectory(destinationB);
            }
            
            
            // STEP 1: Copy all directory structure first (including empty folders)
            var allDirectories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
            foreach (var sourceDir in allDirectories)
            {
                var relativePath = Path.GetRelativePath(sourcePath, sourceDir);
                var destDirA = Path.Combine(destinationA, relativePath);
                
                Directory.CreateDirectory(destDirA);
                
                if (!string.IsNullOrEmpty(destinationB))
                {
                    var destDirB = Path.Combine(destinationB, relativePath);
                    Directory.CreateDirectory(destDirB);
                }
            }
            
            // STEP 2: Get all files to copy
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
                    
                    // Directories already created in STEP 1
                    
                    // Copy to Destination A (required)
                    await CopyFileWithHashAsync(sourceFile, destFileA, cancellationToken);
                    
                    // Copy to Destination B only if provided (optional)
                    if (!string.IsNullOrEmpty(destinationB))
                    {
                        var destFileB = Path.Combine(destinationB, relativePath);
                        await CopyFileWithHashAsync(sourceFile, destFileB, cancellationToken);
                    }
                    
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
            await GenerateMhlAsync(destinationA, mhlPathA);
            result.MhlPathA = mhlPathA;
            
            // Only generate MHL for Destination B if provided (optional)
            if (!string.IsNullOrEmpty(destinationB))
            {
                var mhlPathB = Path.Combine(destinationB, $"veriflow_{DateTime.Now:yyyyMMdd_HHmmss}.mhl");
                await GenerateMhlAsync(destinationB, mhlPathB);
                result.MhlPathB = mhlPathB;
            }
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
        string targetPath,
        IProgress<OffloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new VerifyResult();
        var mismatches = new List<string>();
        
        try
        {
            // Find MHL file in target folder
            var mhlFiles = Directory.GetFiles(targetPath, "*.mhl");
            
            if (mhlFiles.Length == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No MHL file found in target folder";
                return result;
            }
            
            // Use the most recent MHL file
            var mhlFile = mhlFiles.OrderByDescending(f => File.GetCreationTime(f)).First();
            
            // Parse MHL and verify hashes
            var expectedHashes = ParseMhlFile(mhlFile);
            
            int filesVerified = 0;
            int totalFiles = expectedHashes.Count;
            
            foreach (var kvp in expectedHashes)
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
                
                // Verify file exists and hash matches
                var fullPath = Path.Combine(targetPath, filePath);
                if (!File.Exists(fullPath))
                {
                    mismatches.Add($"{filePath} (missing)");
                    continue;
                }
                
                var actualHash = await ComputeXxHash64Async(fullPath, cancellationToken);
                if (actualHash != expectedHash)
                {
                    mismatches.Add($"{filePath} (hash mismatch)");
                }
                else
                {
                    filesVerified++;
                }
            }
            
            result.FilesVerified = filesVerified;
            result.MismatchCount = mismatches.Count;
            result.MismatchedFiles = mismatches.ToArray();
            result.Success = mismatches.Count == 0;
            
            progress?.Report(new OffloadProgress
            {
                Status = result.Success ? "Verification complete!" : "Verification failed"
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
