using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Veriflow.Core.Interfaces;
using Veriflow.Infrastructure.Audio;
using static Veriflow.Infrastructure.Audio.MiniAudioNative;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// Professional MiniAudio-based audio engine
/// Native implementation supporting 192kHz, 32-bit float, 32 tracks
/// Public Domain license - completely free for commercial use
/// </summary>
public class MiniAudioEngine : IAudioEngine, IDisposable
{
    private const int MaxTracksConst = 32;
    private const int SampleRateConst = 192000; // 192kHz professional audio
    
    private readonly List<AudioTrack> _tracks = new();
    private ma_engine _engine;
    private bool _engineInitialized;
    private bool _isPlaying;
    private bool _disposed;
    private CancellationTokenSource? _updateCancellation;
    
    public int MaxTracks => MaxTracksConst;
    public int SampleRate => SampleRateConst;
    public bool IsPlaying => _isPlaying;
    
    public MiniAudioEngine()
    {
        InitializeEngine();
    }
    
    private void InitializeEngine()
    {
        try
        {
            // Initialize MiniAudio engine with professional settings
            var config = ma_engine_config_init();
            config.channels = 2; // Stereo
            config.sampleRate = (uint)SampleRateConst; // 192kHz
            config.noAutoStart = 0; // Auto-start enabled
            
            var result = ma_engine_init(ref config, out _engine);
            
            if (result != ma_result.MA_SUCCESS)
            {
                throw new InvalidOperationException($"Failed to initialize MiniAudio engine: {result}");
            }
            
            _engineInitialized = true;
        }
        catch (DllNotFoundException)
        {
            throw new InvalidOperationException(
                "MiniAudio native library not found. Please ensure miniaudio.dll is in the application directory or system PATH.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize MiniAudio engine: {ex.Message}", ex);
        }
    }
    
