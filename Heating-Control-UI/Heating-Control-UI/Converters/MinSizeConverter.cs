using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Heating_Control_UI.Converters;
public class MinSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var bounds = (Rect)value;
        var min = Math.Min(bounds.Width, bounds.Height);

        return min;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
