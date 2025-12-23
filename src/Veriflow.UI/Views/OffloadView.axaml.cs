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
        // Only allow copy for folders (using new DataTransfer API)
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }
    
    private void Drop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File)) return;
        
        var files = e.DataTransfer.TryGetFiles();
        
        if (files != null && files.Length > 0 && DataContext is ViewModels.OffloadViewModel vm)
        {
            var path = files[0].Path.LocalPath;
            
            // Check if it's a directory
            if (System.IO.Directory.Exists(path))
            {
                // Determine which TextBox was dropped on by checking the Grid row
                if (sender is TextBox textBox && textBox.Parent is Grid grid)
                {
                    var row = Grid.GetRow(textBox);
                    
                    switch (row)
                    {
                        case 0:
                            vm.SourceFolder = path;
                            vm.AppendLog($"Source: {path}");
                            break;
                        case 1:
                            vm.DestinationA = path;
                            vm.AppendLog($"Destination A: {path}");
                            break;
                        case 2:
                            vm.DestinationB = path;
                            vm.AppendLog($"Destination B: {path}");
                            break;
                    }
                }
            }
            else
            {
                // It's a file, not a folder
                vm.AppendLog("⚠️ Please drop a folder, not a file");
            }
        }
    }
}