    public async Task LoadTrackAsync(int trackIndex, string filePath, CancellationToken cancellationToken = default)
    {
        if (trackIndex < 0 || trackIndex >= MaxTracksConst)
            throw new ArgumentOutOfRangeException(nameof(trackIndex));
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Audio file not found: {filePath}", filePath);
        
        if (!_engineInitialized)
            throw new InvalidOperationException("Engine not initialized");
        
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Remove existing track if present
            var existingTrack = _tracks.FirstOrDefault(t => t.Index == trackIndex);
            if (existingTrack != null)
            {
                if (existingTrack.SoundInitialized)
                {
                    ma_sound_uninit(ref existingTrack.Sound);
                }
                _tracks.Remove(existingTrack);
            }
            
            // Load new sound with streaming flag for large files
            ma_sound sound;
            var flags = MA_SOUND_FLAG_STREAM | MA_SOUND_FLAG_DECODE;
            var result = ma_sound_init_from_file(
                ref _engine,
                filePath,
                flags,
                IntPtr.Zero,
                IntPtr.Zero,
                out sound);
            
            if (result != ma_result.MA_SUCCESS)
            {
                throw new InvalidOperationException($"Failed to load audio file '{filePath}': {result}");
            }
            
            var track = new AudioTrack
            {
                Index = trackIndex,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Sound = sound,
                SoundInitialized = true,
                Volume = 1.0f,
                Pan = 0.0f,
                IsMuted = false,
                IsSolo = false,
                PeakLeft = 0.0f,
                PeakRight = 0.0f
            };
            
            // Set initial volume and pan
            ma_sound_set_volume(ref track.Sound, track.Volume);
            ma_sound_set_pan(ref track.Sound, track.Pan);
            
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
            if (track.SoundInitialized)
            {
                ma_sound_stop(ref track.Sound);
                ma_sound_uninit(ref track.Sound);
            }
            _tracks.Remove(track);
        }
    }
    
    public void Play()
    {
        if (!_engineInitialized)
            throw new InvalidOperationException("Engine not initialized");
        
        if (!_isPlaying)
        {
            // Start engine
            ma_engine_start(ref _engine);
            
            // Start all non-muted tracks
            foreach (var track in _tracks.Where(t => !t.IsMuted && t.SoundInitialized))
            {
                ma_sound_start(ref track.Sound);
            }
            
            _isPlaying = true;
            StartVuMeterUpdates();
        }
    }
    
    public void Pause()
    {
        if (_isPlaying)
        {
            foreach (var track in _tracks.Where(t => t.SoundInitialized))
            {
                ma_sound_stop(ref track.Sound);
            }
            
            _isPlaying = false;
            StopVuMeterUpdates();
        }
    }
    
    public void Stop()
    {
        if (_isPlaying || _tracks.Any())
        {
            foreach (var track in _tracks.Where(t => t.SoundInitialized))
            {
                ma_sound_stop(ref track.Sound);
                ma_sound_seek_to_pcm_frame(ref track.Sound, 0);
            }
            
            if (_engineInitialized)
            {
                ma_engine_stop(ref _engine);
            }
            
            _isPlaying = false;
            StopVuMeterUpdates();
        }
    }
    
    public void Seek(double positionSeconds)
    {
        var pcmFrame = (ulong)(positionSeconds * SampleRateConst);
        
        foreach (var track in _tracks.Where(t => t.SoundInitialized))
        {
            ma_sound_seek_to_pcm_frame(ref track.Sound, pcmFrame);
        }
    }
    
    public void SetTrackVolume(int trackIndex, float volume)
    {
        if (trackIndex < 0 || trackIndex >= MaxTracksConst)
            return;
        
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null && track.SoundInitialized)
        {
            volume = Math.Clamp(volume, 0.0f, 1.0f);
            track.Volume = volume;
            ma_sound_set_volume(ref track.Sound, volume);
        }
    }
    
    public void SetTrackPan(int trackIndex, float pan)
    {
        if (trackIndex < 0 || trackIndex >= MaxTracksConst)
            return;
        
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null && track.SoundInitialized)
        {
            pan = Math.Clamp(pan, -1.0f, 1.0f);
            track.Pan = pan;
            ma_sound_set_pan(ref track.Sound, pan);
        }
    }
    
    public void SetTrackMute(int trackIndex, bool muted)
    {
        if (trackIndex < 0 || trackIndex >= MaxTracksConst)
            return;
        
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null && track.SoundInitialized)
        {
            track.IsMuted = muted;
            
            if (muted && _isPlaying)
            {
                ma_sound_stop(ref track.Sound);
            }
            else if (!muted && _isPlaying)
            {
                ma_sound_start(ref track.Sound);
            }
        }
    }
    
    public void SetTrackSolo(int trackIndex, bool solo)
    {
        if (trackIndex < 0 || trackIndex >= MaxTracksConst)
            return;
        
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track != null)
        {
            track.IsSolo = solo;
            
            // If any track is soloed, mute all non-solo tracks
            var hasSolo = _tracks.Any(t => t.IsSolo);
            
            foreach (var t in _tracks.Where(tr => tr.SoundInitialized))
            {
                var shouldPlay = !hasSolo || t.IsSolo;
                
                if (_isPlaying)
                {
                    if (shouldPlay && !t.IsMuted)
                        ma_sound_start(ref t.Sound);
                    else
                        ma_sound_stop(ref t.Sound);
                }
            }
        }
    }
    
    public double GetPosition()
    {
        var firstTrack = _tracks.FirstOrDefault(t => t.SoundInitialized);
        if (firstTrack == null)
            return 0.0;
        
        var result = ma_sound_get_cursor_in_pcm_frames(ref firstTrack.Sound, out ulong cursor);
        if (result != ma_result.MA_SUCCESS)
            return 0.0;
        
        return (double)cursor / SampleRateConst;
    }
    
    public double GetDuration()
    {
        var firstTrack = _tracks.FirstOrDefault(t => t.SoundInitialized);
        if (firstTrack == null)
            return 0.0;
        
        var result = ma_sound_get_length_in_pcm_frames(ref firstTrack.Sound, out ulong length);
        if (result != ma_result.MA_SUCCESS)
            return 0.0;
        
        return (double)length / SampleRateConst;
    }
    
    public (float left, float right) GetTrackPeaks(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= MaxTracksConst)
            return (0f, 0f);
        
        var track = _tracks.FirstOrDefault(t => t.Index == trackIndex);
        if (track == null)
            return (0f, 0f);
        
        return (track.PeakLeft, track.PeakRight);
    }
    
    public (float left, float right) GetMasterPeaks()
    {
        float maxLeft = 0f;
        float maxRight = 0f;
        
        foreach (var track in _tracks.Where(t => !t.IsMuted && t.SoundInitialized))
        {
            maxLeft = Math.Max(maxLeft, track.PeakLeft);
            maxRight = Math.Max(maxRight, track.PeakRight);
        }
        
        return (maxLeft, maxRight);
    }
    
    private void StartVuMeterUpdates()
    {
        _updateCancellation = new CancellationTokenSource();
        
        Task.Run(async () =>
        {
            while (!_updateCancellation.Token.IsCancellationRequested)
            {
                UpdateVuMeters();
                await Task.Delay(50, _updateCancellation.Token); // 20Hz update rate
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
        foreach (var track in _tracks.Where(t => t.SoundInitialized))
        {
            if (!track.IsMuted && ma_sound_is_playing(ref track.Sound) != 0)
            {
                // Get current volume as peak approximation
                var volume = ma_sound_get_volume(ref track.Sound);
                track.PeakLeft = volume * 0.8f; // Simulate peak
                track.PeakRight = volume * 0.8f;
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
        if (_disposed)
            return;
        
        Stop();
        StopVuMeterUpdates();
        
        foreach (var track in _tracks.Where(t => t.SoundInitialized))
        {
            ma_sound_uninit(ref track.Sound);
        }
        _tracks.Clear();
        
        if (_engineInitialized)
        {
            ma_engine_uninit(ref _engine);
            _engineInitialized = false;
        }
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    
    private class AudioTrack
    {
        public int Index;
        public string FilePath = string.Empty;
        public string FileName = string.Empty;
        public ma_sound Sound; // Field instead of property for ref passing
        public bool SoundInitialized;
        public float Volume;
        public float Pan;
        public bool IsMuted;
        public bool IsSolo;
        public float PeakLeft;
        public float PeakRight;
    }
}
