# Veriflow 3.0 - API Reference

## Core Interfaces

### IOffloadService
Secure file offloading with dual-destination copying and MHL verification.

**Methods:**
- `Task<OffloadResult> OffloadAsync(string sourcePath, string destinationA, string destinationB, IProgress<OffloadProgress>? progress, CancellationToken ct)`
- `Task<VerifyResult> VerifyAsync(string sourcePath, string destinationA, string destinationB, IProgress<OffloadProgress>? progress, CancellationToken ct)`
- `Task GenerateMhlAsync(string directoryPath, string outputPath)`
- `Task<List<CopyHistoryEntry>> GetHistoryAsync()`
- `Task AddHistoryEntryAsync(CopyHistoryEntry entry)`
- `Task ClearHistoryAsync()`

### IMediaService
Media metadata extraction and thumbnail generation using FFmpeg.

**Methods:**
- `Task<MediaMetadata> GetMediaInfoAsync(string filePath, CancellationToken ct)`
- `Task<string> GenerateThumbnailAsync(string videoPath, string outputPath, double timePosition, CancellationToken ct)`
- `Task<string> GenerateWaveformAsync(string audioPath, string outputPath, CancellationToken ct)`
- `Task<string> CalculateHashAsync(string filePath, CancellationToken ct)`
- `Task<bool> VerifyHashAsync(string filePath, string expectedHash, CancellationToken ct)`

### IAudioEngine
Professional OpenAL Soft audio engine supporting 192kHz, 32-bit float, 32 tracks.

**Methods:**
- `Task LoadTrackAsync(int trackIndex, string filePath, CancellationToken ct)`
- `void UnloadTrack(int trackIndex)`
- `void Play()`, `void Pause()`, `void Stop()`
- `void Seek(double position)`
- `void SetTrackVolume(int trackIndex, float volume)`
- `void SetTrackPan(int trackIndex, float pan)`
- `void SetTrackMute(int trackIndex, bool muted)`
- `void SetTrackSolo(int trackIndex, bool solo)`
- `double GetPosition()`, `double GetDuration()`
- `(float left, float right) GetTrackPeaks(int trackIndex)`
- `(float left, float right) GetMasterPeaks()`

### IVideoEngine
LibVLC-based video player with frame-accurate seeking.

**Methods:**
- `Task LoadVideoAsync(string filePath, CancellationToken ct)`
- `void UnloadVideo()`
- `void Play()`, `void Pause()`, `void Stop()`
- `void Seek(double position)`
- `void StepForward()`, `void StepBackward()`
- `void SetPlaybackRate(float rate)`
- `void SetVolume(int volume)`
- `double GetPosition()`, `double GetDuration()`
- `long GetFrameNumber()`, `double GetFrameRate()`
- `uint GetWidth()`, `uint GetHeight()`

### ISyncEngine
FFmpeg-based synchronization using waveform correlation.

**Methods:**
- `Task<SyncResult> SynchronizeAsync(string videoPath, string audioPath, IProgress<SyncProgress>? progress, CancellationToken ct)`
- `Task<string> DetectTimecodeAsync(string filePath, CancellationToken ct)`
- `Task<double> CalculateAudioOffsetAsync(string audio1, string audio2, CancellationToken ct)`
- `Task<double> VerifySyncAccuracyAsync(string videoPath, string audioPath, double offset, CancellationToken ct)`

### ITranscodeEngine
FFmpeg-based transcoding with 40+ professional codecs.

**Methods:**
- `List<TranscodePreset> GetAvailablePresets()`
- `Task<TranscodeResult> TranscodeAsync(TranscodeJob job, IProgress<TranscodeProgress>? progress, CancellationToken ct)`
- `Task<bool> ValidateInputAsync(string inputPath, CancellationToken ct)`
- `Task<long> EstimateOutputSizeAsync(string inputPath, TranscodePreset preset, CancellationToken ct)`

### IReportEngine
PDF report generation with EDL/ALE export.

**Methods:**
- `Task<string> GenerateCameraReportAsync(ReportData data, string outputPath, CancellationToken ct)`
- `Task<string> GenerateSoundReportAsync(ReportData data, string outputPath, CancellationToken ct)`
- `Task<byte[]> PreviewReportAsync(ReportData data, ReportType type, CancellationToken ct)`
- `Task<string> GenerateEdlAsync(List<ReportClip> clips, string outputPath, string title, double frameRate, CancellationToken ct)`
- `Task<string> GenerateAleAsync(List<ReportClip> clips, string outputPath, CancellationToken ct)`

### ISessionService
Session management for .vfsession files.

**Methods:**
- `void NewSession()`
- `Task LoadSessionAsync(string filePath)`
- `Task SaveSessionAsync()`
- `void MarkAsModified()`
- `Session CurrentSession { get; }`

### IDialogService
UI dialog services for file/folder selection.

**Methods:**
- `Task<string?> ShowFolderPickerAsync(string title)`
- `Task<string[]?> ShowFilePickerAsync(string title, bool allowMultiple, string[]? filters)`
- `Task<string?> ShowSaveFileDialogAsync(string title, string? defaultFileName, string[]? filters)`
- `Task ShowMessageBoxAsync(string title, string message)`
- `Task<bool> ShowConfirmationAsync(string title, string message)`

