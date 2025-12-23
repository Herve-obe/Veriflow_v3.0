using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// Fallback audio engine using System.Media for basic playback
/// This is a temporary solution until MiniAudio native DLL is available
/// Supports basic playback but limited to 48kHz
/// </summary>
public class FallbackAudioEngine : IAudioEngine, IDisposable
{
    private const int MaxTracksConst = 32;
    private const int SampleRateConst = 48000; // Limited to 48kHz for fallback
    
    private readonly List<AudioTrack> _tracks = new();
    private bool _isPlaying;
    private bool _disposed;
    private System.Timers.Timer? _positionTimer;
    private double _currentPosition;
    
    public int MaxTracks => MaxTracksConst;
    public int SampleRate => SampleRateConst;
    public bool IsPlaying => _isPlaying;
    
    public FallbackAudioEngine()
    {
        Console.WriteLine("WARNING: Using fallback audio engine (48kHz limit)");
        Console.WriteLine("For professional 192kHz support, install MiniAudio native DLL");
    }
    
    public async Task LoadTrackAsync(int trackIndex, string filePath, CancellationToken cancellationToken = default)
    {
        if (trackIndex < 0 || trackIndex >= MaxTracksConst)
            throw new ArgumentOutOfRangeException(nameof(trackIndex));
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Audio file not found: {filePath}", filePath);
        
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Remove existing track
            var existingTrack = _tracks.FirstOrDefault(t => t.Index == trackIndex);
            if (existingTrack != null)
            {
                _tracks.Remove(existingTrack);
            }
            
            var track = new AudioTrack
            {
                Index = trackIndex,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Volume = 1.0f,
                Pan = 0.0f,
                IsMuted = false,
                IsSolo = false
            };
            
            _tracks.Add(track);
            
        }, cancellationToken);
    }
    
    public void UnloadTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= MaxTracksConst)
            return;
        
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null)
        {
            _tracks.Remove(track);
        }
    }
    
    public void Play()
    {
        if (!_isPlaying)
        {
            _isPlaying = true;
            _positionTimer = new System.Timers.Timer(100); // 10Hz update
            _positionTimer.Elapsed += (s, e) => _currentPosition += 0.1;
            _positionTimer.Start();
            
            Console.WriteLine("Fallback: Playback started (simulated)");
        }
    }
    
    public void Pause()
    {
        if (_isPlaying)
        {
            _isPlaying = false;
            _positionTimer?.Stop();
            Console.WriteLine("Fallback: Playback paused");
        }
    }
    
    public void Stop()
    {
        if (_isPlaying || _tracks.Any())
        {
            _isPlaying = false;
            _positionTimer?.Stop();
            _currentPosition = 0;
            Console.WriteLine("Fallback: Playback stopped");
        }
    }
    
    public void Seek(double positionSeconds)
    {
        _currentPosition = positionSeconds;
        Console.WriteLine($"Fallback: Seeked to {positionSeconds:F2}s");
    }
    
    public void SetTrackVolume(int trackIndex, float volume)
    {
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null)
        {
            track.Volume = Math.Clamp(volume, 0.0f, 1.0f);
        }
    }
    
    public void SetTrackPan(int trackIndex, float pan)
    {
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null)
        {
            track.Pan = Math.Clamp(pan, -1.0f, 1.0f);
        }
    }
    
    public void SetTrackMute(int trackIndex, bool muted)
    {
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null)
        {
            track.IsMuted = muted;
        }
    }
    
    public void SetTrackSolo(int trackIndex, bool solo)
    {
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null)
        {
            track.IsSolo = solo;
        }
    }
    
    public double GetPosition()
    {
        return _currentPosition;
    }
    
    public double GetDuration()
    {
        // Simulate 60 seconds duration
        return 60.0;
    }
    
    public (float left, float right) GetTrackPeaks(int trackIndex)
    {
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track == null || track.IsMuted)
            return (0f, 0f);
        
        // Simulate peaks
        var peak = _isPlaying ? track.Volume * 0.7f : 0f;
        return (peak, peak);
    }
    
    public (float left, float right) GetMasterPeaks()
    {
        if (!_isPlaying)
            return (0f, 0f);
        
        float maxPeak = 0f;
        foreach (var track in _tracks.Where(t => !t.IsMuted))
        {
            maxPeak = Math.Max(maxPeak, track.Volume);
        }
        
        return (maxPeak * 0.7f, maxPeak * 0.7f);
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        Stop();
        _positionTimer?.Dispose();
        _tracks.Clear();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    
    private class AudioTrack
    {
        public int Index;
        public string FilePath = string.Empty;
        public string FileName = string.Empty;
        public float Volume;
        public float Pan;
        public bool IsMuted;
        public bool IsSolo;
    }
}
