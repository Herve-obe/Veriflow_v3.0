# Veriflow 3.0 - User Guide

**Version**: 1.0.0  
**Last Updated**: 2025-12-23  
**Build**: Production Release

---

## Table of Contents
1. [Getting Started](#getting-started)
2. [Interface Overview](#interface-overview)
3. [Module Guides](#module-guides)
4. [Keyboard Shortcuts](#keyboard-shortcuts)
5. [Advanced Features](#advanced-features)
6. [Troubleshooting](#troubleshooting)

---

## Getting Started

### First Launch

1. **Start Veriflow**: Double-click the Veriflow icon or run from command line
2. **Select Profile**: Choose Video or Audio profile (top-left toggle)
3. **Create Session**: File → New Session or `Ctrl+N`

### Profile Modes

#### Video Profile (Blue)
- Camera Report generation
- Video-specific features
- Frame-accurate playback
- EDL/ALE export

#### Audio Profile (Red)
- Sound Report generation
- Multi-track audio mixing (32 tracks)
- Waveform synchronization
- WAV metadata editing (BWF/iXML)

---

## Interface Overview

### Main Window

### Navigation
- **F1-F6**: Switch between modules
- **Ctrl+N**: New session
- **Ctrl+O**: Open session (.vfsession files)
- **Ctrl+S**: Save session
- **Ctrl+Q**: Quit application

---

## Module Guides

### 1. OFFLOAD (F1) - Secure Dual-Destination File Transfer

**Purpose**: Copy files to two destinations simultaneously with MHL verification

**Workflow**:
1. **Select Source**: Click "Browse..." or drag & drop folder
2. **Select Destination A**: Click "Browse..." or drag & drop folder
3. **Select Destination B**: Click "Browse..." or drag & drop folder
4. Click **"Start Offload"**
5. Wait for completion and MHL generation

**Features**:
- ✅ Dual-destination copying (A + B simultaneously)
- ✅ MHL 1.1 hash file generation (xxHash64)
- ✅ **Drag & Drop support** (3 zones)
- ✅ **Copy History tracking** (last 100 operations)
- ✅ Progress tracking with speed monitoring
- ✅ Verify mode for hash validation

**New in v3.0**:
- **Drag & Drop**: Drop folders directly on Source, Dest A, or Dest B fields
- **Copy History**: View history with "View History" button
  - Shows: Date/Time, Paths, File Count, Size, Duration, Status
  - Persistent storage in `%APPDATA%\Veriflow\copy_history.json`
  - Clear history with confirmation

**Tips**:
- Always verify MHL files match
- Keep originals until both destinations verified
- Use fast, reliable drives (SSD recommended)
- Monitor speed for performance issues

---

### 2. MEDIA (F2) - Media Library & Metadata

**Purpose**: Browse media files and edit metadata

**Workflow**:
1. Navigate to media folder
2. View thumbnails and metadata
3. Click to preview
4. Double-click to open in PLAYER

**Features**:
- Thumbnail grid view
- FFmpeg metadata extraction
- Waveform generation
- Hash calculation (xxHash64)
- **WAV metadata editing** (BWF + iXML)

**New in v3.0 - WAV Metadata Editing**:

**Supported Formats**:
- **BWF (Broadcast Wave Format)**: Professional broadcast metadata
- **iXML**: Production metadata (Scene, Take, Notes)

**BWF Fields**:
- Description (256 chars)
- Originator (32 chars)
- Originator Reference (32 chars)
- Origination Date (YYYY-MM-DD)
- Origination Time (HH:MM:SS)
- Time Reference (samples)

**iXML Fields**:
- Project
- Scene
- Take
- Tape
- Circled Take
- Notes

**Usage**:
1. Select WAV file
2. Right-click → "Edit Metadata"
3. Modify fields
4. Click "Save"

---

### 3. PLAYER (F3) - Multi-Track Audio/Video Playback

#### Audio Player (32 Tracks)

**Features**:
- 32-track mixer with OpenAL Soft engine
- Real-time VU meters (per-track + master)
- Volume/Pan controls per track
- Solo/Mute per track
- **Logged Clips management**
- **Keyboard shortcuts** (J/K/L shuttle)

**New in v3.0 - Keyboard Shortcuts**:
- `Space`: Play/Pause toggle
- `J`: Shuttle backward (2x, 4x, 8x, pause)
- `K`: Pause
- `L`: Shuttle forward (2x, 4x, 8x, 1x)
- `Left Arrow`: Step backward
- `Right Arrow`: Step forward

**New in v3.0 - Logged Clips**:

**Purpose**: Track and log clips during playback

**Features**:
- Log current clip with timecode
- Scene/Take/Notes metadata
- Rating system (0-5 stars)
- Markers support
- Export to EDL/ALE

**Usage**:
1. Load audio file
2. Play to desired position
3. Click **"Log Current Clip"**
4. Clip added to list with:
   - File name
   - Timecode In/Out
   - Duration
   - Timestamp

**Logged Clips Panel**:
- View all logged clips
- Remove individual clips (✕ button)
- Clear all clips
- Counter shows total clips logged

#### Video Player

**Features**:
- LibVLC-based playback
- Frame-accurate seeking
- Timecode display
- Frame stepping
- Playback rate control (0.25x to 4x)

**Controls**:
- `Space`: Play/Pause
- `Left/Right`: Frame step
- `Up/Down`: Speed adjust
- `F`: Fullscreen

---

### 4. SYNC (F4) - Audio/Video Synchronization

**Purpose**: Synchronize external audio with video using FFmpeg

**Workflow**:
1. Add video files to Video Pool
2. Add audio files to Audio Pool
3. Select video and audio
4. Click **"Sync by Waveform"** or **"Sync by Timecode"**
5. Review offset and confidence
6. Export synchronized pairs

**Methods**:
- **Waveform Correlation**: FFT-based (most accurate, slower)
- **Timecode**: Metadata-based (fastest, requires timecode)

**Confidence Levels**:
- 90-100%: Excellent (auto-accept)
- 80-90%: Good (review recommended)
- 70-80%: Acceptable (manual review)
- <70%: Manual adjustment needed

**Tips**:
- Use clean audio for best correlation
- Sync early in workflow
- Verify sync manually in PLAYER
- Save sync data to session

---

### 5. TRANSCODE (F5) - Format Conversion

**Purpose**: Convert media to different formats using FFmpeg

**Workflow**:
1. Select preset from dropdown
2. Add files to queue
3. Click **"Start Queue"**
4. Monitor progress

**Available Presets** (40+ codecs):

**Professional Editing**:
- ProRes 422 (10-bit, 1080p/4K)
- ProRes 422 HQ (high quality)
- ProRes 4444 (with alpha)
- DNxHD 1080p (Avid workflows)
- DNxHR (4K+ workflows)

**Delivery**:
- H.264 High (high quality, compatible)
- H.265 HEVC (efficient compression)
- VP9 (web delivery)
- AV1 (next-gen compression)

**Audio**:
- WAV PCM (uncompressed)
- FLAC (lossless compression)
- AAC (high quality lossy)
- Opus (low bitrate)

**Features**:
- Queue management
- Progress tracking
- Output size estimation
- Batch processing
- Cancel/Resume support

**Tips**:
- Use ProRes for editing (large files, fast decode)
- Use H.264/H.265 for delivery (small files, slow decode)
- Check output size before starting
- Test with one file first

---

### 6. REPORTS (F6) - PDF Generation & EDL/ALE Export

**Purpose**: Generate production reports and export lists

#### Camera Report (Video Profile)

**Fields**:
- Project information (Name, Date, Location, Production Company)
- Crew (Director, DOP, Camera Operator, Data Manager)
- Equipment (Camera Model, Lens Info)
- Clips table (Scene, Take, Timecode, Duration, Resolution, FPS, Good)
- Notes

#### Sound Report (Audio Profile)

**Fields**:
- Project information
- Crew (Sound Recordist, Boom Operator)
- Equipment (Recorder Model, Microphone, Timecode Rate, Bit Depth)
- Clips table (Scene, Take, Timecode, Duration, Good)
- Notes

**Workflow**:
1. Fill in project details
2. Add clips to table
3. Add notes
4. Click **"Generate Camera Report"** or **"Generate Sound Report"**
5. Save PDF

**New in v3.0 - EDL/ALE Export**:

#### EDL (Edit Decision List) - CMX 3600 Format

**Purpose**: Export clip list for video editing software

**Compatible with**: DaVinci Resolve, Premiere Pro, Final Cut Pro, Avid Media Composer

**Usage**:
1. Add clips to report
2. Click **"Export EDL"**
3. Choose location
4. Import in DaVinci Resolve, Premiere Pro, etc.

#### ALE (Avid Log Exchange) - Avid Format

**Purpose**: Export clip metadata for Avid Media Composer

**Compatible with**: Avid Media Composer, Pro Tools

**Usage**:
1. Add clips to report
2. Click **"Export ALE"**
3. Choose location
4. Import in Avid Media Composer

**Supported Fields**:
- Name, Tape, Scene, Take
- Start, End, Duration
- FPS, Resolution, Codec
- Notes

---

## Keyboard Shortcuts

### Global
- `F1`: OFFLOAD
- `F2`: MEDIA
- `F3`: PLAYER
- `F4`: SYNC
- `F5`: TRANSCODE
- `F6`: REPORTS
- `Ctrl+N`: New session
- `Ctrl+O`: Open session
- `Ctrl+S`: Save session
- `Ctrl+Q`: Quit

### Player (New in v3.0)
- `Space`: Play/Pause toggle
- `J`: Shuttle backward (2x, 4x, 8x, pause)
- `K`: Pause
- `L`: Shuttle forward (2x, 4x, 8x, 1x)
- `Left Arrow`: Step backward
- `Right Arrow`: Step forward

---

## Advanced Features

### Session Management

**File Format**: `.vfsession` (JSON)

**Contents**:
- Project metadata
- Current page
- Profile mode (Video/Audio)
- Logged clips
- Sync data
- Transcode queue

**Usage**:
- Save: `Ctrl+S` or File → Save Session
- Open: `Ctrl+O` or File → Open Session
- Auto-save: Every 5 minutes (if modified)

### Copy History

**Location**: `%APPDATA%\Veriflow\copy_history.json`

**Stored Data**:
- Timestamp
- Source → Destination A/B paths
- File count, Total size
- Duration
- MHL paths
- Success/Error status

**Retention**: Last 100 operations (FIFO)

**Usage**:
1. Go to OFFLOAD (F1)
2. Click **"View History"**
3. Review past operations
4. Click **"Clear History"** to delete (with confirmation)

### MHL (Media Hash List) 1.1

**Purpose**: Verify file integrity

**Format**: XML with xxHash64 checksums

**Contents**:
- File paths (relative)
- File sizes
- Last modification dates
- xxHash64 hashes

**Generation**: Automatic after each offload

**Verification**:
1. Switch to VERIFY mode
2. Select source and destinations
3. Click "Verify"
4. Review results (✅ match, ❌ mismatch)

---

## Troubleshooting

### Common Issues

#### "FFmpeg not found"
**Solution**: Install FFmpeg and add to PATH
```bash
# Windows (with Chocolatey)
choco install ffmpeg

# macOS (with Homebrew)
brew install ffmpeg

# Linux (Ubuntu/Debian)
sudo apt install ffmpeg
```

**Verify**:
```bash
ffmpeg -version
ffprobe -version
```

#### "LibVLC not found"
**Solution**: Install VLC Media Player
- Download from: https://www.videolan.org/
- Install to default location
- Restart Veriflow

#### "Checksum mismatch"
**Causes**:
- File corruption during transfer
- Disk errors
- Incomplete copy
- Different file versions

**Solution**:
1. Re-copy the file
2. Check disk health (SMART status)
3. Use different destination drive
4. Verify source file integrity

#### "Sync confidence low"
**Causes**:
- Different audio sources (camera vs recorder)
- Heavy background noise
- Clipped audio
- Different sample rates

**Solution**:
1. Try manual offset adjustment
2. Use timecode sync if available
3. Check audio quality in PLAYER
4. Re-record if necessary

#### "Drag & Drop not working"
**Solution**:
- Ensure dropping **folders**, not files
- Check folder permissions
- Try "Browse..." button instead
- Restart application

#### "WAV metadata not saving"
**Causes**:
- File is read-only
- File is in use by another application
- Insufficient permissions

**Solution**:
1. Close file in other applications
2. Check file properties (remove read-only)
3. Run Veriflow as administrator (Windows)
4. Copy file to writable location

---

## Best Practices

### Offloading
1. ✅ Always use dual-destination (A + B)
2. ✅ Verify MHL files match
3. ✅ Keep originals until both verified
4. ✅ Use fast, reliable drives (SSD preferred)
5. ✅ Monitor transfer speed (should be >100 MB/s)
6. ✅ Review Copy History regularly

### Synchronization
1. ✅ Use clean audio for best correlation
2. ✅ Sync early in workflow (before editing)
3. ✅ Verify sync manually in PLAYER
4. ✅ Save sync data to session
5. ✅ Use timecode when available (fastest)
6. ✅ Check confidence levels (>80% recommended)

### Transcoding
1. ✅ Choose appropriate preset for use case
2. ✅ Test with one file first
3. ✅ Monitor disk space (ProRes is large)
4. ✅ Keep originals (never transcode only copy)
5. ✅ Use ProRes for editing, H.264/H.265 for delivery
6. ✅ Verify output quality

### Reports
1. ✅ Fill all required fields
2. ✅ Review before generating
3. ✅ Use consistent naming (Scene/Take)
4. ✅ Export EDL/ALE for editors
5. ✅ Archive reports with project
6. ✅ Log clips during shoot (not after)

### WAV Metadata
1. ✅ Edit metadata immediately after recording
2. ✅ Use consistent naming (Scene/Take)
3. ✅ Fill Originator field (your name/company)
4. ✅ Set correct Origination Date/Time
5. ✅ Use Time Reference for sync
6. ✅ Backup original files before editing

---

## Support

- **Documentation**: [docs/](../docs/)
- **API Reference**: [docs/API_REFERENCE.md](../docs/API_REFERENCE.md)
- **Deployment Guide**: [docs/DEPLOYMENT.md](../docs/DEPLOYMENT.md)
- **Issues**: [GitHub Issues](https://github.com/Herve-obe/Veriflow_v3.0/issues)
- **Email**: support@veriflow.com

---

## What's New in v3.0

### Major Features
- ✅ **Drag & Drop** in OFFLOAD (3 zones)
- ✅ **Copy History** with persistent storage
- ✅ **Logged Clips** in PLAYER
- ✅ **Keyboard Shortcuts** (J/K/L shuttle)
- ✅ **EDL/ALE Export** (CMX 3600 + Avid)
- ✅ **WAV Metadata Editing** (BWF + iXML)

### Improvements
- ✅ Session Open dialog with .vfsession filter
- ✅ MHL 1.1 with xxHash64 (faster than SHA256)
- ✅ Dual-destination offload (A + B simultaneously)
- ✅ 32-track audio mixer (OpenAL Soft)
- ✅ Complete documentation (API + Deployment)

### Bug Fixes
- ✅ Removed QuestPDF dependency (license issue)
- ✅ Fixed drag & drop validation
- ✅ Improved error handling
- ✅ Better progress tracking

---

**Version**: 1.0.0  
**Build**: Production Release  
**Last Updated**: 2025-12-23  
**License**: MIT (see EULA.md)
