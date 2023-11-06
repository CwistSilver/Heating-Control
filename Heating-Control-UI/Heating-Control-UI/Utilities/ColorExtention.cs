using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Heating_Control_UI.Utilities;
internal static class ColorExtention
{
    internal static Color ToColor(this IBrush brush)
    {
        return ((ImmutableSolidColorBrush)brush).Color;
    }

    internal static Color ToColor(this IBrush brush, byte alpha = 255)
    {
        var color = brush.ToColor();
        return new Color(alpha, color.R, color.G, color.B);
    }
}
