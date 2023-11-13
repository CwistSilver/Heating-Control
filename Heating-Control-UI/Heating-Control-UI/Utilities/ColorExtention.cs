using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkiaSharp;

namespace Heating_Control_UI.Utilities;
/// <summary>
/// Provides extension methods for color conversions between types.
/// </summary>
internal static class ColorExtention
{
    /// <summary>
    /// Converts a System.Drawing Color to an SKColor.
    /// </summary>
    /// <param name="color">The System.Drawing Color to convert.</param>
    /// <returns>The corresponding SKColor.</returns>
    internal static SKColor ToSKColor(this Color color) => new SKColor(color.R, color.G, color.B, color.A);

    /// <summary>
    /// Converts an IBrush to an SKColor, with an optional alpha value override.
    /// </summary>
    /// <param name="brush">The brush to convert.</param>
    /// <param name="alpha">Optional alpha value to override the brush's alpha.</param>
    /// <returns>The corresponding SKColor with the specified or original alpha.</returns>
    internal static SKColor ToSKColor(this IBrush brush, float? alpha = null)
    {
        var color = ToColor(brush);
        var colorAlpha = color.A;
        if(alpha is not null)
        {
            colorAlpha = (byte)(alpha * 255);
        }
        return new SKColor(color.R, color.G, color.B, colorAlpha);
    }

    /// <summary>
    /// Converts an IBrush to a System.Drawing Color.
    /// </summary>
    /// <param name="brush">The brush to convert.</param>
    /// <returns>The corresponding System.Drawing Color.</returns>
    internal static Color ToColor(this IBrush brush) => ((ImmutableSolidColorBrush)brush).Color;

    /// <summary>
    /// Converts an IBrush to a System.Drawing Color with an optional alpha value.
    /// </summary>
    /// <param name="brush">The brush to convert.</param>
    /// <param name="alpha">Optional alpha value for the color.</param>
    /// <returns>The corresponding System.Drawing Color with the specified or original alpha.</returns>
    internal static Color ToColor(this IBrush brush, byte alpha = 255)
    {
        var color = brush.ToColor();
        return new Color(alpha, color.R, color.G, color.B);
    }
}
