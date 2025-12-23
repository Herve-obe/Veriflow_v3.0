using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Veriflow.UI.Converters;

/// <summary>
/// Converts peak level (0.0-1.0) to width for horizontal VU meter
/// </summary>
public class PeakToWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float peak)
        {
            // Assuming max width of 100
            return Math.Clamp(peak * 100, 0, 100);
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts peak level (0.0-1.0) to height for vertical VU meter
/// </summary>
public class PeakToHeightConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float peak)
        {
            // Assuming max height of 300 (track VU meter height)
            return Math.Clamp(peak * 300, 0, 300);
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
