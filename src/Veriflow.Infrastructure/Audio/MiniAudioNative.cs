using System;
using System.Runtime.InteropServices;

namespace Veriflow.Infrastructure.Audio;

/// <summary>
/// Native P/Invoke bindings for miniaudio.h
/// MiniAudio is a single-file public domain audio library
/// Supports up to 192kHz, 32-bit float, cross-platform
/// </summary>
public static class MiniAudioNative
{
    private const string DllName = "miniaudio";
    
    // Format
    public enum ma_format
    {
        ma_format_unknown = 0,
        ma_format_u8 = 1,
        ma_format_s16 = 2,
        ma_format_s24 = 3,
        ma_format_s32 = 4,
        ma_format_f32 = 5
    }
    
    // Result codes
    public enum ma_result
    {
        MA_SUCCESS = 0,
        MA_ERROR = -1,
        MA_INVALID_ARGS = -2,
        MA_INVALID_OPERATION = -3,
        MA_OUT_OF_MEMORY = -4,
        MA_OUT_OF_RANGE = -5,
        MA_ACCESS_DENIED = -6,
        MA_DOES_NOT_EXIST = -7,
        MA_ALREADY_EXISTS = -8,
        MA_TOO_MANY_OPEN_FILES = -9,
        MA_INVALID_FILE = -10,
        MA_TOO_BIG = -11,
        MA_PATH_TOO_LONG = -12,
        MA_NAME_TOO_LONG = -13,
        MA_NOT_DIRECTORY = -14,
        MA_IS_DIRECTORY = -15,
        MA_DIRECTORY_NOT_EMPTY = -16,
        MA_AT_END = -17,
        MA_NO_SPACE = -18,
        MA_BUSY = -19,
        MA_IO_ERROR = -20,
        MA_INTERRUPT = -21,
        MA_UNAVAILABLE = -22,
        MA_ALREADY_IN_USE = -23,
        MA_BAD_ADDRESS = -24,
        MA_BAD_SEEK = -25,
        MA_BAD_PIPE = -26,
        MA_DEADLOCK = -27,
        MA_TOO_MANY_LINKS = -28,
        MA_NOT_IMPLEMENTED = -29,
        MA_NO_MESSAGE = -30,
        MA_BAD_MESSAGE = -31,
        MA_NO_DATA_AVAILABLE = -32,
        MA_INVALID_DATA = -33,
        MA_TIMEOUT = -34,
        MA_NO_NETWORK = -35,
        MA_NOT_UNIQUE = -36,
        MA_NOT_SOCKET = -37,
        MA_NO_ADDRESS = -38,
        MA_BAD_PROTOCOL = -39,
        MA_PROTOCOL_UNAVAILABLE = -40,
        MA_PROTOCOL_NOT_SUPPORTED = -41,
        MA_PROTOCOL_FAMILY_NOT_SUPPORTED = -42,
        MA_ADDRESS_FAMILY_NOT_SUPPORTED = -43,
        MA_SOCKET_NOT_SUPPORTED = -44,
        MA_CONNECTION_RESET = -45,
        MA_ALREADY_CONNECTED = -46,
        MA_NOT_CONNECTED = -47,
        MA_CONNECTION_REFUSED = -48,
        MA_NO_HOST = -49,
        MA_IN_PROGRESS = -50,
        MA_CANCELLED = -51,
        MA_MEMORY_ALREADY_MAPPED = -52,
        MA_FORMAT_NOT_SUPPORTED = -100,
        MA_DEVICE_TYPE_NOT_SUPPORTED = -101,
        MA_SHARE_MODE_NOT_SUPPORTED = -102,
        MA_NO_BACKEND = -103,
        MA_NO_DEVICE = -104,
        MA_API_NOT_FOUND = -105,
        MA_INVALID_DEVICE_CONFIG = -106,
        MA_LOOP = -107,
        MA_BACKEND_NOT_ENABLED = -108,
        MA_DEVICE_NOT_INITIALIZED = -200,
        MA_DEVICE_ALREADY_INITIALIZED = -201,
        MA_DEVICE_NOT_STARTED = -202,
        MA_DEVICE_NOT_STOPPED = -203,
        MA_FAILED_TO_INIT_BACKEND = -300,
        MA_FAILED_TO_OPEN_BACKEND_DEVICE = -301,
        MA_FAILED_TO_START_BACKEND_DEVICE = -302,
        MA_FAILED_TO_STOP_BACKEND_DEVICE = -303
    }
    
