using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Linq;

namespace Veriflow.UI.Views;

public partial class OffloadView : UserControl
{
    public OffloadView()
    {
        InitializeComponent();
        
        // Setup drag & drop handlers
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }
    
    private void DragOver(object? sender, DragEventArgs e)
    {
        // Only allow folders
        e.DragEffects = DragDropEffects.Copy;
    }
    
    private void Drop(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var files = e.Data.GetFiles()?.ToArray();
#pragma warning restore CS0618
        if (files != null && files.Length > 0)
        {
            var path = files[0].Path.LocalPath;
            
            // Determine which border was dropped on
            if (sender is Border border && DataContext is ViewModels.OffloadViewModel vm)
            {
                // Check if it's a directory
                if (System.IO.Directory.Exists(path))
                {
                    // Find which border by checking the row
                    var parent = border.Parent as Grid;
                    var row = Grid.GetRow(border);
                    
                    if (row == 0)
                        vm.SourceFolder = path;
                    else if (row == 1)
                        vm.DestinationA = path;
                    else if (row == 2)
                        vm.DestinationB = path;
                        
                    vm.AppendLog($"Dropped: {path}");
                }
            }
        }
    }
}
