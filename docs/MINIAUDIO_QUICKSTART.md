# MiniAudio Pre-Compiled Binaries - Quick Setup

## Quick Start (No Compilation Required)

Since Visual Studio may not be available, here's how to get MiniAudio working quickly:

### Option 1: Download Pre-Compiled DLL (Recommended)

**Windows x64**:
1. Download from: https://github.com/mackron/miniaudio/releases
2. Or use this direct link: [miniaudio.dll for Windows](https://github.com/mackron/miniaudio/releases/latest/download/miniaudio.dll)
3. Copy to: `d:\ELEMENT\VERIFLOW 3.0\src\Veriflow.UI\bin\Debug\net8.0\miniaudio.dll`

**Alternative - Use NuGet Package**:
```bash
dotnet add src/Veriflow.Infrastructure/Veriflow.Infrastructure.csproj package MiniAudioNative
```

### Option 2: Manual Compilation (If you have MinGW)

```bash
# Download miniaudio.h
curl -O https://raw.githubusercontent.com/mackron/miniaudio/master/miniaudio.h

# Create wrapper
echo "#define MINIAUDIO_IMPLEMENTATION" > miniaudio_wrapper.c
echo "#include \"miniaudio.h\"" >> miniaudio_wrapper.c

# Compile with MinGW
gcc -shared -O3 -o miniaudio.dll miniaudio_wrapper.c

# Copy to output
copy miniaudio.dll src\Veriflow.UI\bin\Debug\net8.0\
```

### Option 3: Use Fallback (Testing Only)

For testing purposes, the application will gracefully handle missing DLL with an error message.
You can test all other features while waiting for the native library.

## Verification

Run Veriflow and check:
- If DLL is found: Audio player will work with 192kHz support
- If DLL is missing: Error message will appear, but app continues

## Production Deployment

For production, include `miniaudio.dll` in the installer package.

## Support

- MiniAudio GitHub: https://github.com/mackron/miniaudio
- Veriflow Issues: https://github.com/Herve-obe/Veriflow_v3.0/issues
