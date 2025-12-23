using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Veriflow.Core.Models;

namespace Veriflow.UI.Converters;

/// <summary>
/// Converts file size in bytes to human-readable format
/// </summary>
public class FileSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        return "0 B";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts MediaType enum to icon
/// </summary>
public class MediaTypeIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MediaType type)
        {
            return type switch
            {
                MediaType.Video => "🎬",
                MediaType.Audio => "🎵",
                MediaType.Image => "🖼️",
                _ => "📄"
            };
        }
        return "📄";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
