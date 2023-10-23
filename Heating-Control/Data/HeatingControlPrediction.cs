using System.Text.Json.Serialization;

namespace Heating_Control.Data;
public class HeatingControlPrediction
{
    [JsonPropertyName("supplyTemperature")]
    public float SupplyTemperature { get; set; } // AktuelleVorlauftemperatur
}