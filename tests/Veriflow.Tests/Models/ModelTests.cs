using Xunit;
using FluentAssertions;
using Veriflow.Core.Models;

namespace Veriflow.Tests.Models;

/// <summary>
/// Unit tests for core models
/// </summary>
public class ModelTests
{
    [Fact]
    public void Session_ShouldInitializeWithDefaults()
    {
        // Act
        var session = new Session();
        
        // Assert
        session.Id.Should().NotBeEmpty();
        session.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        session.ProjectName.Should().BeEmpty();
    }
    
    [Fact]
    public void MediaFile_ShouldStoreMetadata()
    {
        // Act
        var mediaFile = new MediaFile
        {
            FilePath = "test.mp4",
            FileName = "test.mp4",
            FileSize = 1024000,
            Type = MediaType.Video
        };
        
        // Assert
        mediaFile.FilePath.Should().Be("test.mp4");
        mediaFile.FileSize.Should().Be(1024000);
        mediaFile.Type.Should().Be(MediaType.Video);
    }
    
    [Fact]
    public void TranscodeJob_ShouldTrackProgress()
    {
        // Act
        var job = new TranscodeJob
        {
            InputPath = "input.mov",
            OutputPath = "output.mp4",
            Status = TranscodeStatus.Processing,
            Progress = 50.5
        };
        
        // Assert
        job.Status.Should().Be(TranscodeStatus.Processing);
        job.Progress.Should().Be(50.5);
    }
    
    [Fact]
    public void ReportData_ShouldContainAllFields()
    {
        // Act
        var reportData = new ReportData
        {
            ProjectName = "Test Project",
            Director = "John Doe",
            Date = DateTime.Now
        };
        
        // Assert
        reportData.ProjectName.Should().Be("Test Project");
        reportData.Director.Should().Be("John Doe");
        reportData.Clips.Should().NotBeNull();
        reportData.Clips.Should().BeEmpty();
    }
}
