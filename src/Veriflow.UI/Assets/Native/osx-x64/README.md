# MiniAudio Native Binaries - Placeholder

This directory contains pre-compiled MiniAudio native libraries for macOS.

## Required File

**libminiaudio.dylib** - macOS x64 native library

## How to Obtain

Run the build script from project root:
```bash
chmod +x build_miniaudio.sh
./build_miniaudio.sh
```

This will:
1. Download miniaudio.h
2. Compile libminiaudio.dylib
3. Copy to this directory automatically

## Automatic Deployment

Once placed here, the .csproj is configured to automatically copy this library to the output directory during build.

**Status**: ⚠️ Library not yet included (to be added before deployment)
