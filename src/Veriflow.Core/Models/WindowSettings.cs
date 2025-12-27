namespace Veriflow.Core.Models;

/// <summary>
/// Represents the window state settings for persistence across application sessions
/// </summary>
public class WindowSettings
{
    /// <summary>
    /// Window width in pixels
    /// </summary>
    public double Width { get; set; } = 1600;

    /// <summary>
    /// Window height in pixels
    /// </summary>
    public double Height { get; set; } = 900;

    /// <summary>
    /// Window X position on screen
    /// </summary>
    public double X { get; set; } = 100;

    /// <summary>
    /// Window Y position on screen
    /// </summary>
    public double Y { get; set; } = 100;

    /// <summary>
    /// Whether the window is maximized
    /// </summary>
    public bool IsMaximized { get; set; } = true;

    /// <summary>
    /// Creates default window settings (maximized state for first launch)
    /// </summary>
    public static WindowSettings CreateDefault()
    {
        return new WindowSettings
        {
            Width = 1600,
            Height = 900,
            X = 100,
            Y = 100,
            IsMaximized = true
        };
    }
}
