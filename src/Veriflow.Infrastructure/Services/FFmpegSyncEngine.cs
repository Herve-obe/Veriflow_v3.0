using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// FFmpeg-based synchronization engine
/// Uses waveform correlation for audio/video sync
/// </summary>
public class FFmpegSyncEngine : ISyncEngine
{
    private readonly IMediaService _mediaService;
    
    public FFmpegSyncEngine(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }
    
    public async Task<SyncResult> SynchronizeAsync(
        string videoFilePath, 
        string audioFilePath, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract audio from video
            var videoAudioPath = await ExtractAudioFromVideoAsync(videoFilePath, cancellationToken);
            
            // Calculate offset using waveform correlation
            var offset = await CalculateAudioOffsetAsync(videoAudioPath, audioFilePath, cancellationToken);
            
            // Get metadata
            var videoInfo = await _mediaService.GetMediaInfoAsync(videoFilePath, cancellationToken);
            var audioInfo = await _mediaService.GetMediaInfoAsync(audioFilePath, cancellationToken);
            
            // Detect timecodes
            var videoTimecode = await DetectTimecodeAsync(videoFilePath, cancellationToken);
            
            // Calculate frame offset
            var frameRate = videoInfo.FrameRate ?? 25.0;
            var frameOffset = (int)(offset.TotalSeconds * frameRate);
            
            // Calculate confidence based on correlation strength
            var confidence = await CalculateConfidenceAsync(videoAudioPath, audioFilePath, offset, cancellationToken);
            
            // Cleanup temp file
            if (File.Exists(videoAudioPath))
                File.Delete(videoAudioPath);
            
            return new SyncResult
            {
                Success = true,
                Offset = offset,
                Confidence = confidence,
                VideoTimecode = videoTimecode,
                FrameOffset = frameOffset,
                FrameRate = frameRate
            };
        }
        catch (Exception ex)
        {
            return new SyncResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    public async Task<string?> DetectTimecodeAsync(
        string videoFilePath, 
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v quiet -select_streams v:0 -show_entries stream_tags=timecode -of default=noprint_wrappers=1:nokey=1 \"{videoFilePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                if (process == null) return null;
                
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                return string.IsNullOrWhiteSpace(output) ? null : output.Trim();
            }
            catch
            {
                return null;
            }
        }, cancellationToken);
    }
    
    public async Task<TimeSpan> CalculateAudioOffsetAsync(
        string audioFile1, 
        string audioFile2, 
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Extract waveform data from both files
                var waveform1 = ExtractWaveformData(audioFile1);
                var waveform2 = ExtractWaveformData(audioFile2);
                
                // Perform cross-correlation using FFT
                var offset = PerformCrossCorrelation(waveform1, waveform2);
                
                // Convert sample offset to time (assuming 48kHz)
                var sampleRate = 48000.0;
                var timeOffset = offset / sampleRate;
                
                return TimeSpan.FromSeconds(timeOffset);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }, cancellationToken);
    }
    
    public async Task<double> VerifySyncAccuracyAsync(
        string videoFilePath, 
        string audioFilePath, 
        TimeSpan offset, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var videoAudioPath = await ExtractAudioFromVideoAsync(videoFilePath, cancellationToken);
            var confidence = await CalculateConfidenceAsync(videoAudioPath, audioFilePath, offset, cancellationToken);
            
            if (File.Exists(videoAudioPath))
                File.Delete(videoAudioPath);
            
            return confidence;
        }
        catch
        {
            return 0.0;
        }
    }
    
    private async Task<string> ExtractAudioFromVideoAsync(string videoFilePath, CancellationToken cancellationToken)
    {
        var tempAudioPath = Path.Combine(Path.GetTempPath(), $"temp_audio_{Guid.NewGuid()}.wav");
        
        await Task.Run(() =>
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{videoFilePath}\" -vn -acodec pcm_s16le -ar 48000 -ac 1 \"{tempAudioPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }, cancellationToken);
        
        return tempAudioPath;
    }
    
    private float[] ExtractWaveformData(string audioFilePath)
    {
        // Simplified waveform extraction
        // In production, use NAudio or similar for proper audio reading
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{audioFilePath}\" -f f32le -acodec pcm_f32le -ar 8000 -ac 1 -",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(startInfo);
        if (process == null) return Array.Empty<float>();
        
        using var stream = process.StandardOutput.BaseStream;
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        
        var bytes = memoryStream.ToArray();
        var samples = new float[bytes.Length / 4];
        Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);
        
        // Limit to first 30 seconds for performance
        var maxSamples = 8000 * 30;
        return samples.Take(Math.Min(samples.Length, maxSamples)).ToArray();
    }
    
    private double PerformCrossCorrelation(float[] signal1, float[] signal2)
    {
        // Ensure signals are same length (pad shorter one)
        var maxLength = Math.Max(signal1.Length, signal2.Length);
        var paddedSignal1 = PadArray(signal1, maxLength);
        var paddedSignal2 = PadArray(signal2, maxLength);
        
        // Convert to complex for FFT
        var complex1 = paddedSignal1.Select(x => new System.Numerics.Complex(x, 0)).ToArray();
        var complex2 = paddedSignal2.Select(x => new System.Numerics.Complex(x, 0)).ToArray();
        
        // Perform FFT
        Fourier.Forward(complex1, FourierOptions.Matlab);
        Fourier.Forward(complex2, FourierOptions.Matlab);
        
        // Multiply in frequency domain (conjugate of second signal)
        var correlation = new System.Numerics.Complex[complex1.Length];
        for (int i = 0; i < complex1.Length; i++)
        {
            correlation[i] = complex1[i] * System.Numerics.Complex.Conjugate(complex2[i]);
        }
        
        // Inverse FFT to get correlation
        Fourier.Inverse(correlation, FourierOptions.Matlab);
        
        // Find peak (maximum correlation)
        var maxIndex = 0;
        var maxValue = 0.0;
        for (int i = 0; i < correlation.Length; i++)
        {
            var magnitude = correlation[i].Magnitude;
            if (magnitude > maxValue)
            {
                maxValue = magnitude;
                maxIndex = i;
            }
        }
        
        // Convert index to offset (handle wrap-around)
        var offset = maxIndex;
        if (offset > correlation.Length / 2)
            offset -= correlation.Length;
        
        return offset;
    }
    
    private float[] PadArray(float[] array, int targetLength)
    {
        if (array.Length >= targetLength)
            return array;
        
        var padded = new float[targetLength];
        Array.Copy(array, padded, array.Length);
        return padded;
    }
    
    private async Task<double> CalculateConfidenceAsync(
        string audioFile1, 
        string audioFile2, 
        TimeSpan offset, 
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Extract small segments around the offset
                var waveform1 = ExtractWaveformData(audioFile1);
                var waveform2 = ExtractWaveformData(audioFile2);
                
                // Calculate correlation coefficient
                var correlation = CalculateCorrelationCoefficient(waveform1, waveform2);
                
                // Convert to confidence percentage
                return Math.Max(0, Math.Min(100, correlation * 100));
            }
            catch
            {
                return 0.0;
            }
        }, cancellationToken);
    }
    
    private double CalculateCorrelationCoefficient(float[] signal1, float[] signal2)
    {
        var length = Math.Min(signal1.Length, signal2.Length);
        if (length == 0) return 0;
        
        var mean1 = signal1.Take(length).Average();
        var mean2 = signal2.Take(length).Average();
        
        var numerator = 0.0;
        var denominator1 = 0.0;
        var denominator2 = 0.0;
        
        for (int i = 0; i < length; i++)
        {
            var diff1 = signal1[i] - mean1;
            var diff2 = signal2[i] - mean2;
            
            numerator += diff1 * diff2;
            denominator1 += diff1 * diff1;
            denominator2 += diff2 * diff2;
        }
        
        var denominator = Math.Sqrt(denominator1 * denominator2);
        return denominator > 0 ? numerator / denominator : 0;
    }
}
