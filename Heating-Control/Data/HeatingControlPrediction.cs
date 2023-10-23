using Microsoft.ML.Data;
using System.Text.Json.Serialization;

namespace Heating_Control.Data;
public class HeatingControlPrediction
{
    [ColumnName("Score")]
    [JsonPropertyName("supplyTemperature")]
    public float SupplyTemperature { get; set; } // AktuelleVorlauftemperatur
}