using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// FFmpeg-based transcoding engine
/// Professional video/audio conversion with presets
/// </summary>
public class FFmpegTranscodeEngine : ITranscodeEngine
{
    private static readonly TranscodePreset[] _presets = new[]
    {
        // ProRes Presets
        new TranscodePreset
        {
            Id = "prores_422",
            Name = "Apple ProRes 422",
            Description = "Professional editing codec (10-bit)",
            VideoCodec = "prores_ks",
            AudioCodec = "pcm_s24le",
            Container = "mov",
            CustomArgs = "-profile:v 2"
        },
        new TranscodePreset
        {
            Id = "prores_422hq",
            Name = "Apple ProRes 422 HQ",
            Description = "High quality editing (10-bit)",
            VideoCodec = "prores_ks",
            AudioCodec = "pcm_s24le",
            Container = "mov",
            CustomArgs = "-profile:v 3"
        },
        
        // DNxHD Presets
        new TranscodePreset
        {
            Id = "dnxhd_1080p_175",
            Name = "DNxHD 1080p 175Mbps",
            Description = "Avid DNxHD for 1080p",
            VideoCodec = "dnxhd",
            AudioCodec = "pcm_s16le",
            Container = "mov",
            VideoBitrate = 175000,
            Width = 1920,
            Height = 1080
        },
        
        // H.264 Presets
        new TranscodePreset
        {
            Id = "h264_high",
            Name = "H.264 High Quality",
            Description = "High quality H.264 (8-bit)",
            VideoCodec = "libx264",
            AudioCodec = "aac",
            Container = "mp4",
            CustomArgs = "-preset slow -crf 18"
        },
        new TranscodePreset
        {
            Id = "h264_medium",
            Name = "H.264 Medium Quality",
            Description = "Balanced quality/size",
            VideoCodec = "libx264",
            AudioCodec = "aac",
            Container = "mp4",
            CustomArgs = "-preset medium -crf 23"
        },
        
        // H.265 Presets
        new TranscodePreset
        {
            Id = "h265_high",
            Name = "H.265 (HEVC) High Quality",
            Description = "High efficiency codec (10-bit)",
            VideoCodec = "libx265",
            AudioCodec = "aac",
            Container = "mp4",
            CustomArgs = "-preset slow -crf 20 -pix_fmt yuv420p10le"
        },
        
        // Audio Only
        new TranscodePreset
        {
            Id = "audio_wav",
            Name = "WAV Audio (PCM)",
            Description = "Uncompressed audio",
            VideoCodec = "",
            AudioCodec = "pcm_s24le",
            Container = "wav",
            CustomArgs = "-vn"
        },
        new TranscodePreset
        {
            Id = "audio_aac",
            Name = "AAC Audio",
            Description = "Compressed audio",
            VideoCodec = "",
            AudioCodec = "aac",
            Container = "m4a",
            AudioBitrate = 256,
            CustomArgs = "-vn"
        }
    };
    
    public TranscodePreset[] GetAvailablePresets()
    {
        return _presets;
    }
    
