using System.Threading.Tasks;

namespace Veriflow.Core.Interfaces;

/// <summary>
/// Dialog service for file and folder selection
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Show folder picker dialog
    /// </summary>
    Task<string?> ShowFolderPickerAsync(string title = "Select Folder");
    
    /// <summary>
    /// Show file picker dialog
    /// </summary>
    Task<string[]?> ShowFilePickerAsync(string title = "Select Files", bool allowMultiple = true, string[]? filters = null);
    
    /// <summary>
    /// Show save file dialog
    /// </summary>
    Task<string?> ShowSaveFileDialogAsync(string title = "Save File", string? defaultFileName = null, string[]? filters = null);
    
    /// <summary>
    /// Show message box
    /// </summary>
    Task ShowMessageBoxAsync(string title, string message);
    
    /// <summary>
    /// Show confirmation dialog
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);
}
