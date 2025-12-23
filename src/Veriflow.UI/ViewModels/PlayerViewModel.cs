using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Models;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for PLAYER page (F3)
/// Professional audio/video playback with metadata
/// </summary>
public partial class PlayerViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _loadedFile = string.Empty;
    
    [ObservableProperty]
    private bool _isPlaying;
    
    [ObservableProperty]
    private double _position;
    
    [ObservableProperty]
    private double _duration;
    
    [ObservableProperty]
    private ObservableCollection<AudioTrack> _audioTracks = new();
    
    [ObservableProperty]
    private string _timecode = "00:00:00:00";
    
    public PlayerViewModel()
    {
        StatusMessage = "No file loaded";
    }
    
    [RelayCommand]
    private async Task OpenFileAsync()
    {
        // TODO: Implement file picker
        StatusMessage = "Select file to play";
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private void Play()
    {
        IsPlaying = true;
        StatusMessage = "Playing";
        // TODO: Implement playback
    }
    
    [RelayCommand]
    private void Pause()
    {
        IsPlaying = false;
        StatusMessage = "Paused";
        // TODO: Implement pause
    }
    
    [RelayCommand]
    private void Stop()
    {
        IsPlaying = false;
        Position = 0;
        StatusMessage = "Stopped";
        // TODO: Implement stop
    }
    
    [RelayCommand]
    private void Seek(double position)
    {
        Position = position;
        // TODO: Implement seek
    }
}
