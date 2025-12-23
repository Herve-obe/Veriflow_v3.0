using System;
using System.IO;
using Serilog;
using Serilog.Core;

namespace Veriflow.Core.Services;

/// <summary>
/// Centralized crash logging service for production debugging
/// Logs to %AppData%/Veriflow/Logs with daily rolling files
/// </summary>
public static class CrashLogger
{
    private static ILogger? _logger;
    private static readonly object _lock = new();
    
    /// <summary>
    /// Initialize the crash logger. Should be called once at application startup.
    /// </summary>
    public static void Initialize()
    {
        lock (_lock)
        {
            if (_logger != null) return; // Already initialized
            
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Veriflow",
                "Logs"
            );
            
            Directory.CreateDirectory(logDirectory);
            
            var logPath = Path.Combine(logDirectory, "veriflow-.log");
            
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30, // Keep 30 days of logs
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
            
            _logger.Information("Veriflow crash logger initialized");
        }
    }
    
    /// <summary>
    /// Log an exception with context information
    /// </summary>
    public static void LogException(Exception ex, string context)
    {
        _logger?.Error(ex, "Context: {Context}", context);
    }
    
    /// <summary>
    /// Log an informational message
    /// </summary>
    public static void LogInfo(string message)
    {
        _logger?.Information(message);
    }
    
    /// <summary>
    /// Log a warning message
    /// </summary>
    public static void LogWarning(string message)
    {
        _logger?.Warning(message);
    }
    
    /// <summary>
    /// Log a debug message
    /// </summary>
    public static void LogDebug(string message)
    {
        _logger?.Debug(message);
    }
    
    /// <summary>
    /// Flush and close the logger (call on application shutdown)
    /// </summary>
    public static void Shutdown()
    {
        lock (_lock)
        {
            if (_logger is IDisposable disposable)
            {
                _logger.Information("Veriflow shutting down");
                disposable.Dispose();
            }
            _logger = null;
        }
    }
}
