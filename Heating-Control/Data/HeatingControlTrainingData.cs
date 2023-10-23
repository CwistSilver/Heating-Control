using Microsoft.ML.Data;
using System.Text.Json.Serialization;

namespace Heating_Control.Data;
public class HeatingControlTrainingData
{
    [LoadColumn(0)]
    [JsonPropertyName("outdoorTemperature")]
    public float OutdoorTemperature { get; set; }

    [LoadColumn(1)]
    [JsonPropertyName("predictedOutdoorTemperature")]
    public float PredictedOutdoorTemperature { get; set; }

    [LoadColumn(2)]
    [JsonPropertyName("preferredIndoorTemperature")]
    public float PreferredIndoorTemperature { get; set; }

    [LoadColumn(3)]
    [ColumnName("Label")]
    [JsonPropertyName("supplyTemperature")]
    public float SupplyTemperature { get; set; }
}
