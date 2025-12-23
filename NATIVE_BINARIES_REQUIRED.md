# CRITICAL: Pre-compiled MiniAudio Binaries Required

## Zero Config Deployment Strategy

For production deployment, Veriflow requires pre-compiled MiniAudio native libraries to be included in the repository.

### Required Files

**Windows (win-x64)**:
- `src/Veriflow.UI/Assets/Native/win-x64/miniaudio.dll`

**macOS (osx-x64)**:
- `src/Veriflow.UI/Assets/Native/osx-x64/libminiaudio.dylib`

**Linux (linux-x64)**:
- `src/Veriflow.UI/Assets/Native/linux-x64/libminiaudio.so`

### Current Status

⚠️ **BINARIES NOT YET INCLUDED**

These files must be obtained and committed to the repository before deployment.

### How to Obtain (One-Time Setup)

#### Option 1: Download Pre-compiled (Recommended)

Visit: https://github.com/mackron/miniaudio/releases

Or use community-provided binaries from:
- NuGet packages that include native runtimes
- vcpkg: `vcpkg install miniaudio`
- Homebrew (macOS): `brew install miniaudio`

#### Option 2: Compile Locally (Development)

**Windows**:
```cmd
cd native
.\build_emergency.bat
```

**macOS/Linux**:
```bash
cd native
chmod +x build_miniaudio.sh
./build_miniaudio.sh
```

### Integration

Once obtained, place the binaries in the appropriate `Assets/Native/` directories.
The .csproj is already configured to automatically copy them to the output directory during build.

### License

MiniAudio is Public Domain (Unlicense) or MIT dual-licensed.
Including pre-compiled binaries is fully compliant with the license.

### For Production Release

**CRITICAL**: Before Phase 13 deployment, these binaries MUST be:
1. Obtained (compiled or downloaded)
2. Tested on all platforms
3. Committed to the repository
4. Included in the installer package

This ensures "Zero Config" deployment - users install and run immediately without any setup.
