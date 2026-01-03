using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FlowClip.Converters;

/// <summary>
/// Converts pause state to button icon.
/// </summary>
public class PauseButtonIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPaused)
        {
            return isPaused ? "▶" : "⏸"; // Play (resume) or Pause
        }
        return "⏸";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts pause state to button tooltip.
/// </summary>
public class PauseButtonTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPaused)
        {
            return isPaused ? "Resume Monitoring" : "Pause Monitoring";
        }
        return "Pause Monitoring";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts pause state to button color.
/// </summary>
public class PauseButtonColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPaused && isPaused)
        {
            return new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12)); // Orange/warning color when paused
        }
        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
