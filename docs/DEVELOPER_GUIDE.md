# Veriflow 3.0 - Developer Guide

## Architecture Overview

Veriflow 3.0 follows a **Clean Architecture** pattern with clear separation of concerns.

### Project Structure

```
Veriflow 3.0/
├── src/
│   ├── Veriflow.Core/              # Domain Layer
│   │   ├── Interfaces/             # Service contracts
│   │   ├── Models/                 # Domain models
│   │   └── Services/               # Core business logic
│   │
│   ├── Veriflow.Infrastructure/    # Infrastructure Layer
│   │   └── Services/               # Service implementations
│   │       ├── FFmpegMediaService.cs
│   │       ├── NAudioEngine.cs
│   │       ├── LibVLCVideoEngine.cs
│   │       ├── FFmpegSyncEngine.cs
│   │       ├── FFmpegTranscodeEngine.cs
│   │       └── QuestPDFReportEngine.cs
│   │
│   └── Veriflow.UI/                # Presentation Layer
│       ├── ViewModels/             # MVVM ViewModels
│       ├── Views/                  # Avalonia Views
│       ├── Converters/             # Value converters
│       └── ServiceConfiguration.cs # DI setup
│
└── tests/
    └── Veriflow.Tests/             # Test suite
```

---

## Design Patterns

### 1. MVVM (Model-View-ViewModel)

**ViewModels** inherit from `ViewModelBase`:

```csharp
public partial class MyViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _myProperty;
    
    [RelayCommand]
    private async Task MyCommandAsync()
    {
        // Command logic
    }
}
```

**Benefits**:
- Testability
- Separation of concerns
- Data binding support

### 2. Dependency Injection

Services registered in `ServiceConfiguration.cs`:

```csharp
public static void ConfigureServices(
    IServiceCollection services, 
    Window mainWindow)
{
    // Core Services
    services.AddSingleton<IMediaService, FFmpegMediaService>();
    services.AddSingleton<IAudioEngine, NAudioEngine>();
    
    // ViewModels
    services.AddTransient<MediaViewModel>();
}
```

### 3. Repository Pattern

Session management:

```csharp
public interface ISessionService
{
    Session CreateSession(string projectName);
    void SaveSession(Session session, string filePath);
    Session LoadSession(string filePath);
}
```

---

## Core Services

### 1. IMediaService

**Purpose**: Media file analysis and metadata extraction

**Implementation**: `FFmpegMediaService`

```csharp
public interface IMediaService
{
    Task<MediaInfo> GetMediaInfoAsync(
        string filePath, 
        CancellationToken cancellationToken);
    
    Task<string> GenerateThumbnailAsync(
        string videoPath, 
        TimeSpan position, 
        CancellationToken cancellationToken);
}
```

**Usage**:
```csharp
var mediaInfo = await _mediaService.GetMediaInfoAsync(
    filePath, 
    cancellationToken
);

Console.WriteLine($"Duration: {mediaInfo.Duration}");
Console.WriteLine($"Resolution: {mediaInfo.Width}x{mediaInfo.Height}");
```

---

### 2. IAudioEngine

**Purpose**: Multi-track audio playback

**Implementation**: `NAudioEngine`

```csharp
public interface IAudioEngine
{
    Task LoadAsync(string[] filePaths, CancellationToken cancellationToken);
    void Play();
    void Pause();
    void Stop();
    void SetVolume(int trackIndex, float volume);
    void SetPan(int trackIndex, float pan);
}
```

**Features**:
- 32-track support
- Real-time VU meters
- Per-track volume/pan
- Solo/Mute

---

### 3. IVideoEngine

**Purpose**: Frame-accurate video playback

**Implementation**: `LibVLCVideoEngine`

```csharp
public interface IVideoEngine
{
    Task LoadAsync(string filePath, CancellationToken cancellationToken);
    void Play();
    void Pause();
    void SeekToFrame(long frameNumber);
    void StepForward();
    void StepBackward();
}
```

**Features**:
- Frame-accurate seeking
- Playback rate control
- Frame stepping
- Timecode display

---

### 4. ISyncEngine

**Purpose**: Audio/video synchronization

**Implementation**: `FFmpegSyncEngine`

```csharp
public interface ISyncEngine
{
    Task<SyncResult> SynchronizeAsync(
        string videoFilePath, 
        string audioFilePath, 
        CancellationToken cancellationToken);
    
    Task<string?> DetectTimecodeAsync(
        string videoFilePath, 
        CancellationToken cancellationToken);
}
```

**Algorithm**: FFT-based cross-correlation

**Accuracy**: ±1 frame @ 24fps

---

### 5. ITranscodeEngine

**Purpose**: Media format conversion

**Implementation**: `FFmpegTranscodeEngine`

```csharp
public interface ITranscodeEngine
{
    Task<TranscodeResult> TranscodeAsync(
        string inputPath,
        string outputPath,
        TranscodePreset preset,
        IProgress<TranscodeProgress>? progress,
        CancellationToken cancellationToken);
    
    TranscodePreset[] GetAvailablePresets();
}
```

