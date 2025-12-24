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
        
        // Setup drag & drop handlers for the entire control
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
        
        // Diagnostic: Log DataContext changes
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is ViewModels.OffloadViewModel vm)
            {
                vm.AppendLog($">>> DataContext set to OffloadViewModel");
                vm.AppendLog($">>> StartOffloadCommand exists: {vm.StartOffloadCommand != null}");
            }
        };
    }
    
    // Diagnostic: Click event handler to test if button responds
    private void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.OffloadViewModel vm)
        {
            vm.AppendLog(">>> CLICK EVENT FIRED - Button works!");
            vm.AppendLog($">>> StartOffloadCommand: {vm.StartOffloadCommand?.GetType().Name ?? "NULL"}");
            vm.AppendLog($">>> CanExecute: {vm.StartOffloadCommand?.CanExecute(null) ?? false}");
            
            // Try to execute command manually
            if (vm.StartOffloadCommand?.CanExecute(null) == true)
            {
                vm.AppendLog(">>> Executing command manually...");
                vm.StartOffloadCommand.Execute(null);
            }
            else
            {
                vm.AppendLog(">>> Command CanExecute returned FALSE");
            }
        }
    }
    
    private void DragOver(object? sender, DragEventArgs e)
    {
        // Only allow copy for folders
        if (e.Data.Contains(DataFormats.Files))
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
        if (!e.Data.Contains(DataFormats.Files)) return;
        
        var files = e.Data.GetFiles()?.ToArray();
        
        if (files != null && files.Length > 0 && DataContext is ViewModels.OffloadViewModel vm)
        {
            var path = files[0].Path.LocalPath;
            
            // Check if it's a directory
            if (System.IO.Directory.Exists(path))
            {
                // Find which Border was dropped on by checking the source
                var source = e.Source;
                
                // Traverse up to find the Border with a Name
                while (source != null)
                {
                    if (source is Border border && !string.IsNullOrEmpty(border.Name))
                    {
                        switch (border.Name)
                        {
                            case "SourceDropZone":
                                vm.SourceFolder = path;
                                vm.AppendLog($"Source: {path}");
                                return;
                            case "DestinationADropZone":
                                vm.DestinationA = path;
                                vm.AppendLog($"Destination A: {path}");
                                return;
                            case "DestinationBDropZone":
                                vm.DestinationB = path;
                                vm.AppendLog($"Destination B: {path}");
                                return;
                            case "VerifyTargetDropZone":
                                vm.VerifyTargetFolder = path;
                                vm.AppendLog($"Verify Target: {path}");
                                return;
                        }
                    }
                    
                    if (source is Avalonia.Visual visual)
                    {
                        source = visual.Parent;
                    }
                    else
                    {
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
