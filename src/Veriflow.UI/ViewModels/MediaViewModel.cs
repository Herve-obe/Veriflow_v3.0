using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;
using Veriflow.UI.Messages;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for MEDIA page (F2)
/// File browser and media library management
/// </summary>
public partial class MediaViewModel : ViewModelBase
{
    private readonly IMediaService _mediaService;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _cancellationTokenSource;
    
    [ObservableProperty]
    private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    
    [ObservableProperty]
    private ObservableCollection<MediaFile> _mediaFiles = new();
    
    [ObservableProperty]
    private MediaFile? _selectedMedia;
    
    [ObservableProperty]
    private string _searchQuery = string.Empty;
    
    [ObservableProperty]
    private bool _isGridView = true;
    
    [ObservableProperty]
    private bool _showVideo = true;
    
    [ObservableProperty]
    private bool _showAudio = true;
    
    [ObservableProperty]
    private bool _isDragging = false;
    
    [ObservableProperty]
    private int _viewMode = 0; // 0=Grid, 1=List, 2=Filmstrip
    
    // Filtered media files based on Video/Audio toggle
    public ObservableCollection<MediaFile> FilteredMediaFiles
    {
        get
        {
            var filtered = new ObservableCollection<MediaFile>();
            foreach (var file in MediaFiles)
            {
                if ((ShowVideo && file.IsVideo) || (ShowAudio && file.IsAudio))
                {
                    filtered.Add(file);
                }
            }
            return filtered;
        }
    }
    
    public MediaViewModel(IMediaService mediaService, IDialogService dialogService)
    {
        _mediaService = mediaService;
        _dialogService = dialogService;
        StatusMessage = "Browse media files";
        
        // Load initial directory
        _ = LoadMediaFilesAsync();
    }
    
    [RelayCommand]
    private async Task NavigateToFolderAsync(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        
        if (Directory.Exists(path))
        {
            CurrentPath = path;
            await LoadMediaFilesAsync();
        }
    }
    
    [RelayCommand]
    private async Task BrowseFolderAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Media Folder");
        if (!string.IsNullOrEmpty(folder))
        {
            CurrentPath = folder;
            await LoadMediaFilesAsync();
        }
    }
    
    [RelayCommand]
    private async Task LoadMediaFilesAsync()
    {
        IsBusy = true;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            StatusMessage = $"Loading {CurrentPath}...";
            MediaFiles.Clear();
            
            if (!Directory.Exists(CurrentPath))
            {
                StatusMessage = "Directory not found";
                return;
            }
            
            // Get all media files
            var supportedExtensions = new[] { ".mov", ".mp4", ".mxf", ".avi", ".mkv", ".wav", ".mp3", ".aiff", ".flac" };
            var files = Directory.GetFiles(CurrentPath)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToArray();
            
            StatusMessage = $"Found {files.Length} media files";
            
            // Load files with metadata
            foreach (var file in files)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;
                
                try
                {
                    var mediaFile = await _mediaService.GetMediaInfoAsync(file, _cancellationTokenSource.Token);
                    MediaFiles.Add(mediaFile);
                }
                catch
                {
                    // Skip files that can't be read
                }
            }
            
            StatusMessage = $"Loaded {MediaFiles.Count} files";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private void SelectMedia(MediaFile? media)
    {
        SelectedMedia = media;
        if (media != null)
            StatusMessage = $"Selected: {media.FileName}";
    }
    
    [RelayCommand]
    private async Task PreviewMediaAsync()
    {
        if (SelectedMedia == null) return;
        
        StatusMessage = $"Previewing {SelectedMedia.FileName}";
        // TODO: Implement preview
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task EditMetadataAsync()
    {
        if (SelectedMedia == null) return;
        
        // Only support WAV files for now
        if (!SelectedMedia.FilePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "Metadata editing only supported for WAV files";
            return;
        }
        
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            StatusMessage = "Reading WAV metadata...";
            
            // Read current BWF metadata
            var bwfMetadata = await _mediaService.ReadWavBwfMetadataAsync(SelectedMedia.FilePath, _cancellationTokenSource.Token);
            var ixmlMetadata = await _mediaService.ReadWavIxmlMetadataAsync(SelectedMedia.FilePath, _cancellationTokenSource.Token);
            
            // For now, show a simple dialog with the metadata
            // In a full implementation, this would open a dialog window
            bwfMetadata.TryGetValue("Description", out var description);
            bwfMetadata.TryGetValue("Originator", out var originator);
            ixmlMetadata.TryGetValue("Scene", out var scene);
            ixmlMetadata.TryGetValue("Take", out var take);
            
            StatusMessage = $"Metadata: Scene={scene ?? ""}, Take={take ?? ""}, Originator={originator ?? ""}";
            
            // TODO: Open dialog for editing
            // For now, just demonstrate reading capability
            await _dialogService.ShowMessageBoxAsync(
                "WAV Metadata",
                $"File: {SelectedMedia.FileName}\n" +
                $"Description: {description}\n" +
                $"Originator: {originator}\n" +
                $"Scene: {scene}\n" +
                $"Take: {take}\n\n" +
                $"Full metadata editing dialog coming soon!"
            );
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error reading metadata: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private void ToggleView()
    {
        IsGridView = !IsGridView;
    }
    
    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadMediaFilesAsync();
            return;
        }
        
        var filtered = MediaFiles.Where(m => 
            m.FileName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        MediaFiles.Clear();
        foreach (var item in filtered)
            MediaFiles.Add(item);
        
        StatusMessage = $"Found {MediaFiles.Count} matching files";
    }
    
    partial void OnShowVideoChanged(bool value)
    {
        OnPropertyChanged(nameof(FilteredMediaFiles));
    }
    
    partial void OnShowAudioChanged(bool value)
    {
        OnPropertyChanged(nameof(FilteredMediaFiles));
    }
    
    partial void OnMediaFilesChanged(ObservableCollection<MediaFile> value)
    {
        OnPropertyChanged(nameof(FilteredMediaFiles));
    }
    
    /// <summary>
    /// Handle folder or file drop
    /// </summary>
    public async Task HandleDropAsync(string path)
    {
        try
        {
            IsDragging = false;
            
            // If it's a directory
            if (Directory.Exists(path))
            {
                CurrentPath = path;
                await LoadMediaFilesAsync();
            }
            // If it's a file
            else if (File.Exists(path))
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    CurrentPath = directory;
                    await LoadMediaFilesAsync();
                    
                    // Select the dropped file
                    SelectedMedia = MediaFiles.FirstOrDefault(m => m.FilePath == path);
                }
            }
            
            // Auto-switch profile based on media type
            await AutoSwitchProfileAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error handling drop: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Automatically switch profile based on loaded media
    /// </summary>
    private async Task AutoSwitchProfileAsync()
    {
        if (!MediaFiles.Any()) return;
        
        var hasVideo = MediaFiles.Any(m => m.IsVideo);
        var hasAudio = MediaFiles.Any(m => m.IsAudio && !m.IsVideo);
        
        // Send message to switch profile if needed
        if (hasAudio && !hasVideo)
        {
            // Request switch to Audio profile
            WeakReferenceMessenger.Default.Send(new ProfileSwitchMessage(switchToVideo: false));
            StatusMessage = "Auto-switched to Audio profile";
        }
        else if (hasVideo && !hasAudio)
        {
            // Request switch to Video profile
            WeakReferenceMessenger.Default.Send(new ProfileSwitchMessage(switchToVideo: true));
            StatusMessage = "Auto-switched to Video profile";
        }
        
        await Task.CompletedTask;
    }
}
