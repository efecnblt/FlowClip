using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FlowClip.Models;

namespace FlowClip.Converters;

/// <summary>
/// Converts ContentType to a tag background color.
/// </summary>
public class ContentTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ClipboardContentType contentType)
        {
            return contentType switch
            {
                ClipboardContentType.Code => new SolidColorBrush(Color.FromRgb(0x6B, 0x8E, 0x23)),   // OliveDrab
                ClipboardContentType.Color => new SolidColorBrush(Color.FromRgb(0xDA, 0x70, 0xD6)), // Orchid
                ClipboardContentType.Image => new SolidColorBrush(Color.FromRgb(0x20, 0xB2, 0xAA)), // LightSeaGreen
                ClipboardContentType.Url => new SolidColorBrush(Color.FromRgb(0x41, 0x69, 0xE1)),   // RoyalBlue
                ClipboardContentType.Email => new SolidColorBrush(Color.FromRgb(0xFF, 0x8C, 0x00)), // DarkOrange
                _ => Brushes.Transparent
            };
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts ContentType to visibility for type-specific elements.
/// </summary>
public class ContentTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ClipboardContentType contentType && parameter is string expectedType)
        {
            var expected = Enum.Parse<ClipboardContentType>(expectedType, true);
            return contentType == expected ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts ColorHex string to SolidColorBrush.
/// </summary>
public class ColorHexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorHex && !string.IsNullOrEmpty(colorHex))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean HasTypeTag to visibility.
/// </summary>
public class HasTypeTagToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool hasTag)
        {
            return hasTag ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
