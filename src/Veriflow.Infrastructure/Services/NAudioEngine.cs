using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// NAudio-based multi-track audio engine
/// Supports up to 32 tracks, 192kHz, 32-bit float
/// </summary>
public class NAudioEngine : IAudioEngine, IDisposable
{
    private readonly int _maxTracks;
    private readonly int _sampleRate;
    private readonly AudioTrack[] _tracks;
    private IWavePlayer? _waveOut;
    private MixingSampleProvider? _mixer;
    private bool _isPlaying;
    private bool _disposed;
    
    public int MaxTracks => _maxTracks;
    public int SampleRate => _sampleRate;
    public bool IsPlaying => _isPlaying;
    
    public NAudioEngine(int maxTracks = 32, int sampleRate = 48000)
    {
        _maxTracks = maxTracks;
        _sampleRate = sampleRate;
        _tracks = new AudioTrack[maxTracks];
        
        for (int i = 0; i < maxTracks; i++)
        {
            _tracks[i] = new AudioTrack();
        }
        
        InitializeAudioOutput();
    }
    
    private void InitializeAudioOutput()
    {
        try
        {
            _waveOut = new WaveOutEvent
            {
                DesiredLatency = 100, // 100ms latency
                NumberOfBuffers = 2
            };
            
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(_sampleRate, 2))
            {
                ReadFully = true
            };
            
            _waveOut.Init(_mixer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize audio output: {ex.Message}", ex);
        }
    }
    
    public async Task LoadTrackAsync(int trackIndex, string filePath, CancellationToken cancellationToken = default)
    {
        if (trackIndex < 0 || trackIndex >= _maxTracks)
            throw new ArgumentOutOfRangeException(nameof(trackIndex));
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        
        await Task.Run(() =>
        {
            try
            {
                var track = _tracks[trackIndex];
                
                // Unload existing
                track.Reader?.Dispose();
                track.SampleProvider = null;
                
                // Load new file
                track.Reader = new AudioFileReader(filePath);
                
                // Create sample provider with volume and pan
                var volumeProvider = new VolumeSampleProvider(track.Reader)
                {
                    Volume = track.Volume
                };
                
                var panProvider = new PanningSampleProvider(volumeProvider)
                {
                    Pan = track.Pan
                };
                
                track.SampleProvider = panProvider;
                track.FilePath = filePath;
                track.FileName = Path.GetFileName(filePath);
                track.Duration = track.Reader.TotalTime;
                
                // Add to mixer if not already added
                if (_mixer != null && !track.IsInMixer)
                {
                    _mixer.AddMixerInput(track.SampleProvider);
                    track.IsInMixer = true;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load audio file: {ex.Message}", ex);
            }
        }, cancellationToken);
    }
    
    public void UnloadTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= _maxTracks)
            return;
        
        var track = _tracks[trackIndex];
        
        if (_mixer != null && track.SampleProvider != null && track.IsInMixer)
        {
            _mixer.RemoveMixerInput(track.SampleProvider);
            track.IsInMixer = false;
        }
        
