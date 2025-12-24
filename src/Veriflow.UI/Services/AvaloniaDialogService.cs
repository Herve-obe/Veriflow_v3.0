using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Interfaces;

namespace Veriflow.UI.Services;

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
        Window? dialog = null;
        
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Width = 100,
            Height = 36
        };
        
        okButton.Click += (s, e) => dialog?.Close();
        
        dialog = new Window
        {
            Title = title,
            Width = 500,
            Height = 250,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SystemDecorations = Avalonia.Controls.SystemDecorations.None,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#222628")),
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 20,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        FontSize = 14
                    },
                    okButton
                }
            }
        };
        
        await dialog.ShowDialog(_mainWindow);
    }
    
    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        bool result = false;
        Window? dialog = null;
        
        var yesButton = new Button
        {
            Content = "Yes",
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Width = 100,
            Height = 36
        };
        
        var noButton = new Button
        {
            Content = "No",
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Width = 100,
            Height = 36
        };
        
        yesButton.Click += (s, e) => { result = true; dialog?.Close(); };
        noButton.Click += (s, e) => { result = false; dialog?.Close(); };
        
        dialog = new Window
        {
            Title = title,
            Width = 500,
            Height = 250,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SystemDecorations = Avalonia.Controls.SystemDecorations.None,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#222628")),
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 20,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        FontSize = 14
                    },
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Spacing = 10,
                        Children =
                        {
                            yesButton,
                            noButton
                        }
                    }
                }
            }
        };
        
        await dialog.ShowDialog(_mainWindow);
        return result;
    }
}
