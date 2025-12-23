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
/// FFmpeg-based transcoding engine with comprehensive codec support
/// Supports 40+ professional codecs across all categories
/// </summary>
public class FFmpegTranscodeEngine : ITranscodeEngine
{
    private readonly string _ffmpegPath;
    
    public FFmpegTranscodeEngine()
    {
        _ffmpegPath = FindFFmpegExecutable();
    }
    
    public List<TranscodePreset> GetAvailablePresets()
    {
        return new List<TranscodePreset>
        {
            // === SOUND CONVERSIONS ===
            new TranscodePreset
            {
                Id = "wav_pcm",
                Name = "WAV PCM (Uncompressed)",
                Category = "Sound",
                VideoCodec = "copy",
                AudioCodec = "pcm_s24le",
                Container = "wav",
                Description = "Uncompressed WAV audio"
            },
            new TranscodePreset
            {
                Id = "aiff_pcm",
                Name = "AIFF PCM",
                Category = "Sound",
                VideoCodec = "copy",
                AudioCodec = "pcm_s24be",
                Container = "aiff",
                Description = "Apple AIFF uncompressed audio"
            },
            new TranscodePreset
            {
                Id = "flac",
                Name = "FLAC (Lossless)",
                Category = "Sound",
                VideoCodec = "copy",
                AudioCodec = "flac",
                Container = "flac",
                Description = "Free Lossless Audio Codec"
            },
            new TranscodePreset
            {
                Id = "mp3_320",
                Name = "MP3 320kbps",
                Category = "Sound",
                VideoCodec = "copy",
                AudioCodec = "libmp3lame",
                AudioBitrate = "320k",
                Container = "mp3",
                Description = "High quality MP3"
            },
            new TranscodePreset
            {
                Id = "aac_256",
                Name = "AAC 256kbps",
                Category = "Sound",
                VideoCodec = "copy",
                AudioCodec = "aac",
                AudioBitrate = "256k",
                Container = "m4a",
                Description = "High quality AAC"
            },
            new TranscodePreset
            {
                Id = "ac3",
                Name = "AC3 (Dolby Digital)",
                Category = "Sound",
                VideoCodec = "copy",
                AudioCodec = "ac3",
                AudioBitrate = "640k",
                Container = "ac3",
                Description = "Dolby Digital AC3"
            },
            new TranscodePreset
            {
                Id = "opus",
                Name = "Opus 192kbps",
                Category = "Sound",
                VideoCodec = "copy",
                AudioCodec = "libopus",
                AudioBitrate = "192k",
                Container = "opus",
                Description = "Modern Opus codec"
            },
            new TranscodePreset
            {
                Id = "vorbis",
                Name = "Vorbis (OGG)",
                Category = "Sound",
                VideoCodec = "copy",
                AudioCodec = "libvorbis",
                AudioBitrate = "192k",
                Container = "ogg",
                Description = "Ogg Vorbis audio"
            },
            
            // === EDITING CODECS ===
            new TranscodePreset
            {
                Id = "dnxhd_1080p_175",
                Name = "DNxHD 1080p 175Mbps",
                Category = "Editing",
                VideoCodec = "dnxhd",
                VideoBitrate = "175M",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "Avid DNxHD for editing"
            },
            new TranscodePreset
            {
                Id = "dnxhr_hqx",
                Name = "DNxHR HQX",
                Category = "Editing",
                VideoCodec = "dnxhd",
                Profile = "dnxhr_hqx",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "Avid DNxHR HQX (10-bit)"
            },
            new TranscodePreset
            {
                Id = "prores_422",
                Name = "Apple ProRes 422",
                Category = "Editing",
                VideoCodec = "prores_ks",
                Profile = "2",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "Apple ProRes 422"
            },
            new TranscodePreset
            {
                Id = "prores_422hq",
                Name = "Apple ProRes 422 HQ",
                Category = "Editing",
                VideoCodec = "prores_ks",
                Profile = "3",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "Apple ProRes 422 HQ"
            },
            new TranscodePreset
            {
                Id = "prores_4444",
                Name = "Apple ProRes 4444",
                Category = "Editing",
                VideoCodec = "prores_ks",
                Profile = "4",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "Apple ProRes 4444 (Alpha support)"
            },
            new TranscodePreset
            {
                Id = "qt_animation",
                Name = "QT Animation (RLE)",
                Category = "Editing",
                VideoCodec = "qtrle",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "QuickTime Animation codec"
            },
            new TranscodePreset
            {
                Id = "cineform",
                Name = "GoPro CineForm",
                Category = "Editing",
                VideoCodec = "cfhd",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "GoPro CineForm intermediate"
            },
            new TranscodePreset
            {
                Id = "uncompressed_yuv422",
                Name = "Uncompressed YUV 4:2:2",
                Category = "Editing",
                VideoCodec = "v210",
                AudioCodec = "pcm_s24le",
                Container = "mov",
                Description = "Uncompressed 10-bit YUV 4:2:2"
            },
            
            // === OUTPUT CODECS ===
            new TranscodePreset
            {
                Id = "h264_high",
                Name = "H.264 High Quality",
                Category = "Output",
                VideoCodec = "libx264",
                Preset = "slow",
                Crf = "18",
                AudioCodec = "aac",
                AudioBitrate = "192k",
                Container = "mp4",
                Description = "H.264 high quality output"
            },
            new TranscodePreset
            {
                Id = "h265_high",
                Name = "H.265 (HEVC) High Quality",
                Category = "Output",
                VideoCodec = "libx265",
                Preset = "slow",
                Crf = "22",
                AudioCodec = "aac",
                AudioBitrate = "192k",
                Container = "mp4",
                Description = "H.265/HEVC high quality"
            },
            new TranscodePreset
            {
                Id = "vp8",
                Name = "VP8 (WebM)",
                Category = "Output",
                VideoCodec = "libvpx",
                VideoBitrate = "2M",
                AudioCodec = "libvorbis",
                AudioBitrate = "128k",
                Container = "webm",
                Description = "Google VP8 for web"
            },
            new TranscodePreset
            {
                Id = "vp9",
                Name = "VP9 (WebM)",
                Category = "Output",
                VideoCodec = "libvpx-vp9",
                Crf = "30",
                AudioCodec = "libopus",
                AudioBitrate = "128k",
                Container = "webm",
                Description = "Google VP9 for web"
            },
            new TranscodePreset
            {
                Id = "av1",
                Name = "AV1 (Next-gen)",
                Category = "Output",
                VideoCodec = "libaom-av1",
                Crf = "30",
                AudioCodec = "libopus",
                AudioBitrate = "128k",
                Container = "mp4",
                Description = "AV1 next-generation codec"
            },
            
            // === BROADCAST CODECS ===
            new TranscodePreset
            {
                Id = "xdcam_hd422",
                Name = "XDCAM HD422",
                Category = "Broadcast",
                VideoCodec = "mpeg2video",
                VideoBitrate = "50M",
                Profile = "422",
                AudioCodec = "pcm_s16le",
                Container = "mxf",
                Description = "Sony XDCAM HD422"
            },
            new TranscodePreset
            {
                Id = "avc_intra_100",
                Name = "AVC-Intra 100",
                Category = "Broadcast",
                VideoCodec = "libx264",
                VideoBitrate = "100M",
                Profile = "high422",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "Panasonic AVC-Intra 100"
            },
            new TranscodePreset
            {
                Id = "xavc",
                Name = "XAVC (Sony)",
                Category = "Broadcast",
                VideoCodec = "libx264",
                VideoBitrate = "100M",
                Profile = "high422",
                AudioCodec = "pcm_s24le",
                Container = "mxf",
                Description = "Sony XAVC broadcast"
            },
            new TranscodePreset
            {
                Id = "hap",
                Name = "HAP (Vidvox)",
                Category = "Broadcast",
                VideoCodec = "hap",
                AudioCodec = "pcm_s16le",
                Container = "mov",
                Description = "HAP codec for playback"
            },
            
            // === OLD/LEGACY CODECS ===
            new TranscodePreset
            {
                Id = "theora",
                Name = "Theora (OGG)",
                Category = "Legacy",
                VideoCodec = "libtheora",
                VideoBitrate = "2M",
                AudioCodec = "libvorbis",
                AudioBitrate = "128k",
                Container = "ogv",
                Description = "Theora video codec"
            },
            new TranscodePreset
            {
                Id = "mpeg2",
                Name = "MPEG-2",
                Category = "Legacy",
                VideoCodec = "mpeg2video",
                VideoBitrate = "8M",
                AudioCodec = "mp2",
                AudioBitrate = "192k",
                Container = "mpg",
                Description = "MPEG-2 video"
            },
            new TranscodePreset
            {
                Id = "mjpeg",
                Name = "Motion JPEG",
                Category = "Legacy",
                VideoCodec = "mjpeg",
                Quality = "2",
                AudioCodec = "pcm_s16le",
                Container = "avi",
                Description = "Motion JPEG"
            },
            new TranscodePreset
            {
                Id = "xvid",
                Name = "Xvid (MPEG-4)",
                Category = "Legacy",
                VideoCodec = "libxvid",
                Quality = "4",
                AudioCodec = "libmp3lame",
                AudioBitrate = "128k",
                Container = "avi",
                Description = "Xvid MPEG-4"
            },
            new TranscodePreset
            {
                Id = "dv",
                Name = "DV (Digital Video)",
                Category = "Legacy",
                VideoCodec = "dvvideo",
                AudioCodec = "pcm_s16le",
                Container = "dv",
                Description = "DV digital video"
            },
            new TranscodePreset
            {
                Id = "wmv",
                Name = "WMV (Windows Media)",
                Category = "Legacy",
                VideoCodec = "wmv2",
                VideoBitrate = "2M",
                AudioCodec = "wmav2",
                AudioBitrate = "128k",
                Container = "wmv",
                Description = "Windows Media Video"
            },
            new TranscodePreset
            {
                Id = "mpeg1",
                Name = "MPEG-1",
                Category = "Legacy",
                VideoCodec = "mpeg1video",
                VideoBitrate = "1.5M",
                AudioCodec = "mp2",
                AudioBitrate = "128k",
                Container = "mpg",
                Description = "MPEG-1 video"
            }
        };
    }
    
