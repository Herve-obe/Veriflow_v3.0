using Xunit;
using FluentAssertions;
using Veriflow.Core.Services;
using Veriflow.Core.Models;

namespace Veriflow.Tests.Services;

/// <summary>
/// Unit tests for SessionService
/// </summary>
public class SessionServiceTests
{
    [Fact]
    public void CreateSession_ShouldCreateValidSession()
    {
        // Arrange
        var service = new SessionService();
        var projectName = "Test Project";
        
        // Act
        var session = service.CreateSession(projectName);
        
        // Assert
        session.Should().NotBeNull();
        session.ProjectName.Should().Be(projectName);
        session.Id.Should().NotBeEmpty();
        session.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void CreateSession_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var service = new SessionService();
        
        // Act & Assert
        var act = () => service.CreateSession(string.Empty);
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void SaveSession_ShouldPersistSession()
    {
        // Arrange
        var service = new SessionService();
        var session = service.CreateSession("Test Project");
        var filePath = Path.Combine(Path.GetTempPath(), $"test_session_{Guid.NewGuid()}.json");
        
        try
        {
            // Act
            service.SaveSession(session, filePath);
            
            // Assert
            File.Exists(filePath).Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
    
    [Fact]
    public void LoadSession_ShouldRestoreSession()
    {
        // Arrange
        var service = new SessionService();
        var originalSession = service.CreateSession("Test Project");
        var filePath = Path.Combine(Path.GetTempPath(), $"test_session_{Guid.NewGuid()}.json");
        
        try
        {
            service.SaveSession(originalSession, filePath);
            
            // Act
            var loadedSession = service.LoadSession(filePath);
            
            // Assert
            loadedSession.Should().NotBeNull();
            loadedSession.ProjectName.Should().Be(originalSession.ProjectName);
            loadedSession.Id.Should().Be(originalSession.Id);
        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
