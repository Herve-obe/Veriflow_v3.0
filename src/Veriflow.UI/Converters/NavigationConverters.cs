using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Veriflow.UI.Converters;

/// <summary>
/// Converter to determine if a page is active based on current page name
/// </summary>
public class PageActiveConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string currentPage && parameter is string pageName)
        {
            return currentPage == pageName;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to get background color for active navigation button
/// </summary>
public class ActivePageBackgroundConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && 
            values[0] is string currentPage && 
            values[1] is string activeColor &&
            parameter is string pageName)
        {
            if (currentPage == pageName)
            {
                return Brush.Parse(activeColor);
            }
        }
        return Brushes.Transparent;
    }
}

/// <summary>
/// Converter to get foreground color for navigation button
/// </summary>
public class ActivePageForegroundConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 1 && 
            values[0] is string currentPage && 
            parameter is string pageName)
        {
            if (currentPage == pageName)
            {
                return Brushes.White;
            }
        }
        return Brush.Parse("#B0B0B0");
    }
}
