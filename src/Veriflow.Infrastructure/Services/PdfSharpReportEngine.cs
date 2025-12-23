using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// PdfSharp + MigraDoc based report generation engine
/// Professional Camera and Sound Reports with MIT license
/// </summary>
public class PdfSharpReportEngine : IReportEngine
{
    public async Task<string> GenerateCameraReportAsync(
        ReportData reportData,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var document = CreateCameraReportDocument(reportData);
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(outputPath);
            return outputPath;
        }, cancellationToken);
    }
    
    public async Task<string> GenerateSoundReportAsync(
        ReportData reportData,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var document = CreateSoundReportDocument(reportData);
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(outputPath);
            return outputPath;
        }, cancellationToken);
    }
    
    public async Task<byte[]> PreviewReportAsync(
        ReportData reportData,
        ReportType type,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var document = type == ReportType.Camera
                ? CreateCameraReportDocument(reportData)
                : CreateSoundReportDocument(reportData);
            
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();
            
            using var stream = new MemoryStream();
            pdfRenderer.PdfDocument.Save(stream, false);
            return stream.ToArray();
        }, cancellationToken);
    }
    
    private Document CreateCameraReportDocument(ReportData data)
    {
        var document = new Document();
        var section = document.AddSection();
        
        // Page setup
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
        section.PageSetup.RightMargin = Unit.FromCentimeter(2);
        section.PageSetup.TopMargin = Unit.FromCentimeter(2);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
        
        // Title
        var title = section.AddParagraph("CAMERA REPORT");
        title.Format.Font.Size = 20;
        title.Format.Font.Bold = true;
        title.Format.Font.Color = Colors.DarkBlue;
        title.Format.SpaceAfter = 12;
        title.Format.Alignment = ParagraphAlignment.Center;
        
        // Project Info
        AddProjectInfo(section, data);
        
        // Crew Section
        section.AddParagraph("CREW").Format.Font.Bold = true;
        section.AddParagraph("CREW").Format.SpaceAfter = 6;
        AddKeyValue(section, "Director", data.Director);
        AddKeyValue(section, "DOP", data.DOP);
        AddKeyValue(section, "Camera Operator", data.CameraOperator);
        AddKeyValue(section, "Data Manager", data.DataManager);
        section.AddParagraph().Format.SpaceAfter = 12;
        
        // Equipment Section
        section.AddParagraph("EQUIPMENT").Format.Font.Bold = true;
        section.AddParagraph("EQUIPMENT").Format.SpaceAfter = 6;
        AddKeyValue(section, "Camera", data.CameraModel);
        AddKeyValue(section, "Lens", data.LensInfo);
        section.AddParagraph().Format.SpaceAfter = 12;
        
        // Clips Table
        if (data.Clips.Any())
        {
            section.AddParagraph("CLIPS").Format.Font.Bold = true;
            section.AddParagraph("CLIPS").Format.SpaceAfter = 6;
            AddCameraClipsTable(section, data.Clips);
        }
        
        // Notes
        if (!string.IsNullOrWhiteSpace(data.Notes))
        {
            section.AddParagraph().Format.SpaceAfter = 12;
            section.AddParagraph("NOTES").Format.Font.Bold = true;
            section.AddParagraph("NOTES").Format.SpaceAfter = 6;
            section.AddParagraph(data.Notes);
        }
        
        // Footer
        AddFooter(section);
        
        return document;
    }
    
    private Document CreateSoundReportDocument(ReportData data)
    {
        var document = new Document();
        var section = document.AddSection();
        
        // Page setup
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
        section.PageSetup.RightMargin = Unit.FromCentimeter(2);
        section.PageSetup.TopMargin = Unit.FromCentimeter(2);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
        
        // Title
        var title = section.AddParagraph("SOUND REPORT");
        title.Format.Font.Size = 20;
        title.Format.Font.Bold = true;
        title.Format.Font.Color = Colors.DarkRed;
        title.Format.SpaceAfter = 12;
        title.Format.Alignment = ParagraphAlignment.Center;
        
        // Project Info
        AddProjectInfo(section, data);
        
        // Crew Section
        section.AddParagraph("CREW").Format.Font.Bold = true;
        section.AddParagraph("CREW").Format.SpaceAfter = 6;
        AddKeyValue(section, "Sound Recordist", data.SoundRecordist);
        AddKeyValue(section, "Boom Operator", data.BoomOperator);
        section.AddParagraph().Format.SpaceAfter = 12;
        
        // Equipment Section
        section.AddParagraph("EQUIPMENT").Format.Font.Bold = true;
        section.AddParagraph("EQUIPMENT").Format.SpaceAfter = 6;
        AddKeyValue(section, "Recorder", data.RecorderModel);
        AddKeyValue(section, "Microphone", data.MicrophoneInfo);
        AddKeyValue(section, "Timecode Rate", data.TimecodeRate);
        AddKeyValue(section, "Bit Depth", data.BitDepth);
        section.AddParagraph().Format.SpaceAfter = 12;
        
        // Clips Table
        if (data.Clips.Any())
        {
            section.AddParagraph("CLIPS").Format.Font.Bold = true;
            section.AddParagraph("CLIPS").Format.SpaceAfter = 6;
            AddSoundClipsTable(section, data.Clips);
        }
        
        // Notes
        if (!string.IsNullOrWhiteSpace(data.Notes))
        {
            section.AddParagraph().Format.SpaceAfter = 12;
            section.AddParagraph("NOTES").Format.Font.Bold = true;
            section.AddParagraph("NOTES").Format.SpaceAfter = 6;
            section.AddParagraph(data.Notes);
        }
        
        // Footer
        AddFooter(section);
        
        return document;
    }
    
    private void AddProjectInfo(Section section, ReportData data)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        
        table.AddColumn(Unit.FromCentimeter(8));
        table.AddColumn(Unit.FromCentimeter(8));
        
        var row = table.AddRow();
        row.Cells[0].AddParagraph($"Project: {data.ProjectName}");
        row.Cells[1].AddParagraph($"Date: {data.Date:yyyy-MM-dd}");
        
        row = table.AddRow();
        row.Cells[0].AddParagraph($"Production: {data.ProductionCompany}");
        row.Cells[1].AddParagraph($"Location: {data.Location}");
        
        section.AddParagraph().Format.SpaceAfter = 12;
    }
    
    private void AddKeyValue(Section section, string key, string value)
    {
        var para = section.AddParagraph();
        para.AddFormattedText($"{key}: ", TextFormat.Bold);
        para.AddText(value);
    }
    
    private void AddCameraClipsTable(Section section, List<ReportClip> clips)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        
        // Define columns
        table.AddColumn(Unit.FromCentimeter(1.5)); // Scene
        table.AddColumn(Unit.FromCentimeter(1.5)); // Take
        table.AddColumn(Unit.FromCentimeter(2.5)); // Timecode
        table.AddColumn(Unit.FromCentimeter(2)); // Duration
        table.AddColumn(Unit.FromCentimeter(4)); // FileName
        table.AddColumn(Unit.FromCentimeter(2)); // Resolution
        table.AddColumn(Unit.FromCentimeter(1.5)); // FPS
        table.AddColumn(Unit.FromCentimeter(1)); // Good
        
        // Header row
        var headerRow = table.AddRow();
        headerRow.Shading.Color = Colors.LightBlue;
        headerRow.HeadingFormat = true;
        headerRow.Format.Font.Bold = true;
        
        headerRow.Cells[0].AddParagraph("Scene");
        headerRow.Cells[1].AddParagraph("Take");
        headerRow.Cells[2].AddParagraph("Timecode");
        headerRow.Cells[3].AddParagraph("Duration");
        headerRow.Cells[4].AddParagraph("File Name");
        headerRow.Cells[5].AddParagraph("Resolution");
        headerRow.Cells[6].AddParagraph("FPS");
        headerRow.Cells[7].AddParagraph("Good");
        
        // Data rows
        foreach (var clip in clips)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(clip.Scene);
            row.Cells[1].AddParagraph(clip.Take);
            row.Cells[2].AddParagraph(clip.Timecode);
            row.Cells[3].AddParagraph(clip.Duration);
            row.Cells[4].AddParagraph(clip.FileName);
            row.Cells[5].AddParagraph(clip.Resolution);
            row.Cells[6].AddParagraph(clip.FrameRate);
            row.Cells[7].AddParagraph(clip.IsGood ? "✓" : "");
        }
    }
    
    private void AddSoundClipsTable(Section section, List<ReportClip> clips)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        
        // Define columns
        table.AddColumn(Unit.FromCentimeter(2)); // Scene
        table.AddColumn(Unit.FromCentimeter(2)); // Take
        table.AddColumn(Unit.FromCentimeter(3)); // Timecode
        table.AddColumn(Unit.FromCentimeter(2.5)); // Duration
        table.AddColumn(Unit.FromCentimeter(5)); // FileName
        table.AddColumn(Unit.FromCentimeter(1.5)); // Good
        
        // Header row
        var headerRow = table.AddRow();
        headerRow.Shading.Color = Colors.LightCoral;
        headerRow.HeadingFormat = true;
        headerRow.Format.Font.Bold = true;
        
        headerRow.Cells[0].AddParagraph("Scene");
        headerRow.Cells[1].AddParagraph("Take");
        headerRow.Cells[2].AddParagraph("Timecode");
        headerRow.Cells[3].AddParagraph("Duration");
        headerRow.Cells[4].AddParagraph("File Name");
        headerRow.Cells[5].AddParagraph("Good");
        
        // Data rows
        foreach (var clip in clips)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(clip.Scene);
            row.Cells[1].AddParagraph(clip.Take);
            row.Cells[2].AddParagraph(clip.Timecode);
            row.Cells[3].AddParagraph(clip.Duration);
            row.Cells[4].AddParagraph(clip.FileName);
            row.Cells[5].AddParagraph(clip.IsGood ? "✓" : "");
        }
    }
    
    private void AddFooter(Section section)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Center;
        footer.Format.Font.Size = 8;
        footer.Format.Font.Color = Colors.Gray;
        footer.AddText($"Generated by Veriflow Pro - {DateTime.Now:yyyy-MM-dd HH:mm}");
    }
}
