# MiniAudio Native Binaries - Placeholder

This directory contains pre-compiled MiniAudio native libraries for Windows.

## Required File

**miniaudio.dll** - Windows x64 native library

## How to Obtain

### Option 1: Download Pre-compiled (Recommended)
Download from: https://github.com/mackron/miniaudio/releases

### Option 2: Compile Yourself
Run the build script from project root:
```cmd
.\build_miniaudio.bat
```

This will:
1. Download miniaudio.h
2. Compile miniaudio.dll
3. Copy to this directory automatically

## Automatic Deployment

Once placed here, the .csproj is configured to automatically copy this DLL to the output directory during build.

**Status**: ⚠️ DLL not yet included (to be added before deployment)