    public async Task<TranscodeResult> TranscodeAsync(
        string inputPath,
        string outputPath,
        TranscodePreset preset,
        IProgress<TranscodeProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        
        try
        {
            // Build FFmpeg command
            var args = BuildFFmpegArgs(inputPath, outputPath, preset);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = startInfo };
            
            // Get input duration for progress calculation
            var inputDuration = await GetDurationAsync(inputPath, cancellationToken);
            
            // Start process
            process.Start();
            
            // Monitor progress
            var progressTask = Task.Run(() =>
            {
                while (!process.StandardError.EndOfStream)
                {
                    var line = process.StandardError.ReadLine();
                    if (line == null) continue;
                    
                    var progressInfo = ParseProgress(line, inputDuration, startTime);
                    if (progressInfo != null)
                    {
                        progress?.Report(progressInfo);
                    }
                }
            }, cancellationToken);
            
            // Wait for completion
            await process.WaitForExitAsync(cancellationToken);
            await progressTask;
            
            var duration = DateTime.Now - startTime;
            
            if (process.ExitCode == 0 && File.Exists(outputPath))
            {
                var outputSize = new FileInfo(outputPath).Length;
                
                return new TranscodeResult
                {
                    Success = true,
                    OutputPath = outputPath,
                    OutputSize = outputSize,
                    Duration = duration
                };
            }
            else
            {
                return new TranscodeResult
                {
                    Success = false,
                    ErrorMessage = $"FFmpeg exited with code {process.ExitCode}"
                };
            }
        }
        catch (Exception ex)
        {
            return new TranscodeResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    public async Task<bool> ValidateInputAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return false;
        
        try
        {
            var duration = await GetDurationAsync(filePath, cancellationToken);
            return duration > TimeSpan.Zero;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<long> EstimateOutputSizeAsync(
        string inputPath,
        TranscodePreset preset,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inputSize = new FileInfo(inputPath).Length;
            var duration = await GetDurationAsync(inputPath, cancellationToken);
            
            // Rough estimation based on bitrate
            if (preset.VideoBitrate.HasValue)
            {
                var estimatedSize = (long)(preset.VideoBitrate.Value * 1000 / 8 * duration.TotalSeconds);
                return estimatedSize;
            }
            
            // Default: assume similar size for lossless, smaller for compressed
            return preset.VideoCodec.Contains("prores") ? inputSize * 2 : inputSize / 2;
        }
        catch
        {
            return 0;
        }
    }
    
    private string BuildFFmpegArgs(string inputPath, string outputPath, TranscodePreset preset)
    {
        var args = new List<string>
        {
            "-i", $"\"{inputPath}\"",
            "-y" // Overwrite output
        };
        
        // Video codec
        if (!string.IsNullOrEmpty(preset.VideoCodec))
        {
            args.Add("-c:v");
            args.Add(preset.VideoCodec);
            
            if (preset.VideoBitrate.HasValue)
            {
                args.Add("-b:v");
                args.Add($"{preset.VideoBitrate}k");
            }
            
            if (preset.Width.HasValue && preset.Height.HasValue)
            {
                args.Add("-s");
                args.Add($"{preset.Width}x{preset.Height}");
            }
            
            if (preset.FrameRate.HasValue)
            {
                args.Add("-r");
                args.Add(preset.FrameRate.Value.ToString());
            }
        }
        
        // Audio codec
        if (!string.IsNullOrEmpty(preset.AudioCodec))
        {
            args.Add("-c:a");
            args.Add(preset.AudioCodec);
            
            if (preset.AudioBitrate.HasValue)
            {
                args.Add("-b:a");
                args.Add($"{preset.AudioBitrate}k");
            }
        }
        
        // Custom arguments
        if (!string.IsNullOrEmpty(preset.CustomArgs))
        {
            args.Add(preset.CustomArgs);
        }
        
        // Output
        args.Add($"\"{outputPath}\"");
        
        return string.Join(" ", args);
    }
    
    private async Task<TimeSpan> GetDurationAsync(string filePath, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v quiet -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(startInfo);
        if (process == null) return TimeSpan.Zero;
        
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        
        if (double.TryParse(output.Trim(), out var seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }
        
        return TimeSpan.Zero;
    }
    
    private TranscodeProgress? ParseProgress(string line, TimeSpan totalDuration, DateTime startTime)
    {
        // Parse FFmpeg progress output
        // Example: frame= 1234 fps=30 q=28.0 size=   12345kB time=00:01:23.45 bitrate=1234.5kbits/s speed=1.5x
        
        var timeMatch = Regex.Match(line, @"time=(\d{2}):(\d{2}):(\d{2}\.\d{2})");
        var speedMatch = Regex.Match(line, @"speed=\s*(\d+\.?\d*)x");
        
        if (timeMatch.Success)
        {
            var hours = int.Parse(timeMatch.Groups[1].Value);
            var minutes = int.Parse(timeMatch.Groups[2].Value);
            var seconds = double.Parse(timeMatch.Groups[3].Value);
            
            var currentTime = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            var percentage = totalDuration.TotalSeconds > 0 ? (currentTime.TotalSeconds / totalDuration.TotalSeconds) * 100 : 0;
            
            var elapsed = DateTime.Now - startTime;
            var speed = speedMatch.Success ? double.Parse(speedMatch.Groups[1].Value) : 1.0;
            var remaining = speed > 0 ? TimeSpan.FromSeconds((totalDuration.TotalSeconds - currentTime.TotalSeconds) / speed) : TimeSpan.Zero;
            
            return new TranscodeProgress
            {
                Percentage = Math.Min(100, percentage),
                Elapsed = elapsed,
                Remaining = remaining,
                Speed = speed
            };
        }
        
        return null;
    }
}