**Presets**: ProRes, DNxHD, H.264, H.265

---

### 6. IReportEngine

**Purpose**: PDF report generation

**Implementation**: `QuestPDFReportEngine`

```csharp
public interface IReportEngine
{
    Task<string> GenerateCameraReportAsync(
        ReportData reportData,
        string outputPath,
        CancellationToken cancellationToken);
    
    Task<string> GenerateSoundReportAsync(
        ReportData reportData,
        string outputPath,
        CancellationToken cancellationToken);
}
```

**Library**: QuestPDF

---

## Adding a New Module

### Step 1: Create ViewModel

```csharp
// src/Veriflow.UI/ViewModels/MyModuleViewModel.cs
public partial class MyModuleViewModel : ViewModelBase
{
    private readonly IMyService _myService;
    
    public MyModuleViewModel(IMyService myService)
    {
        _myService = myService;
        StatusMessage = "Ready";
    }
    
    [RelayCommand]
    private async Task DoSomethingAsync()
    {
        IsBusy = true;
        try
        {
            await _myService.DoWorkAsync();
            StatusMessage = "Complete";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### Step 2: Create View

```xml
<!-- src/Veriflow.UI/Views/MyModuleView.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             x:Class="Veriflow.UI.Views.MyModuleView">
    <StackPanel>
        <TextBlock Text="{Binding StatusMessage}" />
        <Button Content="Do Something" 
                Command="{Binding DoSomethingCommand}" />
    </StackPanel>
</UserControl>
```

### Step 3: Register in DI

```csharp
// src/Veriflow.UI/ServiceConfiguration.cs
services.AddTransient<MyModuleViewModel>();
```

### Step 4: Add Navigation

```csharp
// src/Veriflow.UI/ViewModels/MainWindowViewModel.cs
[RelayCommand]
private async Task NavigateToMyModuleAsync()
{
    CurrentViewModel = _serviceProvider.GetRequiredService<MyModuleViewModel>();
}
```

---

## Testing

### Unit Test Example

```csharp
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
}
```

### Integration Test Example

```csharp
[Fact]
public async Task TranscodeAsync_WithValidInput_ShouldSucceed()
{
    // Arrange
    var engine = new FFmpegTranscodeEngine();
    var preset = engine.GetAvailablePresets()
        .First(p => p.Id == "h264_high");
    
    // Act
    var result = await engine.TranscodeAsync(
        inputPath,
        outputPath,
        preset,
        null,
        CancellationToken.None
    );
    
    // Assert
    result.Success.Should().BeTrue();
    File.Exists(outputPath).Should().BeTrue();
}
```

---

## Performance Optimization

### 1. Async/Await Best Practices

```csharp
// ✅ Good
public async Task<MediaInfo> GetMediaInfoAsync(string path)
{
    return await Task.Run(() => ExtractMetadata(path));
}

// ❌ Bad (blocking)
public MediaInfo GetMediaInfo(string path)
{
    return ExtractMetadata(path); // Blocks UI thread
}
```

### 2. Cancellation Support

```csharp
public async Task ProcessAsync(CancellationToken cancellationToken)
{
    foreach (var file in files)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ProcessFileAsync(file);
    }
}
```

### 3. Progress Reporting

```csharp
public async Task TranscodeAsync(
    IProgress<double> progress)
{
    for (int i = 0; i < totalFrames; i++)
    {
        ProcessFrame(i);
        progress?.Report((double)i / totalFrames * 100);
    }
}
```

---

## Code Style

### Naming Conventions

- **Classes**: PascalCase (`MediaService`)
- **Methods**: PascalCase (`GetMediaInfoAsync`)
- **Properties**: PascalCase (`FileName`)
- **Fields**: _camelCase (`_mediaService`)
- **Constants**: PascalCase (`MaxTracks`)

### Async Methods

- Always suffix with `Async`
- Return `Task` or `Task<T>`
- Accept `CancellationToken`

```csharp
public async Task<MediaInfo> GetMediaInfoAsync(
    string filePath,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

---

## Building & Deployment

### Debug Build

```bash
dotnet build
```

### Release Build

```bash
dotnet build -c Release
```

### Publish (Self-Contained)

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

---

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

### Code Review Checklist

- [ ] Tests pass
- [ ] Code coverage maintained
- [ ] Documentation updated
- [ ] No compiler warnings
- [ ] Follows code style

---

## Resources

- [.NET 8 Documentation](https://docs.microsoft.com/dotnet/)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [NAudio Documentation](https://github.com/naudio/NAudio)
- [LibVLCSharp Documentation](https://code.videolan.org/videolan/LibVLCSharp)
- [QuestPDF Documentation](https://www.questpdf.com/)

---

**Version**: 3.0.0-beta  
**Last Updated**: 2025-12-23
