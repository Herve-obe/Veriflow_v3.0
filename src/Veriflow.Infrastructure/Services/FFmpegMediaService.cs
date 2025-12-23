using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    
    public async Task<MediaMetadata> GetMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Media file not found: {filePath}");
        
        var metadata = new MediaMetadata
        {
            FileName = Path.GetFileName(filePath),
            FilePath = filePath,
            FileSize = new FileInfo(filePath).Length,
            CreationDate = File.GetCreationTime(filePath),
            ModificationDate = File.GetLastWriteTime(filePath)
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
            
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                throw new Exception($"FFprobe failed: {error}");
            }
            
            // Parse JSON output
            var jsonDoc = JsonDocument.Parse(output);
            var root = jsonDoc.RootElement;
            
            // Extract format info
            if (root.TryGetProperty("format", out var format))
            {
                if (format.TryGetProperty("format_name", out var formatName))
                    metadata.Format = formatName.GetString() ?? string.Empty;
                
                if (format.TryGetProperty("duration", out var duration))
                    metadata.Duration = TimeSpan.FromSeconds(duration.GetDouble());
                
                if (format.TryGetProperty("bit_rate", out var bitrate))
                    metadata.Bitrate = bitrate.GetInt64();
                
                // Extract timecode from tags
                if (format.TryGetProperty("tags", out var tags))
                {
                    if (tags.TryGetProperty("timecode", out var timecode))
                        metadata.Timecode = timecode.GetString() ?? string.Empty;
                    
                    // Extract custom metadata
                    foreach (var tag in tags.EnumerateObject())
                    {
                        metadata.CustomMetadata[tag.Name] = tag.Value.GetString() ?? string.Empty;
                    }
                }
            }
            
            // Extract streams info
            if (root.TryGetProperty("streams", out var streams))
            {
                foreach (var stream in streams.EnumerateArray())
                {
                    if (!stream.TryGetProperty("codec_type", out var codecType))
                        continue;
                    
                    var type = codecType.GetString();
                    
                    if (type == "video" && !metadata.HasVideo)
                    {
                        metadata.HasVideo = true;
                        
                        if (stream.TryGetProperty("codec_name", out var codec))
                            metadata.VideoCodec = codec.GetString() ?? string.Empty;
                        
                        if (stream.TryGetProperty("width", out var width))
                            metadata.Width = width.GetInt32();
                        
                        if (stream.TryGetProperty("height", out var height))
                            metadata.Height = height.GetInt32();
                        
                        if (stream.TryGetProperty("r_frame_rate", out var frameRate))
                        {
                            var fps = frameRate.GetString() ?? "0/1";
                            var parts = fps.Split('/');
                            if (parts.Length == 2 && double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den) && den != 0)
                                metadata.FrameRate = num / den;
                        }
                        
                        if (stream.TryGetProperty("display_aspect_ratio", out var aspectRatio))
                            metadata.AspectRatio = aspectRatio.GetString() ?? string.Empty;
                        
                        if (stream.TryGetProperty("pix_fmt", out var pixFmt))
                            metadata.ColorSpace = pixFmt.GetString() ?? string.Empty;
                        
                        if (stream.TryGetProperty("bits_per_raw_sample", out var bitDepth))
                            metadata.BitDepth = bitDepth.GetInt32();
                    }
                    else if (type == "audio" && !metadata.HasAudio)
                    {
                        metadata.HasAudio = true;
                        
                        if (stream.TryGetProperty("codec_name", out var codec))
                            metadata.AudioCodec = codec.GetString() ?? string.Empty;
                        
                        if (stream.TryGetProperty("sample_rate", out var sampleRate))
                            int.TryParse(sampleRate.GetString(), out var sr) ? metadata.SampleRate = sr : 0;
                        
                        if (stream.TryGetProperty("channels", out var channels))
                            metadata.Channels = channels.GetInt32();
                        
                        if (stream.TryGetProperty("bit_rate", out var audioBitrate))
                            int.TryParse(audioBitrate.GetString(), out var abr) ? metadata.AudioBitrate = abr : 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but return partial metadata
            metadata.CustomMetadata["error"] = ex.Message;
        }
        
        return metadata;
    }
    
    public async Task<byte[]> GenerateThumbnailAsync(string filePath, double timeInSeconds = 0, int width = 160, int height = 120, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Media file not found: {filePath}");
        
        var tempFile = Path.Combine(Path.GetTempPath(), $"thumb_{Guid.NewGuid()}.jpg");
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = FFmpegPath,
                Arguments = $"-ss {timeInSeconds} -i \"{filePath}\" -vframes 1 -vf scale={width}:{height} -y \"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            
            if (process.ExitCode != 0 || !File.Exists(tempFile))
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                throw new Exception($"FFmpeg thumbnail generation failed: {error}");
            }
            
            return await File.ReadAllBytesAsync(tempFile, cancellationToken);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    public async Task<float[]> GenerateWaveformDataAsync(string filePath, int samples = 1000, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Media file not found: {filePath}");
        
        // Extract audio to raw PCM data
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
            
            // Downsample to requested number of samples
            var waveform = new float[samples];
            var step = floatData.Length / samples;
            
            for (int i = 0; i < samples; i++)
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
    
    public async Task<bool> ValidateFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return false;
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = FFprobePath,
                Arguments = $"-v error \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            
            return string.IsNullOrWhiteSpace(error);
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<string> ExtractTimecodeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var metadata = await GetMetadataAsync(filePath, cancellationToken);
        return metadata.Timecode;
    }
}
