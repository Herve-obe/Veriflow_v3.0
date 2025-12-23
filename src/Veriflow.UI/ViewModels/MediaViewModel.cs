using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Models;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for MEDIA page (F2)
/// File browser and media library management
/// </summary>
public partial class MediaViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _currentPath = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<MediaFile> _mediaFiles = new();
    
    [ObservableProperty]
    private MediaFile? _selectedMedia;
    
    [ObservableProperty]
    private string _searchQuery = string.Empty;
    
    public MediaViewModel()
    {
        StatusMessage = "Browse media files";
    }
    
    [RelayCommand]
    private void NavigateToFolder(string path)
    {
        CurrentPath = path;
        LoadMediaFiles();
    }
    
    [RelayCommand]
    private void LoadMediaFiles()
    {
        IsBusy = true;
        StatusMessage = $"Loading files from {CurrentPath}";
        
        try
        {
            // TODO: Implement file loading with IMediaService
            MediaFiles.Clear();
        }
        finally
        {
            IsBusy = false;
            StatusMessage = $"{MediaFiles.Count} files loaded";
        }
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
}