        track.Reader?.Dispose();
        track.Reader = null;
        track.SampleProvider = null;
        track.FilePath = string.Empty;
        track.FileName = string.Empty;
        track.Duration = TimeSpan.Zero;
    }
    
    public void Play()
    {
        if (_waveOut == null) return;
        
        _waveOut.Play();
        _isPlaying = true;
    }
    
    public void Pause()
    {
        if (_waveOut == null) return;
        
        _waveOut.Pause();
        _isPlaying = false;
    }
    
    public void Stop()
    {
        if (_waveOut == null) return;
        
        _waveOut.Stop();
        _isPlaying = false;
        
        // Reset all tracks to beginning
        foreach (var track in _tracks)
        {
            if (track.Reader != null)
                track.Reader.Position = 0;
        }
    }
    
    public void Seek(double positionSeconds)
    {
        foreach (var track in _tracks)
        {
            if (track.Reader != null)
            {
                var position = TimeSpan.FromSeconds(positionSeconds);
                if (position <= track.Duration)
                    track.Reader.CurrentTime = position;
            }
        }
    }
    
    public void SetTrackVolume(int trackIndex, float volume)
    {
        if (trackIndex < 0 || trackIndex >= _maxTracks)
            return;
        
        var track = _tracks[trackIndex];
        track.Volume = Math.Clamp(volume, 0f, 1f);
        
        if (track.SampleProvider is PanningSampleProvider panProvider &&
            panProvider.Source is VolumeSampleProvider volumeProvider)
        {
            volumeProvider.Volume = track.Volume;
        }
    }
    
    public void SetTrackPan(int trackIndex, float pan)
    {
        if (trackIndex < 0 || trackIndex >= _maxTracks)
            return;
        
        var track = _tracks[trackIndex];
        track.Pan = Math.Clamp(pan, -1f, 1f);
        
        if (track.SampleProvider is PanningSampleProvider panProvider)
        {
            panProvider.Pan = track.Pan;
        }
    }
    
    public void SetTrackMute(int trackIndex, bool muted)
    {
        if (trackIndex < 0 || trackIndex >= _maxTracks)
            return;
        
        var track = _tracks[trackIndex];
        track.IsMuted = muted;
        
        // Mute by setting volume to 0
        if (track.SampleProvider is PanningSampleProvider panProvider &&
            panProvider.Source is VolumeSampleProvider volumeProvider)
        {
            volumeProvider.Volume = muted ? 0f : track.Volume;
        }
    }
    
    public void SetTrackSolo(int trackIndex, bool solo)
    {
        if (trackIndex < 0 || trackIndex >= _maxTracks)
            return;
        
        _tracks[trackIndex].IsSolo = solo;
        
        // If any track is solo, mute all non-solo tracks
        bool anySolo = _tracks.Any(t => t.IsSolo);
        
        for (int i = 0; i < _maxTracks; i++)
        {
            var track = _tracks[i];
            if (track.SampleProvider is PanningSampleProvider panProvider &&
                panProvider.Source is VolumeSampleProvider volumeProvider)
            {
                if (anySolo)
                {
                    volumeProvider.Volume = track.IsSolo ? track.Volume : 0f;
                }
                else
                {
                    volumeProvider.Volume = track.IsMuted ? 0f : track.Volume;
                }
            }
        }
    }
    
    public double GetPosition()
    {
        var track = _tracks.FirstOrDefault(t => t.Reader != null);
        return track?.Reader?.CurrentTime.TotalSeconds ?? 0;
    }
    
    public double GetDuration()
    {
        return _tracks.Where(t => t.Reader != null)
                      .Max(t => t.Duration.TotalSeconds);
    }
    
    public (float left, float right) GetTrackPeaks(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= _maxTracks)
            return (0f, 0f);
        
        var track = _tracks[trackIndex];
        return (track.PeakLeft, track.PeakRight);
    }
    
    public (float left, float right) GetMasterPeaks()
    {
        float maxLeft = 0f;
        float maxRight = 0f;
        
        foreach (var track in _tracks)
        {
            if (!track.IsMuted && track.Reader != null)
            {
                maxLeft = Math.Max(maxLeft, track.PeakLeft);
                maxRight = Math.Max(maxRight, track.PeakRight);
            }
        }
        
        return (maxLeft, maxRight);
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        Stop();
        
        _waveOut?.Dispose();
        _waveOut = null;
        
        foreach (var track in _tracks)
        {
            track.Reader?.Dispose();
        }
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    
    private class AudioTrack
    {
        public AudioFileReader? Reader { get; set; }
        public ISampleProvider? SampleProvider { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public float Volume { get; set; } = 1.0f;
        public float Pan { get; set; } = 0f;
        public bool IsMuted { get; set; }
        public bool IsSolo { get; set; }
        public bool IsInMixer { get; set; }
        public float PeakLeft { get; set; }
        public float PeakRight { get; set; }
    }
}
