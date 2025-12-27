using Veriflow.Core.Models;

namespace Veriflow.UI.Services;

/// <summary>
/// Service for managing window settings persistence
/// </summary>
public interface IWindowSettingsService
{
    /// <summary>
    /// Loads window settings from persistent storage
    /// </summary>
    /// <returns>Window settings, or default settings if none exist</returns>
    WindowSettings LoadSettings();

    /// <summary>
    /// Saves window settings to persistent storage
    /// </summary>
    /// <param name="settings">Settings to save</param>
    void SaveSettings(WindowSettings settings);
}
