using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;
using Veriflow.Core.Models;

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
    private void EditMetadata()
    {
        if (SelectedMedia == null) return;
        
        StatusMessage = "Edit metadata";
        // TODO: Implement metadata editor
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
}
