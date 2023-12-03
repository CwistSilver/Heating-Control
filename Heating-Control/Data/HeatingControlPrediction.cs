using System.Text.Json.Serialization;

namespace Heating_Control.Data;
/// <summary>
/// Represents the prediction output for heating control.
/// </summary>
public sealed class HeatingControlPrediction
{
    /// <summary>
    /// The predicted supply temperature necessary to achieve the desired indoor conditions.
    /// </summary>
    [JsonPropertyName("supplyTemperature")]
    public float SupplyTemperature { get; set; }
}