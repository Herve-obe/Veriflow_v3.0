#!/bin/bash
# MiniAudio Native Library Build Script for macOS/Linux
# This script downloads and compiles libminiaudio.dylib/.so

echo "========================================"
echo "MiniAudio Native Library Builder"
echo "========================================"
echo ""

# Detect OS
if [[ "$OSTYPE" == "darwin"* ]]; then
    OS="macos"
    LIB_NAME="libminiaudio.dylib"
    NATIVE_DIR="native/osx-x64"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    OS="linux"
    LIB_NAME="libminiaudio.so"
    NATIVE_DIR="native/linux-x64"
else
    echo "ERROR: Unsupported OS: $OSTYPE"
    exit 1
fi

echo "Building for: $OS"
echo ""

# Check if gcc is installed
if ! command -v gcc &> /dev/null; then
    echo "ERROR: gcc not found!"
    echo "Please install gcc:"
    if [[ "$OS" == "macos" ]]; then
        echo "  xcode-select --install"
    else
        echo "  sudo apt-get install build-essential"
    fi
    exit 1
fi

# Create native directory
mkdir -p "$NATIVE_DIR"
cd "$NATIVE_DIR"

echo "Downloading miniaudio.h..."
curl -L -o miniaudio.h https://raw.githubusercontent.com/mackron/miniaudio/master/miniaudio.h

if [ ! -f "miniaudio.h" ]; then
    echo "ERROR: Failed to download miniaudio.h"
    exit 1
fi

echo "Creating wrapper source..."
cat > miniaudio_wrapper.c << 'EOF'
#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"
EOF

echo ""
echo "Compiling $LIB_NAME..."

if [[ "$OS" == "macos" ]]; then
    gcc -shared -O3 -DNDEBUG -o "$LIB_NAME" miniaudio_wrapper.c
else
    gcc -shared -fPIC -O3 -DNDEBUG -o "$LIB_NAME" miniaudio_wrapper.c
fi

if [ ! -f "$LIB_NAME" ]; then
    echo "ERROR: Compilation failed!"
    exit 1
fi

echo ""
echo "Copying library to Assets directory..."
if [[ "$OS" == "macos" ]]; then
    mkdir -p ../../src/Veriflow.UI/Assets/Native/osx-x64
    cp -f "$LIB_NAME" ../../src/Veriflow.UI/Assets/Native/osx-x64/ 2>/dev/null || true
    DEST_DIR="src/Veriflow.UI/Assets/Native/osx-x64"
else
    mkdir -p ../../src/Veriflow.UI/Assets/Native/linux-x64
    cp -f "$LIB_NAME" ../../src/Veriflow.UI/Assets/Native/linux-x64/ 2>/dev/null || true
    DEST_DIR="src/Veriflow.UI/Assets/Native/linux-x64"
fi

echo ""
echo "========================================"
echo "SUCCESS! $LIB_NAME compiled"
echo "Location: $NATIVE_DIR/$LIB_NAME"
echo "Copied to: $DEST_DIR/"
echo "========================================"
echo ""

cd ../..