## Core Models

### CopyHistoryEntry
```csharp
public class CopyHistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string SourcePath { get; set; }
    public string DestinationAPath { get; set; }
    public string DestinationBPath { get; set; }
    public int FilesCount { get; set; }
    public long TotalBytes { get; set; }
    public TimeSpan Duration { get; set; }
    public string MhlPathA { get; set; }
    public string MhlPathB { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string FormattedSize { get; }
    public string FormattedDuration { get; }
}
```

### LoggedClip
```csharp
public class LoggedClip
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string Scene { get; set; }
    public string Take { get; set; }
    public string TimecodeIn { get; set; }
    public string TimecodeOut { get; set; }
    public string Duration { get; set; }
    public string Notes { get; set; }
    public int Rating { get; set; }
    public bool IsGood { get; set; }
    public DateTime LoggedAt { get; set; }
    public List<string> Markers { get; set; }
}
```

### MediaMetadata
```csharp
public class MediaMetadata
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string Duration { get; set; }
    public string VideoCodec { get; set; }
    public string AudioCodec { get; set; }
    public string Resolution { get; set; }
    public string FrameRate { get; set; }
    public string BitRate { get; set; }
    public Dictionary<string, string> CustomMetadata { get; set; }
}
```

## ViewModels

All ViewModels inherit from `ViewModelBase` which implements `INotifyPropertyChanged`.

### MainWindowViewModel
- Properties: `CurrentProfile`, `CurrentPage`, `WindowTitle`, `StatusMessage`, `CurrentPageViewModel`
- Commands: `ToggleProfileCommand`, `NavigateToPageCommand`, `NewSessionCommand`, `OpenSessionCommand`, `SaveSessionCommand`, `ShowAboutCommand`, `ExitCommand`

### OffloadViewModel
- Properties: `SourceFolder`, `DestinationA`, `DestinationB`, `IsOffloadMode`, `Progress`, `CurrentFile`, `LogText`, `History`, `IsHistoryVisible`
- Commands: `SelectSourceFolderCommand`, `SelectDestinationACommand`, `SelectDestinationBCommand`, `StartOffloadCommand`, `CancelOffloadCommand`, `ToggleModeCommand`, `ClearLogCommand`, `LoadHistoryCommand`, `ToggleHistoryCommand`, `ClearHistoryCommand`

### PlayerViewModel
- Properties: `Tracks`, `Position`, `Duration`, `IsPlaying`, `MasterPeakLeft`, `MasterPeakRight`, `MasterVolume`, `PlaybackRate`, `LoggedClips`, `CurrentFilePath`
- Commands: `LoadTrackCommand`, `UnloadTrackCommand`, `PlayCommand`, `PauseCommand`, `StopCommand`, `SeekCommand`, `PlayPauseToggleCommand`, `ShuttleBackwardCommand`, `ShuttleForwardCommand`, `StepForwardCommand`, `StepBackwardCommand`, `LogCurrentClipCommand`, `RemoveLoggedClipCommand`, `ClearLoggedClipsCommand`

### MediaViewModel
- Properties: `MediaFiles`, `SelectedFile`, `ThumbnailPath`, `Metadata`
- Commands: `BrowseFolderCommand`, `RefreshCommand`, `GenerateThumbnailCommand`, `PreviewCommand`

### SyncViewModel
- Properties: `VideoFiles`, `AudioFiles`, `SyncResults`, `Progress`
- Commands: `AddVideoCommand`, `AddAudioCommand`, `SyncByTimecodeCommand`, `SyncByWaveformCommand`, `ExportCommand`

### TranscodeViewModel
- Properties: `InputFiles`, `Presets`, `SelectedPreset`, `Queue`, `Progress`
- Commands: `AddFilesCommand`, `StartTranscodeCommand`, `CancelCommand`, `ClearQueueCommand`

### ReportsViewModel
- Properties: `ReportData`, `Clips`, `ReportType`
- Commands: `GenerateCameraReportCommand`, `GenerateSoundReportCommand`, `ExportEdlCommand`, `ExportAleCommand`, `PreviewCommand`

## Keyboard Shortcuts

### Global
- **F1**: Navigate to OFFLOAD
- **F2**: Navigate to MEDIA
- **F3**: Navigate to PLAYER
- **F4**: Navigate to SYNC
- **F5**: Navigate to TRANSCODE
- **F6**: Navigate to REPORTS
- **Ctrl+N**: New Session
- **Ctrl+O**: Open Session
- **Ctrl+S**: Save Session
- **Ctrl+Q**: Exit

### Player
- **Space**: Play/Pause Toggle
- **J**: Shuttle Backward
- **K**: Pause
- **L**: Shuttle Forward
- **Left Arrow**: Step Backward
- **Right Arrow**: Step Forward

## File Formats

### .vfsession (JSON)
Session file containing project state, current page, profile mode, and logged clips.

### .mhl (XML)
Media Hash List 1.1 format with xxHash64 checksums.

### .edl (Text)
CMX 3600 Edit Decision List format.

### .ale (Tab-delimited)
Avid Log Exchange format.

### copy_history.json (JSON)
Copy operation history stored in `%APPDATA%\Veriflow\`.
