using Xunit;
using FluentAssertions;
using Veriflow.Core.Interfaces;

namespace Veriflow.Tests.Interfaces;

/// <summary>
/// Integration tests for Transcode Engine
/// </summary>
public class TranscodeEngineTests
{
    [Fact]
    public void GetAvailablePresets_ShouldReturnPresets()
    {
        // Arrange
        var engine = new Veriflow.Infrastructure.Services.FFmpegTranscodeEngine();
        
        // Act
        var presets = engine.GetAvailablePresets();
        
        // Assert
        presets.Should().NotBeNull();
        presets.Should().NotBeEmpty();
        presets.Should().Contain(p => p.Id == "prores_422");
        presets.Should().Contain(p => p.Id == "h264_high");
    }
    
    [Fact]
    public void TranscodePreset_ShouldHaveRequiredProperties()
    {
        // Arrange
        var engine = new Veriflow.Infrastructure.Services.FFmpegTranscodeEngine();
        var presets = engine.GetAvailablePresets();
        
        // Act
        var proresPreset = presets.First(p => p.Id == "prores_422");
        
        // Assert
        proresPreset.Name.Should().NotBeNullOrEmpty();
        proresPreset.Description.Should().NotBeNullOrEmpty();
        proresPreset.VideoCodec.Should().NotBeNullOrEmpty();
        proresPreset.Container.Should().NotBeNullOrEmpty();
    }
}
