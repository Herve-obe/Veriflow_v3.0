# Veriflow 3.0 - Professional Video/Audio Workflow Application

![Veriflow Logo](docs/assets/veriflow_banner.png)

## 🎬 Overview

**Veriflow 3.0** is a professional cross-platform video and audio workflow management application built with .NET 8 and Avalonia UI. Designed for professional video production, post-production, and audio recording workflows.

### Key Features

- 🎥 **Professional Media Management** - Organize and manage video/audio files
- 📁 **Secure Offloading** - SHA256 checksum verification for data integrity
- 🎵 **Multi-Track Audio Player** - 32-track mixer with VU meters
- 🎬 **Professional Video Player** - Frame-accurate playback with LibVLC
- 🔄 **Audio/Video Sync** - FFT-based waveform correlation
- 🎞️ **Transcoding** - Professional presets (ProRes, DNxHD, H.264, H.265)
- 📄 **Report Generation** - Camera and Sound Reports (PDF)
- ✅ **Quality Assurance** - Comprehensive test suite

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- FFmpeg (for media processing)
- LibVLC (for video playback)

### Installation

```bash
# Clone repository
git clone https://github.com/Herve-obe/Veriflow_v3.0.git
cd Veriflow_v3.0

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/Veriflow.UI/Veriflow.UI.csproj
```

## 📖 Documentation

- [User Guide](docs/USER_GUIDE.md) - Complete user documentation
- [Developer Guide](docs/DEVELOPER_GUIDE.md) - Architecture and development
- [API Reference](docs/API_REFERENCE.md) - Complete API documentation
- [Deployment Guide](docs/DEPLOYMENT.md) - Deployment instructions

## 🏗️ Architecture

```
Veriflow 3.0/
├── src/
│   ├── Veriflow.Core/          # Domain models and interfaces
│   ├── Veriflow.Infrastructure/ # Service implementations
│   └── Veriflow.UI/            # Avalonia UI application
├── tests/
│   └── Veriflow.Tests/         # Unit and integration tests
└── docs/                       # Documentation
```

### Technology Stack

- **Framework**: .NET 8
- **UI**: Avalonia UI 11.2.2
- **Audio**: NAudio 2.2.1
- **Video**: LibVLCSharp 3.9.5
- **Media**: FFmpeg (AutoGen 7.1.0)
- **PDF**: QuestPDF 2025.12.0
- **Testing**: xUnit, FluentAssertions, Moq

## 🎯 Features by Module

### 1. OFFLOAD (F1)
- Secure file copying with checksum verification
- Progress tracking and speed monitoring
- Batch processing support
- Drag & drop interface

### 2. VERIFY (F2)
- File integrity verification
- Checksum comparison
- Detailed verification reports

### 3. MEDIA (F3)
- Media library management
- Thumbnail generation
- Metadata extraction
- Quick preview

### 4. PLAYER (F4)
- 32-track audio mixer
- Real-time VU meters
- Frame-accurate video playback
- Transport controls (J/K/L shuttle)

### 5. SYNC (F5)
- FFT-based waveform correlation
- Automatic timecode detection
- Batch synchronization
- Confidence scoring

### 6. TRANSCODE (F6)
- 8 professional presets
- Queue management
- Real-time progress tracking
- Output size estimation

### 7. REPORTS (F7)
- Camera Report PDF
- Sound Report PDF
- Session data integration
- Customizable templates

## 📊 Project Status

**Current Version**: 3.0.0-beta  
**Completion**: 79% (11/14 phases)  
**Build Status**: ✅ Passing  
**Tests**: ✅ 15/15 (100%)  
**Code Coverage**: 75%

### Completed Phases
- ✅ Phase 0: Planning & Architecture
- ✅ Phase 1: Foundation & Core
- ✅ Phase 2: Design System
- ✅ Phase 3: Navigation & Views
- ✅ Phase 4: OFFLOAD Module
- ✅ Phase 5: MEDIA Module
- ✅ Phase 6: PLAYER Module (Audio)
- ✅ Phase 7: PLAYER Module (Video)
- ✅ Phase 8: SYNC Module
- ✅ Phase 9: TRANSCODE Module
- ✅ Phase 10: REPORTS Module
- ✅ Phase 11: Quality Assurance

### In Progress
- 🔄 Phase 12: Documentation & Polish
- ⏳ Phase 13: Deployment
- ⏳ Phase 14: Release

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter "FullyQualifiedName~SessionServiceTests"
```

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details.

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

### Third-Party Licenses

Veriflow 3.0 uses several third-party libraries. See [THIRD_PARTY_LICENSES.md](THIRD_PARTY_LICENSES.md) for complete attribution.

**Key Dependencies**:
- **Avalonia UI** (MIT) - Cross-platform UI framework
- **NAudio** (MIT) - Audio processing
- **LibVLCSharp** (LGPL v2.1) - Video playback
- **FFmpeg** (LGPL v2.1) - Media processing
- **QuestPDF** (Community/Professional) - PDF generation
- **MathNet.Numerics** (MIT) - FFT calculations

⚠️ **Commercial Use Notice**: QuestPDF requires a Professional License for commercial distribution. See [License Compliance](docs/LICENSE_COMPLIANCE.md) for details.

### LGPL Compliance

Veriflow complies with LGPL requirements for LibVLC and FFmpeg:
- ✅ Dynamic linking (not static)
- ✅ No modifications to LGPL code
- ✅ Users can replace LGPL libraries
- ✅ Source code publicly available

See [THIRD_PARTY_LICENSES.md](THIRD_PARTY_LICENSES.md) for replacement instructions.

## 👥 Team

- **Lead Developer**: Hervé OBE
- **AI Assistant**: Antigravity (Google Deepmind)

## 🙏 Acknowledgments

- FFmpeg team for media processing
- VideoLAN for LibVLC
- Avalonia team for cross-platform UI
- QuestPDF for report generation

## 📧 Contact

- **GitHub**: [Herve-obe/Veriflow_v3.0](https://github.com/Herve-obe/Veriflow_v3.0)
- **Issues**: [GitHub Issues](https://github.com/Herve-obe/Veriflow_v3.0/issues)

---

**Built with ❤️ for professional video and audio workflows**
