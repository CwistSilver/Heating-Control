using System.Text.Json.Serialization;

namespace Heating_Control.Data;
/// <summary>
/// Represents input data for heating control predictions.
/// </summary>
public sealed class HeatingControlInputData
{
    /// <summary>
    /// The current temperature outside the building.
    /// </summary>
    [JsonPropertyName("outdoorTemperature")]
    public float OutdoorTemperature { get; set; }

    /// <summary>
    /// A forecasted outdoor temperature for a future point in time.
    /// </summary>
    [JsonPropertyName("predictedOutdoorTemperature")]
    public float PredictedOutdoorTemperature { get; set; }

    /// <summary>
    /// The desired temperature inside the building.
    /// </summary>
    [JsonPropertyName("preferredIndoorTemperature")]
    public float PreferredIndoorTemperature { get; set; }
}