    public async Task<TranscodeResult> TranscodeAsync(
        string inputPath,
        string outputPath,
        TranscodePreset preset,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new TranscodeResult();
        var startTime = DateTime.Now;
        
        try
        {
            // Build FFmpeg command
            var arguments = BuildFFmpegArguments(inputPath, outputPath, preset);
            
            var processInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = new Process { StartInfo = processInfo };
            
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();
            
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };
            
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    
                    // Parse progress from FFmpeg output
                    var match = Regex.Match(e.Data, @"time=(\d+):(\d+):(\d+\.\d+)");
                    if (match.Success)
                    {
                        var hours = int.Parse(match.Groups[1].Value);
                        var minutes = int.Parse(match.Groups[2].Value);
                        var seconds = double.Parse(match.Groups[3].Value);
                        var currentTime = hours * 3600 + minutes * 60 + seconds;
                        
                        // Report progress (simplified - would need duration for accurate %)
                        progress?.Report(currentTime);
                    }
                }
            };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync(cancellationToken);
            
            result.Success = process.ExitCode == 0;
            result.OutputPath = outputPath;
            result.Duration = DateTime.Now - startTime;
            
            if (!result.Success)
            {
                result.ErrorMessage = errorBuilder.ToString();
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }
        
        return result;
    }
    
    private string BuildFFmpegArguments(string inputPath, string outputPath, TranscodePreset preset)
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
        }
        
        // Video bitrate
        if (!string.IsNullOrEmpty(preset.VideoBitrate))
        {
            args.Add("-b:v");
            args.Add(preset.VideoBitrate);
        }
        
        // CRF (Constant Rate Factor)
        if (!string.IsNullOrEmpty(preset.Crf))
        {
            args.Add("-crf");
            args.Add(preset.Crf);
        }
        
        // Preset (encoding speed)
        if (!string.IsNullOrEmpty(preset.Preset))
        {
            args.Add("-preset");
            args.Add(preset.Preset);
        }
        
        // Profile
        if (!string.IsNullOrEmpty(preset.Profile))
        {
            args.Add("-profile:v");
            args.Add(preset.Profile);
        }
        
        // Quality
        if (!string.IsNullOrEmpty(preset.Quality))
        {
            args.Add("-q:v");
            args.Add(preset.Quality);
        }
        
        // Audio codec
        if (!string.IsNullOrEmpty(preset.AudioCodec))
        {
            args.Add("-c:a");
            args.Add(preset.AudioCodec);
        }
        
        // Audio bitrate
        if (!string.IsNullOrEmpty(preset.AudioBitrate))
        {
            args.Add("-b:a");
            args.Add(preset.AudioBitrate);
        }
        
        // Output file
        args.Add($"\"{outputPath}\"");
        
        return string.Join(" ", args);
    }
    
    private string FindFFmpegExecutable()
    {
        // Try common locations
        var possiblePaths = new[]
        {
            "ffmpeg",
            "ffmpeg.exe",
            @"C:\ffmpeg\bin\ffmpeg.exe",
            @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
            "/usr/bin/ffmpeg",
            "/usr/local/bin/ffmpeg"
        };
        
        foreach (var path in possiblePaths)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                        return path;
                }
            }
            catch
            {
                continue;
            }
        }
        
        return "ffmpeg"; // Fallback to PATH
    }
}