    // Device type
    public enum ma_device_type
    {
        ma_device_type_playback = 1,
        ma_device_type_capture = 2,
        ma_device_type_duplex = 3,
        ma_device_type_loopback = 4
    }
    
    // Engine config
    [StructLayout(LayoutKind.Sequential)]
    public struct ma_engine_config
    {
        public IntPtr pResourceManager;
        public IntPtr pContext;
        public IntPtr pDevice;
        public IntPtr pLog;
        public uint listenerCount;
        public uint channels;
        public uint sampleRate;
        public uint periodSizeInFrames;
        public uint periodSizeInMilliseconds;
        public uint gainSmoothTimeInFrames;
        public uint gainSmoothTimeInMilliseconds;
        public uint defaultVolumeSmoothTimeInPCMFrames;
        public ma_allocation_callbacks allocationCallbacks;
        public byte noAutoStart;
        public byte noDevice;
        public ma_format monoExpansionMode;
        public IntPtr pResourceManagerVFS;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ma_allocation_callbacks
    {
        public IntPtr pUserData;
        public IntPtr onMalloc;
        public IntPtr onRealloc;
        public IntPtr onFree;
    }
    
    // Engine
    [StructLayout(LayoutKind.Sequential)]
    public struct ma_engine
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] opaque;
    }
    
    // Sound
    [StructLayout(LayoutKind.Sequential)]
    public struct ma_sound
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2048)]
        public byte[] opaque;
    }
    
    // Engine functions
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_engine_config ma_engine_config_init();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_engine_init(ref ma_engine_config pConfig, out ma_engine pEngine);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ma_engine_uninit(ref ma_engine pEngine);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_engine_start(ref ma_engine pEngine);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_engine_stop(ref ma_engine pEngine);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_engine_set_volume(ref ma_engine pEngine, float volume);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float ma_engine_get_volume(ref ma_engine pEngine);
    
    // Sound functions
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ma_result ma_sound_init_from_file(
        ref ma_engine pEngine,
        [MarshalAs(UnmanagedType.LPStr)] string pFilePath,
        uint flags,
        IntPtr pGroup,
        IntPtr pDoneFence,
        out ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ma_sound_uninit(ref ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_sound_start(ref ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_sound_stop(ref ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ma_sound_set_volume(ref ma_sound pSound, float volume);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float ma_sound_get_volume(ref ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ma_sound_set_pan(ref ma_sound pSound, float pan);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float ma_sound_get_pan(ref ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte ma_sound_is_playing(ref ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte ma_sound_at_end(ref ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_sound_seek_to_pcm_frame(ref ma_sound pSound, ulong frameIndex);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_sound_get_cursor_in_pcm_frames(ref ma_sound pSound, out ulong pCursor);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ma_result ma_sound_get_length_in_pcm_frames(ref ma_sound pSound, out ulong pLength);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte ma_sound_is_looping(ref ma_sound pSound);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ma_sound_set_looping(ref ma_sound pSound, byte isLooping);
    
    // Sound flags
    public const uint MA_SOUND_FLAG_STREAM = 0x00000001;
    public const uint MA_SOUND_FLAG_DECODE = 0x00000002;
    public const uint MA_SOUND_FLAG_ASYNC = 0x00000004;
    public const uint MA_SOUND_FLAG_WAIT_INIT = 0x00000008;
    public const uint MA_SOUND_FLAG_NO_DEFAULT_ATTACHMENT = 0x00001000;
    public const uint MA_SOUND_FLAG_NO_PITCH = 0x00002000;
    public const uint MA_SOUND_FLAG_NO_SPATIALIZATION = 0x00004000;
}
