using System;
using System.IO;
using System.Text.Json;
using Veriflow.Core.Models;

namespace Veriflow.UI.Services;

/// <summary>
/// Service for persisting window settings to a JSON file
/// </summary>
public class WindowSettingsService : IWindowSettingsService
{
    private readonly string _settingsFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public WindowSettingsService()
    {
        // Store settings in %APPDATA%/Veriflow/window-settings.json
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var veriflowFolder = Path.Combine(appDataPath, "Veriflow");
        
        // Ensure directory exists
        Directory.CreateDirectory(veriflowFolder);
        
        _settingsFilePath = Path.Combine(veriflowFolder, "window-settings.json");
    }

    /// <summary>
    /// Loads window settings from JSON file
    /// </summary>
    public WindowSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                // First launch - return default settings (maximized)
                return WindowSettings.CreateDefault();
            }

            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<WindowSettings>(json, JsonOptions);
            
            return settings ?? WindowSettings.CreateDefault();
        }
        catch (Exception ex)
        {
            // If there's any error reading/parsing the file, return defaults
            Console.WriteLine($"Error loading window settings: {ex.Message}");
            return WindowSettings.CreateDefault();
        }
    }

    /// <summary>
    /// Saves window settings to JSON file
    /// </summary>
    public void SaveSettings(WindowSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            Console.WriteLine($"Error saving window settings: {ex.Message}");
        }
    }
}
