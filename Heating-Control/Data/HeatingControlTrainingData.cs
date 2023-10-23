using System.Text.Json.Serialization;

namespace Heating_Control.Data;
public class HeatingControlTrainingData
{
    [JsonPropertyName("outdoorTemperature")]
    public float OutdoorTemperature { get; set; }

    [JsonPropertyName("predictedOutdoorTemperature")]
    public float PredictedOutdoorTemperature { get; set; }

    [JsonPropertyName("preferredIndoorTemperature")]
    public float PreferredIndoorTemperature { get; set; }

    [JsonPropertyName("supplyTemperature")]
    public float SupplyTemperature { get; set; }
}
