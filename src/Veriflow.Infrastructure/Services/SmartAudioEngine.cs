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
/// Smart audio engine that uses MiniAudio if available, falls back to basic engine otherwise
/// </summary>
public class SmartAudioEngine : IAudioEngine, IDisposable
{
    private readonly IAudioEngine _engine;
    private readonly bool _usingMiniAudio;
    
    public int MaxTracks => _engine.MaxTracks;
    public int SampleRate => _engine.SampleRate;
    public bool IsPlaying => _engine.IsPlaying;
    
    public SmartAudioEngine()
    {
        // Try to initialize MiniAudio
        try
        {
            _engine = new MiniAudioEngine();
            _usingMiniAudio = true;
            Console.WriteLine("✅ MiniAudio engine initialized successfully (192kHz support)");
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("⚠️  MiniAudio DLL not found, using fallback engine (48kHz limit)");
            Console.WriteLine("   For professional 192kHz support, run: .\\build_miniaudio.bat");
            _engine = new FallbackAudioEngine();
            _usingMiniAudio = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  MiniAudio initialization failed: {ex.Message}");
            Console.WriteLine("   Using fallback engine (48kHz limit)");
            _engine = new FallbackAudioEngine();
            _usingMiniAudio = false;
        }
    }
    
    public Task LoadTrackAsync(int trackIndex, string filePath, CancellationToken cancellationToken = default)
        => _engine.LoadTrackAsync(trackIndex, filePath, cancellationToken);
    
    public void UnloadTrack(int trackIndex)
        => _engine.UnloadTrack(trackIndex);
    
    public void Play()
        => _engine.Play();
    
    public void Pause()
        => _engine.Pause();
    
    public void Stop()
        => _engine.Stop();
    
    public void Seek(double positionSeconds)
        => _engine.Seek(positionSeconds);
    
    public void SetTrackVolume(int trackIndex, float volume)
        => _engine.SetTrackVolume(trackIndex, volume);
    
    public void SetTrackPan(int trackIndex, float pan)
        => _engine.SetTrackPan(trackIndex, pan);
    
    public void SetTrackMute(int trackIndex, bool muted)
        => _engine.SetTrackMute(trackIndex, muted);
    
    public void SetTrackSolo(int trackIndex, bool solo)
        => _engine.SetTrackSolo(trackIndex, solo);
    
    public double GetPosition()
        => _engine.GetPosition();
    
    public double GetDuration()
        => _engine.GetDuration();
    
    public (float left, float right) GetTrackPeaks(int trackIndex)
        => _engine.GetTrackPeaks(trackIndex);
    
    public (float left, float right) GetMasterPeaks()
        => _engine.GetMasterPeaks();
    
    public void Dispose()
    {
        _engine?.Dispose();
        GC.SuppressFinalize(this);
    }
}
