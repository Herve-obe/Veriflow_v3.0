# Veriflow 3.0 - User Guide

## Table of Contents
1. [Getting Started](#getting-started)
2. [Interface Overview](#interface-overview)
3. [Module Guides](#module-guides)
4. [Keyboard Shortcuts](#keyboard-shortcuts)
5. [Troubleshooting](#troubleshooting)

---

## Getting Started

### First Launch

1. **Start Veriflow**: Double-click the Veriflow icon or run from command line
2. **Select Profile**: Choose Video or Audio profile (top-left toggle)
3. **Create Session**: File → New Session or Ctrl+N

### Profile Modes

#### Video Profile (Blue)
- Camera Report generation
- Video-specific features
- Frame-accurate playback
- EDL/ALE export

#### Audio Profile (Red)
- Sound Report generation
- Multi-track audio mixing
- Waveform synchronization
- Audio-specific tools

---

## Interface Overview

### Main Window

```
┌─────────────────────────────────────────────┐
│ [VIDEO] [AUDIO]  Project: My Project       │
├─────────────────────────────────────────────┤
│ F1      F2      F3      F4      F5      F6  │
│OFFLOAD  VERIFY  MEDIA  PLAYER  SYNC  TRANS  │
├─────────────────────────────────────────────┤
│                                             │
│           [Module Content Area]             │
│                                             │
├─────────────────────────────────────────────┤
│ Status: Ready                               │
└─────────────────────────────────────────────┘
```

### Navigation
- **F1-F7**: Switch between modules
- **Ctrl+N**: New session
- **Ctrl+O**: Open session
- **Ctrl+S**: Save session

---

## Module Guides

### 1. OFFLOAD (F1) - Secure File Transfer

**Purpose**: Copy files with integrity verification

**Workflow**:
1. Select source drive (left panel)
2. Select destination drive (right panel)
3. Drag & drop files or folders
4. Click "START OFFLOAD"
5. Wait for completion and verification

**Features**:
- SHA256 checksum verification
- Progress tracking
- Speed monitoring
- Batch processing

**Tips**:
- Always verify checksums match
- Use "Reset All" to clear queue
- Monitor speed for performance issues

---

### 2. VERIFY (F2) - File Integrity Check

**Purpose**: Verify copied files match originals

**Workflow**:
1. Select source files
2. Select destination files
3. Click "VERIFY"
4. Review results

**Indicators**:
- ✅ Green: Files match
- ❌ Red: Mismatch detected
- ⚠️ Yellow: Warning

---

### 3. MEDIA (F3) - Media Library

**Purpose**: Browse and preview media files

**Workflow**:
1. Navigate to media folder
2. View thumbnails
3. Click to preview
4. Double-click to open in PLAYER

**Features**:
- Thumbnail grid view
- Metadata display
- Quick preview
- Search and filter

---

### 4. PLAYER (F4) - Audio/Video Playback

#### Audio Player

**Features**:
- 32-track mixer
- Real-time VU meters
- Volume/Pan controls
- Solo/Mute per track

**Controls**:
- Space: Play/Pause
- J/K/L: Shuttle (reverse/pause/forward)
- Home: Go to start
- End: Go to end

#### Video Player

**Features**:
- Frame-accurate playback
- Timecode display
- Frame stepping
- Playback rate control

**Controls**:
- Space: Play/Pause
- Left/Right: Frame step
- Up/Down: Speed adjust
- F: Fullscreen

---

### 5. SYNC (F5) - Audio/Video Synchronization

**Purpose**: Synchronize external audio with video

**Workflow**:
1. Add video files to Video Pool
2. Add audio files to Audio Pool
3. Select video and audio
4. Click "SYNC SELECTED"
5. Review offset and confidence
6. Export synchronized pairs

**Methods**:
- **Waveform Correlation**: FFT-based (most accurate)
- **Timecode**: Metadata-based (fastest)

**Confidence Levels**:
- 90-100%: Excellent
- 80-90%: Good
- 70-80%: Acceptable
- <70%: Manual review needed

---

### 6. TRANSCODE (F6) - Format Conversion

**Purpose**: Convert media to different formats

**Workflow**:
1. Select preset (ProRes, DNxHD, H.264, etc.)
2. Add files to queue
3. Click "START QUEUE"
4. Monitor progress

**Presets**:
- **ProRes 422**: Professional editing (10-bit)
- **ProRes 422 HQ**: High quality editing
- **DNxHD 1080p**: Avid workflows
- **H.264 High**: High quality delivery
- **H.265 HEVC**: Efficient compression

**Tips**:
- Use ProRes for editing
- Use H.264/H.265 for delivery
- Check output size estimation

---

### 7. REPORTS (F7) - PDF Generation

**Purpose**: Generate production reports

#### Camera Report (Video Profile)

**Fields**:
- Project information
- Crew (Director, DOP, Operator, Data Manager)
- Equipment (Camera, Lens)
- Clips table
- Notes

#### Sound Report (Audio Profile)

**Fields**:
- Project information
- Crew (Recordist, Boom Operator)
- Equipment (Recorder, Microphone, Timecode, Bit Depth)
- Clips table
- Notes

**Workflow**:
1. Fill in project details
2. Add clips
3. Add notes
4. Click "GENERATE REPORT"
5. Save PDF

---

## Keyboard Shortcuts

### Global
- `F1-F7`: Switch modules
- `Ctrl+N`: New session
- `Ctrl+O`: Open session
- `Ctrl+S`: Save session
- `Ctrl+Q`: Quit

### Player
- `Space`: Play/Pause
- `J`: Reverse
- `K`: Pause
- `L`: Forward
- `Left/Right`: Frame step
- `Home`: Go to start
- `End`: Go to end
- `F`: Fullscreen

### Offload
- `Ctrl+A`: Select all
- `Delete`: Remove selected
- `Ctrl+R`: Reset all

---

## Troubleshooting

### Common Issues

#### "FFmpeg not found"
**Solution**: Install FFmpeg and add to PATH
```bash
# Windows
choco install ffmpeg

# macOS
brew install ffmpeg

# Linux
sudo apt install ffmpeg
```

#### "LibVLC not found"
**Solution**: Install VLC Media Player
- Download from: https://www.videolan.org/

#### "Checksum mismatch"
**Causes**:
- File corruption during transfer
- Disk errors
- Incomplete copy

**Solution**:
1. Re-copy the file
2. Check disk health
3. Use different destination

#### "Sync confidence low"
**Causes**:
- Different audio sources
- Heavy background noise
- Clipped audio

**Solution**:
1. Try manual offset adjustment
2. Use timecode if available
3. Check audio quality

---

## Best Practices

### Offloading
1. Always verify checksums
2. Keep original files until verified
3. Use fast, reliable drives
4. Monitor transfer speed

### Synchronization
1. Use clean audio for best results
2. Sync early in workflow
3. Verify sync manually
4. Save sync data

### Transcoding
1. Choose appropriate preset
2. Test with one file first
3. Monitor disk space
4. Keep originals

### Reports
1. Fill all required fields
2. Review before generating
3. Save templates
4. Archive reports

---

## Support

- **Documentation**: [docs/](../docs/)
- **Issues**: [GitHub Issues](https://github.com/Herve-obe/Veriflow_v3.0/issues)
- **Email**: support@veriflow.com

---

**Version**: 3.0.0-beta  
**Last Updated**: 2025-12-23
