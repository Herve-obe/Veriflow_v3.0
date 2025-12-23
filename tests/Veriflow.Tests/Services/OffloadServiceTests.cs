using Xunit;
using FluentAssertions;
using Veriflow.Core.Services;
using Veriflow.Core.Models;
using System.IO;

namespace Veriflow.Tests.Services;

/// <summary>
/// Unit tests for OffloadService
/// </summary>
public class OffloadServiceTests
{
    [Fact]
    public void CreateOffloadJob_ShouldCreateValidJob()
    {
        // Arrange
        var service = new OffloadService();
        var sourcePath = "D:\\source";
        var destinationPath = "E:\\destination";
        
        // Act
        var job = service.CreateOffloadJob(sourcePath, destinationPath);
        
        // Assert
        job.Should().NotBeNull();
        job.SourcePath.Should().Be(sourcePath);
        job.DestinationPath.Should().Be(destinationPath);
        job.Status.Should().Be(OffloadStatus.Pending);
        job.Id.Should().NotBeEmpty();
    }
    
    [Fact]
    public void CreateOffloadJob_WithNullSource_ShouldThrowException()
    {
        // Arrange
        var service = new OffloadService();
        
        // Act & Assert
        var act = () => service.CreateOffloadJob(null!, "destination");
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void CalculateChecksum_ShouldReturnValidHash()
    {
        // Arrange
        var service = new OffloadService();
        var testFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
        var testContent = "Test content for checksum";
        
        try
        {
            File.WriteAllText(testFile, testContent);
            
            // Act
            var checksum = service.CalculateChecksum(testFile);
            
            // Assert
            checksum.Should().NotBeNullOrEmpty();
            checksum.Length.Should().Be(64); // SHA256 hex length
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }
    
    [Fact]
    public void VerifyChecksum_WithMatchingFiles_ShouldReturnTrue()
    {
        // Arrange
        var service = new OffloadService();
        var testFile1 = Path.Combine(Path.GetTempPath(), $"test1_{Guid.NewGuid()}.txt");
        var testFile2 = Path.Combine(Path.GetTempPath(), $"test2_{Guid.NewGuid()}.txt");
        var testContent = "Identical content";
        
        try
        {
            File.WriteAllText(testFile1, testContent);
            File.WriteAllText(testFile2, testContent);
            
            var checksum1 = service.CalculateChecksum(testFile1);
            var checksum2 = service.CalculateChecksum(testFile2);
            
            // Act
            var isMatch = checksum1 == checksum2;
            
            // Assert
            isMatch.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(testFile1)) File.Delete(testFile1);
            if (File.Exists(testFile2)) File.Delete(testFile2);
        }
    }
}
