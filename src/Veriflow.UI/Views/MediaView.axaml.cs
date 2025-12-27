using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Linq;
using Veriflow.UI.ViewModels;

namespace Veriflow.UI.Views;

public partial class MediaView : UserControl
{
    public MediaView()
    {
        InitializeComponent();
        
        // Setup drag & drop
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
    }
    
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        // Only allow file drops
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        
        e.Handled = true;
    }
    
    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (DataContext is MediaViewModel vm && e.Data.Contains(DataFormats.Files))
        {
            vm.IsDragging = true;
        }
    }
    
    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (DataContext is MediaViewModel vm)
        {
            vm.IsDragging = false;
        }
    }
    
    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is MediaViewModel vm && e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            var firstFile = files?.FirstOrDefault();
            
            if (firstFile != null)
            {
                var path = firstFile.Path.LocalPath;
                await vm.HandleDropAsync(path);
            }
        }
        
        e.Handled = true;
    }
}
