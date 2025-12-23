@echo off
REM Emergency MiniAudio DLL Builder - For Development Only
REM This creates a minimal stub DLL for testing

echo Creating MiniAudio stub DLL for development...

REM Create minimal C wrapper
echo #define MINIAUDIO_IMPLEMENTATION > miniaudio_stub.c
echo #include "miniaudio.h" >> miniaudio_stub.c

REM Download miniaudio.h
powershell -Command "Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/mackron/miniaudio/master/miniaudio.h' -OutFile 'miniaudio.h'"

REM Try to compile with any available compiler
where cl.exe >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Compiling with MSVC...
    cl /LD /O2 miniaudio_stub.c /Fe:miniaudio.dll
    goto :copy
)

where gcc.exe >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Compiling with GCC...
    gcc -shared -O3 -o miniaudio.dll miniaudio_stub.c
    goto :copy
)

echo ERROR: No compiler found!
echo Please install Visual Studio or MinGW
pause
exit /b 1

:copy
copy /Y miniaudio.dll ..\src\Veriflow.UI\Assets\Native\win-x64\
echo Done! DLL copied to Assets/Native/win-x64/
pause
