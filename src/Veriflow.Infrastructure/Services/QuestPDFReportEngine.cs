using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Veriflow.Core.Interfaces;

namespace Veriflow.Infrastructure.Services;

/// <summary>
/// QuestPDF-based report generation engine
/// Professional Camera and Sound Reports
/// </summary>
public class QuestPDFReportEngine : IReportEngine
{
    public QuestPDFReportEngine()
    {
        // Set QuestPDF license (Community for non-commercial)
        QuestPDF.Settings.License = LicenseType.Community;
    }
    
    public async Task<string> GenerateCameraReportAsync(
        ReportData reportData,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var document = CreateCameraReportDocument(reportData);
            document.GeneratePdf(outputPath);
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
            document.GeneratePdf(outputPath);
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
            
            return document.GeneratePdf();
        }, cancellationToken);
    }
    
    private Document CreateCameraReportDocument(ReportData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
                
                page.Header().Element(c => ComposeHeader(c, "CAMERA REPORT", data));
                page.Content().Element(c => ComposeCameraContent(c, data));
                page.Footer().Element(ComposeFooter);
            });
        });
    }
    
    private Document CreateSoundReportDocument(ReportData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
                
                page.Header().Element(c => ComposeHeader(c, "SOUND REPORT", data));
                page.Content().Element(c => ComposeSoundContent(c, data));
                page.Footer().Element(ComposeFooter);
            });
        });
    }
    
    private void ComposeHeader(IContainer container, string title, ReportData data)
    {
        container.Column(column =>
        {
            // Title
            column.Item().AlignCenter().Text(title)
                .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
            
            column.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
            
            // Project Info
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Project: {data.ProjectName}").Bold();
                    col.Item().Text($"Production: {data.ProductionCompany}");
                });
                
                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text($"Date: {data.Date:yyyy-MM-dd}");
                    col.Item().AlignRight().Text($"Location: {data.Location}");
                });
            });
            
            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }
    
    private void ComposeCameraContent(IContainer container, ReportData data)
    {
        container.Column(column =>
        {
            // Crew Section
            column.Item().PaddingTop(10).Element(c => ComposeSection(c, "CREW", new[]
            {
                ("Director", data.Director),
                ("DOP", data.DOP),
                ("Camera Operator", data.CameraOperator),
                ("Data Manager", data.DataManager)
            }));
            
            // Equipment Section
            column.Item().PaddingTop(10).Element(c => ComposeSection(c, "EQUIPMENT", new[]
            {
                ("Camera", data.CameraModel),
                ("Lens", data.LensInfo)
            }));
            
            // Clips Table
            column.Item().PaddingTop(15).Element(c => ComposeClipsTable(c, data.Clips, true));
            
            // Notes
            if (!string.IsNullOrWhiteSpace(data.Notes))
            {
                column.Item().PaddingTop(15).Column(col =>
                {
                    col.Item().Text("NOTES").FontSize(12).Bold();
                    col.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(10).Text(data.Notes);
                });
            }
        });
    }
    
    private void ComposeSoundContent(IContainer container, ReportData data)
    {
        container.Column(column =>
        {
            // Crew Section
            column.Item().PaddingTop(10).Element(c => ComposeSection(c, "CREW", new[]
            {
                ("Sound Recordist", data.SoundRecordist),
                ("Boom Operator", data.BoomOperator)
            }));
            
            // Equipment Section
            column.Item().PaddingTop(10).Element(c => ComposeSection(c, "EQUIPMENT", new[]
            {
                ("Recorder", data.RecorderModel),
                ("Microphone", data.MicrophoneInfo),
                ("Timecode Rate", data.TimecodeRate),
                ("Bit Depth", data.BitDepth)
            }));
            
            // Clips Table
            column.Item().PaddingTop(15).Element(c => ComposeClipsTable(c, data.Clips, false));
            
            // Notes
            if (!string.IsNullOrWhiteSpace(data.Notes))
            {
                column.Item().PaddingTop(15).Column(col =>
                {
                    col.Item().Text("NOTES").FontSize(12).Bold();
                    col.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(10).Text(data.Notes);
                });
            }
        });
    }
    
    private void ComposeSection(IContainer container, string title, (string Label, string Value)[] fields)
    {
        container.Column(column =>
        {
            column.Item().Text(title).FontSize(12).Bold();
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });
                
                foreach (var (label, value) in fields)
                {
                    table.Cell().Padding(2).Text(label + ":").Bold();
                    table.Cell().Padding(2).Text(value);
                }
            });
        });
    }
    
    private void ComposeClipsTable(IContainer container, System.Collections.Generic.List<ReportClip> clips, bool isCamera)
    {
        container.Column(column =>
        {
            column.Item().Text("CLIPS").FontSize(12).Bold();
            column.Item().PaddingTop(5).Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);  // Scene
                    columns.ConstantColumn(40);  // Take
                    columns.ConstantColumn(80);  // Timecode
                    columns.ConstantColumn(60);  // Duration
                    columns.RelativeColumn();    // FileName
                    if (isCamera)
                    {
                        columns.ConstantColumn(70);  // Resolution
                        columns.ConstantColumn(50);  // FPS
                    }
                    columns.ConstantColumn(40);  // Good
                });
                
                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Scene").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Take").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Timecode").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Duration").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("File Name").FontColor(Colors.White).Bold();
                    
                    if (isCamera)
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Resolution").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("FPS").FontColor(Colors.White).Bold();
                    }
                    
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Good").FontColor(Colors.White).Bold();
                });
                
                // Rows
                foreach (var clip in clips)
                {
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(clip.Scene);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(clip.Take);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(clip.Timecode);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(clip.Duration);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(clip.FileName);
                    
                    if (isCamera)
                    {
                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(clip.Resolution);
                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(clip.FrameRate);
                    }
                    
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .AlignCenter().Text(clip.IsGood ? "✓" : "")
                        .FontColor(Colors.Green.Darken2).Bold();
                }
            });
        });
    }
    
    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Generated by ").FontSize(8).FontColor(Colors.Grey.Medium);
            text.Span("Veriflow Pro").FontSize(8).Bold().FontColor(Colors.Blue.Darken2);
            text.Span($" - {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }
}
