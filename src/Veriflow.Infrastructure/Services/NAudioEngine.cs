using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// NAudio-based audio engine with 192kHz support
/// </summary>
public class NAudioEngine : IAudioEngine, IDisposable
{
    private const int MaxTracksConst = 32;
    private const int SampleRateConst = 192000;
    
    private readonly List<AudioTrack> _tracks = new();
    private IWavePlayer? _waveOut;
    private MixingSampleProvider? _mixer;
    private bool _isPlaying;
    private bool _disposed;
    private CancellationTokenSource? _updateCancellation;
    
    public int MaxTracks => MaxTracksConst;
    public int SampleRate => SampleRateConst;
    public bool IsPlaying => _isPlaying;
    
    public NAudioEngine()
    {
        InitializeAudioOutput();
    }
    
    private void InitializeAudioOutput()
    {
        _waveOut = new WaveOutEvent
        {
            DesiredLatency = 100,
            NumberOfBuffers = 2
        };
        
        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRateConst, 2))
        {
            ReadFully = true
        };
        
        _waveOut.Init(_mixer);
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
            
            UnloadTrack(trackIndex);
            
            var reader = new AudioFileReader(filePath);
            var resampler = new MediaFoundationResampler(reader, WaveFormat.CreateIeeeFloatWaveFormat(SampleRateConst, 2));
            var sampleProvider = resampler.ToSampleProvider();
            
            var track = new AudioTrack
            {
                Index = trackIndex,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Reader = reader,
                Resampler = resampler,
                SampleProvider = sampleProvider,
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
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null)
        {
            track.Resampler?.Dispose();
            track.Reader?.Dispose();
            _tracks.Remove(track);
        }
    }
    
    public void Play()
    {
        if (!_isPlaying && _waveOut != null)
        {
            _waveOut.Play();
            _isPlaying = true;
            StartVuMeterUpdates();
        }
    }
    
    public void Pause()
    {
        if (_isPlaying && _waveOut != null)
        {
            _waveOut.Pause();
            _isPlaying = false;
            StopVuMeterUpdates();
        }
    }
    
    public void Stop()
    {
        if (_waveOut != null)
        {
            _waveOut.Stop();
            _isPlaying = false;
            StopVuMeterUpdates();
            
            foreach (var track in _tracks)
            {
                track.Reader?.Seek(0, SeekOrigin.Begin);
            }
        }
    }
    
    public void Seek(double positionSeconds)
    {
        foreach (var track in _tracks)
        {
            if (track.Reader != null)
            {
                var position = (long)(positionSeconds * track.Reader.WaveFormat.AverageBytesPerSecond);
                track.Reader.Position = Math.Max(0, Math.Min(position, track.Reader.Length));
            }
        }
    }
    
    public void SetTrackVolume(int trackIndex, float volume)
    {
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null)
        {
            track.Volume = Math.Clamp(volume, 0.0f, 1.0f);
            if (track.Reader != null)
            {
                track.Reader.Volume = track.Volume;
            }
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
        var firstTrack = _tracks.FirstOrDefault();
        if (firstTrack?.Reader != null)
        {
            return (double)firstTrack.Reader.Position / firstTrack.Reader.WaveFormat.AverageBytesPerSecond;
        }
        return 0.0;
    }
    
    public double GetDuration()
    {
        var firstTrack = _tracks.FirstOrDefault();
        if (firstTrack?.Reader != null)
        {
            return (double)firstTrack.Reader.Length / firstTrack.Reader.WaveFormat.AverageBytesPerSecond;
        }
        return 0.0;
    }
    
    public (float left, float right) GetTrackPeaks(int trackIndex)
    {
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        return track == null ? (0f, 0f) : (track.PeakLeft, track.PeakRight);
    }
    
    public (float left, float right) GetMasterPeaks()
    {
        float maxLeft = 0f, maxRight = 0f;
        
        foreach (var track in _tracks.Where(t => !t.IsMuted))
        {
            maxLeft = Math.Max(maxLeft, track.PeakLeft);
            maxRight = Math.Max(maxRight, track.PeakRight);
        }
        
        return (maxLeft, maxRight);
    }
    
    private void StartVuMeterUpdates()
    {
        _updateCancellation = new CancellationTokenSource();
        
        _ = Task.Run(async () =>
        {
            while (!_updateCancellation.Token.IsCancellationRequested)
            {
                UpdateVuMeters();
                await Task.Delay(50, _updateCancellation.Token);
            }
        }, _updateCancellation.Token);
    }
    
    private void StopVuMeterUpdates()
    {
        _updateCancellation?.Cancel();
        _updateCancellation?.Dispose();
        _updateCancellation = null;
    }
    
    private void UpdateVuMeters()
    {
        foreach (var track in _tracks.Where(t => !t.IsMuted))
        {
            if (_isPlaying && track.Reader != null)
            {
                track.PeakLeft = track.Volume * 0.7f;
                track.PeakRight = track.Volume * 0.7f;
            }
            else
            {
                track.PeakLeft = 0f;
                track.PeakRight = 0f;
            }
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        Stop();
        StopVuMeterUpdates();
        
        foreach (var track in _tracks)
        {
            track.Resampler?.Dispose();
            track.Reader?.Dispose();
        }
        _tracks.Clear();
        
        _waveOut?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    
    private class AudioTrack
    {
        public int Index { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public AudioFileReader? Reader { get; set; }
        public MediaFoundationResampler? Resampler { get; set; }
        public ISampleProvider? SampleProvider { get; set; }
        public float Volume { get; set; }
        public float Pan { get; set; }
        public bool IsMuted { get; set; }
        public bool IsSolo { get; set; }
        public float PeakLeft { get; set; }
        public float PeakRight { get; set; }
    }
}
