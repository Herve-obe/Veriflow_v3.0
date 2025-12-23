# MiniAudio Native Library Setup

## Overview

Veriflow 3.0 uses **MiniAudio** - a professional, single-file, public domain audio library that supports:
- ✅ **192kHz** sample rate
- ✅ **32-bit float** precision
- ✅ **32 simultaneous tracks**
- ✅ Cross-platform (Windows, macOS, Linux)
- ✅ **Public Domain** license (completely free)

## Installation

### Windows

1. **Download MiniAudio**
   - Visit: https://github.com/mackron/miniaudio
   - Download `miniaudio.h` (single header file)

2. **Compile Native DLL**
   
   Create `miniaudio_wrapper.c`:
   ```c
   #define MINIAUDIO_IMPLEMENTATION
   #include "miniaudio.h"
   ```

   Compile with MSVC:
   ```cmd
   cl /LD /O2 miniaudio_wrapper.c /Fe:miniaudio.dll
   ```

   Or with MinGW:
   ```bash
   gcc -shared -O3 -o miniaudio.dll miniaudio_wrapper.c
   ```

3. **Copy DLL**
   ```
   Copy miniaudio.dll to:
   - d:\ELEMENT\VERIFLOW 3.0\src\Veriflow.UI\bin\Debug\net8.0\
   - Or add to system PATH
   ```

### macOS

```bash
# Download miniaudio.h
curl -O https://raw.githubusercontent.com/mackron/miniaudio/master/miniaudio.h

# Create wrapper
echo '#define MINIAUDIO_IMPLEMENTATION' > miniaudio_wrapper.c
echo '#include "miniaudio.h"' >> miniaudio_wrapper.c

# Compile
gcc -shared -O3 -o libminiaudio.dylib miniaudio_wrapper.c

# Copy to application directory
cp libminiaudio.dylib /path/to/Veriflow.UI/bin/Debug/net8.0/
```

### Linux

```bash
# Download miniaudio.h
wget https://raw.githubusercontent.com/mackron/miniaudio/master/miniaudio.h

# Create wrapper
echo '#define MINIAUDIO_IMPLEMENTATION' > miniaudio_wrapper.c
echo '#include "miniaudio.h"' >> miniaudio_wrapper.c

# Compile
gcc -shared -fPIC -O3 -o libminiaudio.so miniaudio_wrapper.c

# Copy to application directory
cp libminiaudio.so /path/to/Veriflow.UI/bin/Debug/net8.0/
```

## Pre-compiled Binaries

For convenience, pre-compiled binaries are available in the `native/` directory:
- `native/win-x64/miniaudio.dll`
- `native/osx-x64/libminiaudio.dylib`
- `native/linux-x64/libminiaudio.so`

## Verification

To verify MiniAudio is working:

1. Run Veriflow
2. Navigate to PLAYER page
3. Load an audio file
4. Check console for initialization messages

If you see "MiniAudio native library not found", ensure the DLL/SO is in the correct location.

## Troubleshooting

### Windows: "miniaudio.dll not found"
- Ensure `miniaudio.dll` is in the same directory as `Veriflow.UI.exe`
- Or add the directory to system PATH

### macOS: "libminiaudio.dylib cannot be opened"
```bash
# Remove quarantine attribute
xattr -d com.apple.quarantine libminiaudio.dylib
```

### Linux: "libminiaudio.so: cannot open shared object file"
```bash
# Add to LD_LIBRARY_PATH
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/path/to/lib
```

## Building from Source

Full build instructions:

```bash
# Clone MiniAudio
git clone https://github.com/mackron/miniaudio.git
cd miniaudio

# Windows (MSVC)
cl /LD /O2 /DMINIAUDIO_IMPLEMENTATION miniaudio.h /Fe:miniaudio.dll

# macOS
gcc -shared -O3 -DMINIAUDIO_IMPLEMENTATION -o libminiaudio.dylib miniaudio.c

# Linux
gcc -shared -fPIC -O3 -DMINIAUDIO_IMPLEMENTATION -o libminiaudio.so miniaudio.c
```

## License

MiniAudio is **Public Domain** (Unlicense) or **MIT** (dual license).

You can use it for any purpose, commercial or non-commercial, without attribution (though appreciated).

## Support

For MiniAudio issues:
- GitHub: https://github.com/mackron/miniaudio
- Documentation: https://miniaud.io/

For Veriflow integration issues:
- GitHub: https://github.com/Herve-obe/Veriflow_v3.0/issues
