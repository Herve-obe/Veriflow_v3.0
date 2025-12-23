using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Veriflow.Core.Models;

namespace Veriflow.UI.ViewModels;

/// <summary>
/// ViewModel for REPORTS page (F6)
/// Quality control reports and EDL generation
/// </summary>
public partial class ReportsViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<MediaFile> _reportFiles = new();
    
    [ObservableProperty]
    private string _reportType = "Camera Report"; // Camera Report or Sound Report
    
    [ObservableProperty]
    private bool _isVideoMode = true;
    
    public ReportsViewModel()
    {
        StatusMessage = "Add files to generate report";
    }
    
    [RelayCommand]
    private void AddFiles()
    {
        // TODO: Implement file picker
        StatusMessage = "Select files for report";
    }
    
    [RelayCommand]
    private async Task GeneratePdfReportAsync()
    {
        IsBusy = true;
        StatusMessage = $"Generating {ReportType}...";
        
        try
        {
            // TODO: Implement QuestPDF report generation
            await Task.Delay(100); // Placeholder
        }
        finally
        {
            IsBusy = false;
            StatusMessage = "Report generated";
        }
    }
    
    [RelayCommand]
    private async Task ExportEdlAsync()
    {
        if (!IsVideoMode) return;
        
        IsBusy = true;
        StatusMessage = "Exporting EDL/ALE...";
        
        try
        {
            // TODO: Implement EDL/ALE generation
            await Task.Delay(100); // Placeholder
        }
        finally
        {
            IsBusy = false;
            StatusMessage = "EDL/ALE exported";
        }
    }
    
    [RelayCommand]
    private void ClearFiles()
    {
        ReportFiles.Clear();
        StatusMessage = "Files cleared";
    }
    
    partial void OnIsVideoModeChanged(bool value)
    {
        ReportType = value ? "Camera Report" : "Sound Report";
    }
}
