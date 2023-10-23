using System.Text.Json.Serialization;

namespace Heating_Control.Data;
public sealed class TemperatureRange
{
    [JsonPropertyName("min")]
    public float Min { get; init; }

    [JsonPropertyName("max")]
    public float Max { get; init; }
}
