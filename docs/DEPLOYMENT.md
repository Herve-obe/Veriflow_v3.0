# Veriflow 3.0 - Deployment Guide

## Build Instructions

### Prerequisites
- .NET 8 SDK
- FFmpeg binaries
- LibVLC binaries (Windows: included via NuGet)

### Development Build
```bash
cd "d:\ELEMENT\VERIFLOW 3.0"
dotnet restore
dotnet build
```

### Release Build
```bash
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Platform-Specific Builds

#### Windows (x64)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64
```

#### Linux (x64)
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64
```

#### macOS (x64)
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64
```

#### macOS (ARM64)
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64
```

## Runtime Dependencies

### FFmpeg
**Required for**: Media processing, transcoding, sync, metadata extraction

**Installation:**

**Windows:**
1. Download from https://ffmpeg.org/download.html
2. Extract to `C:\Program Files\FFmpeg`
3. Add `C:\Program Files\FFmpeg\bin` to PATH

**Linux:**
```bash
sudo apt-get install ffmpeg
```

**macOS:**
```bash
brew install ffmpeg
```

**Verification:**
```bash
ffmpeg -version
```

### LibVLC
**Required for**: Video playback

**Windows:**
- Included via `VideoLAN.LibVLC.Windows` NuGet package
- No manual installation required

**Linux:**
```bash
sudo apt-get install vlc libvlc-dev
```

**macOS:**
```bash
brew install --cask vlc
```

### OpenAL Soft
**Required for**: Audio playback

**Windows/Linux/macOS:**
- Included via `Silk.NET.OpenAL.Soft.Native` NuGet package
- Zero configuration required

## Directory Structure

```
Veriflow/
├── Veriflow.exe (or Veriflow on Linux/macOS)
├── appsettings.json
├── ffmpeg/ (optional, if bundled)
│   ├── ffmpeg.exe
│   └── ffprobe.exe
└── logs/ (created at runtime)
    └── crash_log_YYYYMMDD.txt
```

## Configuration

### Application Data
- **Windows**: `%APPDATA%\Veriflow\`
- **Linux**: `~/.config/Veriflow/`
- **macOS**: `~/Library/Application Support/Veriflow/`

**Files:**
- `copy_history.json` - Offload operation history
- `settings.json` - User preferences (future)

### Session Files
- Extension: `.vfsession`
- Format: JSON
- Location: User-defined

## Packaging

### Windows Installer (WiX)
```xml
<!-- Example Product.wxs -->
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="Veriflow 3.0" Version="3.0.0" 
           Manufacturer="Element Productions" UpgradeCode="YOUR-GUID">
    <Package InstallerVersion="200" Compressed="yes" />
    <Media Id="1" Cabinet="Veriflow.cab" EmbedCab="yes" />
    
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="Veriflow 3.0" />
      </Directory>
    </Directory>
    
    <Feature Id="ProductFeature" Title="Veriflow 3.0" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
    </Feature>
  </Product>
</Wix>
```

Build:
```bash
candle Product.wxs
light Product.wixobj -out Veriflow-3.0-Setup.msi
```

### Linux Package (.deb)
```bash
# Create package structure
mkdir -p veriflow-3.0/DEBIAN
mkdir -p veriflow-3.0/usr/local/bin
mkdir -p veriflow-3.0/usr/share/applications

# Copy binary
cp ./publish/linux-x64/Veriflow veriflow-3.0/usr/local/bin/

# Create control file
cat > veriflow-3.0/DEBIAN/control << EOF
Package: veriflow
Version: 3.0.0
Architecture: amd64
Maintainer: Element Productions
Description: Professional Video/Audio Workflow Application
Depends: ffmpeg, vlc
EOF

# Build package
dpkg-deb --build veriflow-3.0
```

### macOS Bundle (.app)
```bash
mkdir -p Veriflow.app/Contents/MacOS
mkdir -p Veriflow.app/Contents/Resources

# Copy binary
cp ./publish/osx-x64/Veriflow Veriflow.app/Contents/MacOS/

# Create Info.plist
cat > Veriflow.app/Contents/Info.plist << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>Veriflow</string>
    <key>CFBundleVersion</key>
    <string>3.0.0</string>
    <key>CFBundleExecutable</key>
    <string>Veriflow</string>
</dict>
</plist>
EOF

# Create DMG
hdiutil create -volname "Veriflow 3.0" -srcfolder Veriflow.app -ov -format UDZO Veriflow-3.0.dmg
```

## Environment Variables

### Optional Configuration
- `VERIFLOW_FFMPEG_PATH` - Custom FFmpeg binary path
- `VERIFLOW_LOG_LEVEL` - Logging level (Debug, Info, Warning, Error)
- `VERIFLOW_DATA_DIR` - Custom data directory

## Performance Tuning

### Recommended System Requirements
- **CPU**: Quad-core Intel i5 or equivalent
- **RAM**: 8 GB minimum, 16 GB recommended
- **Storage**: SSD for best performance
- **GPU**: Not required (CPU-based processing)

### Offload Performance
- Adjust `MaxConcurrentCopies` in `OffloadService.cs` (default: 4)
- Increase `BufferSize` for faster network copies (default: 1MB)

### Transcode Performance
- FFmpeg will use all available CPU cores by default
- For GPU acceleration, modify FFmpeg arguments in `FFmpegTranscodeEngine.cs`

## Troubleshooting

### FFmpeg Not Found
**Error**: "FFmpeg executable not found"
**Solution**: Ensure FFmpeg is in PATH or set `VERIFLOW_FFMPEG_PATH`

### LibVLC Initialization Failed
**Error**: "LibVLC initialization failed"
**Solution**: 
- Windows: Reinstall VLC or verify NuGet package
- Linux/macOS: Install VLC via package manager

### OpenAL Audio Issues
**Error**: "OpenAL device not found"
**Solution**: 
- Verify audio device is connected
- Check system audio settings
- Restart application

### Permission Errors
**Error**: "Access denied" during offload
**Solution**: Run as administrator (Windows) or with sudo (Linux/macOS)

## License Compliance

### LGPL Components
- **LibVLC**: LGPL v2.1 (dynamically linked)
- **FFmpeg**: LGPL v2.1 (external process)

**Compliance**: 
- ✅ Dynamic linking (not static)
- ✅ No modifications to LGPL code
- ✅ Users can replace libraries
- ✅ Source code publicly available

### MIT Components
- **PdfSharpCore**: MIT
- **MigraDoc**: MIT
- **Avalonia UI**: MIT
- **All other dependencies**: MIT or compatible

## Support

### Logs Location
- **Windows**: `%APPDATA%\Veriflow\logs\`
- **Linux**: `~/.config/Veriflow/logs/`
- **macOS**: `~/Library/Application Support/Veriflow/logs/`

### Crash Reports
Automatic crash logging via Serilog to `crash_log_YYYYMMDD.txt`

### GitHub Issues
https://github.com/Herve-obe/Veriflow_v3.0/issues

## Version History

### 3.0.0-beta (2025-12-23)
- Initial beta release
- All core modules implemented
- 85% feature complete
- Production-ready architecture

### Planned 3.0.0 (TBD)
- Complete test coverage (80%+)
- WAV metadata editing
- Additional documentation
- Performance optimizations
