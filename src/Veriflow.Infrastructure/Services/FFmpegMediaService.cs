using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// FFmpeg-based media service for metadata extraction and thumbnail generation
/// </summary>
public class FFmpegMediaService : IMediaService
{
    private const string FFprobePath = "ffprobe"; // Assumes ffprobe is in PATH
    private const string FFmpegPath = "ffmpeg";   // Assumes ffmpeg is in PATH
    private const int BufferSize = 1024 * 1024;
    
    public async Task<MediaFile> GetMediaInfoAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Media file not found: {filePath}");
        
        var mediaFile = new MediaFile
        {
            FileName = Path.GetFileName(filePath),
            FilePath = filePath,
            FileSizeBytes = new FileInfo(filePath).Length,
            CreatedDate = File.GetCreationTime(filePath),
            ModifiedDate = File.GetLastWriteTime(filePath)
        };
        
        try
        {
            // Run ffprobe to get JSON metadata
            var startInfo = new ProcessStartInfo
            {
                FileName = FFprobePath,
                Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            
            if (process.ExitCode == 0)
            {
                var jsonDoc = JsonDocument.Parse(output);
                var root = jsonDoc.RootElement;
                
                // Extract format info
                if (root.TryGetProperty("format", out var format))
                {
                    if (format.TryGetProperty("duration", out var duration))
                        mediaFile.Duration = TimeSpan.FromSeconds(duration.GetDouble());
                    
                    if (format.TryGetProperty("format_name", out var formatName))
                        mediaFile.Container = formatName.GetString();
                }
                
                // Extract streams info
                if (root.TryGetProperty("streams", out var streams))
                {
                    foreach (var stream in streams.EnumerateArray())
                    {
                        if (!stream.TryGetProperty("codec_type", out var codecType))
                            continue;
                        
                        var type = codecType.GetString();
                        
                        if (type == "video")
                        {
                            mediaFile.Type = MediaType.Video;
                            
                            if (stream.TryGetProperty("codec_name", out var codec))
                                mediaFile.Codec = codec.GetString();
                            
                            if (stream.TryGetProperty("width", out var width))
                                mediaFile.Width = width.GetInt32();
                            
                            if (stream.TryGetProperty("height", out var height))
                                mediaFile.Height = height.GetInt32();
                            
                            if (stream.TryGetProperty("r_frame_rate", out var frameRate))
                            {
                                var fps = frameRate.GetString() ?? "0/1";
                                var parts = fps.Split('/');
                                if (parts.Length == 2 && double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den) && den != 0)
                                    mediaFile.FrameRate = num / den;
                            }
                        }
                        else if (type == "audio")
                        {
                            if (mediaFile.Type == MediaType.Unknown)
                                mediaFile.Type = MediaType.Audio;
                            
                            if (stream.TryGetProperty("codec_name", out var codec))
                                mediaFile.AudioCodec = codec.GetString();
                            
                            if (stream.TryGetProperty("sample_rate", out var sampleRate))
                            {
                                if (int.TryParse(sampleRate.GetString(), out var sr))
                                    mediaFile.SampleRate = sr;
                            }
                            
                            if (stream.TryGetProperty("channels", out var channels))
                                mediaFile.Channels = channels.GetInt32();
                            
                            if (stream.TryGetProperty("bits_per_sample", out var bitDepth))
                                mediaFile.BitDepth = bitDepth.GetInt32();
                        }
                    }
                }
            }
        }
        catch
        {
            // Return partial metadata on error
        }
        
        return mediaFile;
    }
    
    public async Task<byte[]> GenerateThumbnailAsync(string filePath, TimeSpan position, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Media file not found: {filePath}");
        
        var tempFile = Path.Combine(Path.GetTempPath(), $"thumb_{Guid.NewGuid()}.jpg");
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = FFmpegPath,
                Arguments = $"-ss {position.TotalSeconds} -i \"{filePath}\" -vframes 1 -vf scale=160:120 -y \"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            
            if (process.ExitCode != 0 || !File.Exists(tempFile))
                return Array.Empty<byte>();
            
            return await File.ReadAllBytesAsync(tempFile, cancellationToken);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    public async Task<float[]> GenerateWaveformAsync(string filePath, int width, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Media file not found: {filePath}");
        
        var tempFile = Path.Combine(Path.GetTempPath(), $"audio_{Guid.NewGuid()}.raw");
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = FFmpegPath,
                Arguments = $"-i \"{filePath}\" -ac 1 -f f32le -acodec pcm_f32le \"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            
            if (process.ExitCode != 0 || !File.Exists(tempFile))
                return Array.Empty<float>();
            
            // Read PCM data and downsample
            var audioData = await File.ReadAllBytesAsync(tempFile, cancellationToken);
            var floatData = new float[audioData.Length / 4];
            Buffer.BlockCopy(audioData, 0, floatData, 0, audioData.Length);
            
            // Downsample to requested width
            var waveform = new float[width];
            var step = floatData.Length / width;
            
            for (int i = 0; i < width; i++)
            {
                var start = i * step;
                var end = Math.Min(start + step, floatData.Length);
                var max = 0f;
                
                for (int j = start; j < end; j++)
                    max = Math.Max(max, Math.Abs(floatData[j]));
                
                waveform[i] = max;
            }
            
            return waveform;
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    public async Task<string> CalculateHashAsync(string filePath, string algorithm = "xxHash64", IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);
        var hash = new XxHash64();
        
        var buffer = new byte[BufferSize];
        int bytesRead;
        long totalBytes = stream.Length;
        long processedBytes = 0;
        
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            hash.Append(buffer.AsSpan(0, bytesRead));
            processedBytes += bytesRead;
            progress?.Report(processedBytes * 100.0 / totalBytes);
        }
        
        var hashBytes = hash.GetHashAndReset();
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
    
    
    public async Task<bool> VerifyHashAsync(string filePath, string expectedHash, string algorithm = "xxHash64", CancellationToken cancellationToken = default)
    {
        var actualHash = await CalculateHashAsync(filePath, algorithm, null, cancellationToken);
        return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
    
    public async Task<Dictionary<string, string>> ReadWavBwfMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var editor = new WavMetadataEditor();
        return await editor.ReadBwfMetadataAsync(filePath, cancellationToken);
    }
    
    public async Task<Dictionary<string, string>> ReadWavIxmlMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var editor = new WavMetadataEditor();
        return await editor.ReadIxmlMetadataAsync(filePath, cancellationToken);
    }
    
    public async Task WriteWavBwfMetadataAsync(string filePath, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        var editor = new WavMetadataEditor();
        await editor.WriteBwfMetadataAsync(filePath, metadata, cancellationToken);
    }
}
