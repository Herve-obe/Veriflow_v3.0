@echo off
REM MiniAudio Native Library Build Script for Windows
REM This script downloads and compiles miniaudio.dll

echo ========================================
echo MiniAudio Native Library Builder
echo ========================================
echo.

REM Check if Visual Studio is installed
where cl.exe >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Visual Studio C++ compiler not found!
    echo Please run this script from "Developer Command Prompt for VS"
    echo Or install Visual Studio Build Tools
    pause
    exit /b 1
)

REM Create native directory
if not exist "native\win-x64" mkdir "native\win-x64"
cd native\win-x64

echo Downloading miniaudio.h...
powershell -Command "Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/mackron/miniaudio/master/miniaudio.h' -OutFile 'miniaudio.h'"

if not exist "miniaudio.h" (
    echo ERROR: Failed to download miniaudio.h
    pause
    exit /b 1
)

echo Creating wrapper source...
echo #define MINIAUDIO_IMPLEMENTATION > miniaudio_wrapper.c
echo #include "miniaudio.h" >> miniaudio_wrapper.c

echo.
echo Compiling miniaudio.dll...
cl /LD /O2 /DNDEBUG miniaudio_wrapper.c /Fe:miniaudio.dll /link /DEF:NUL

if not exist "miniaudio.dll" (
    echo ERROR: Compilation failed!
    pause
    exit /b 1
)

echo.
echo Copying DLL to Assets directory...
copy /Y miniaudio.dll ..\..\src\Veriflow.UI\Assets\Native\win-x64\ >nul 2>nul

echo.
echo ========================================
echo SUCCESS! miniaudio.dll compiled
echo Location: native\win-x64\miniaudio.dll
echo Copied to: src\Veriflow.UI\Assets\Native\win-x64\
echo ========================================
echo.

cd ..\..
pause
