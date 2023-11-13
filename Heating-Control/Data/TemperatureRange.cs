using System.Text.Json.Serialization;

namespace Heating_Control.Data;
/// <summary>
/// Represents a range of temperatures, specifying minimum and maximum values.
/// </summary>
public sealed class TemperatureRange
{
    /// <summary>
    /// The minimum temperature in the range.
    /// </summary>
    [JsonPropertyName("min")]
    public float Min { get; init; }

    /// <summary>
    /// The maximum temperature in the range.
    /// </summary>
    [JsonPropertyName("max")]
    public float Max { get; init; }
}