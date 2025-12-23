using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// Avalonia implementation of dialog service
/// </summary>
public class AvaloniaDialogService : IDialogService
{
    private readonly Window _mainWindow;
    
    public AvaloniaDialogService(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }
    
    public async Task<string?> ShowFolderPickerAsync(string title = "Select Folder")
    {
        var folders = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });
        
        return folders.FirstOrDefault()?.Path.LocalPath;
    }
    
    public async Task<string[]?> ShowFilePickerAsync(string title = "Select Files", bool allowMultiple = true, string[]? filters = null)
    {
        var fileTypes = filters != null
            ? new[] { new FilePickerFileType("Files") { Patterns = filters } }
            : null;
        
        var files = await _mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple,
            FileTypeFilter = fileTypes
        });
        
        return files.Select(f => f.Path.LocalPath).ToArray();
    }
    
    public async Task<string?> ShowSaveFileDialogAsync(string title = "Save File", string? defaultFileName = null, string[]? filters = null)
    {
        var fileTypes = filters != null
            ? new[] { new FilePickerFileType("Files") { Patterns = filters } }
            : null;
        
        var file = await _mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultFileName,
            FileTypeChoices = fileTypes
        });
        
        return file?.Path.LocalPath;
    }
    
    public async Task ShowMessageBoxAsync(string title, string message)
    {
        // TODO: Implement proper message box (Avalonia doesn't have built-in MessageBox)
        // For now, we'll use a simple approach
        await Task.CompletedTask;
    }
    
    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        // TODO: Implement proper confirmation dialog
        await Task.CompletedTask;
        return true;
    }
}
