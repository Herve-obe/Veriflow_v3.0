# Veriflow 3.0

Professional cross-platform DIT tool for video and audio production.

## Features

- **OFFLOAD**: Secure dual-destination file copying with MHL 1.1 verification
- **MEDIA**: File browser and media library management
- **PLAYER**: Professional audio/video playback with metadata
- **SYNC**: Multi-camera and multi-track synchronization
- **TRANSCODE**: Format conversion and encoding
- **REPORTS**: Quality control reports and EDL generation

## Technology Stack

- **UI**: Avalonia 11.x (cross-platform)
- **Runtime**: .NET 8.0
- **Audio**: MiniAudio (HD multitrack, 192kHz, 32-bit)
- **Video**: LibVLCSharp
- **Media Processing**: FFmpeg.AutoGen
- **PDF**: QuestPDF
- **FFT**: MathNet.Numerics

## Supported Platforms

- Windows 10/11 (x64)
- macOS 12+ (Intel + Apple Silicon)
- Linux (Ubuntu 22.04+)

## Development

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 / Rider / VS Code

### Build

```bash
dotnet restore
dotnet build
```

### Run

```bash
dotnet run --project src/Veriflow.UI/Veriflow.UI.csproj
```

## License

Commercial - © 2025 Veriflow

## Documentation

See `/docs` folder for detailed documentation.
