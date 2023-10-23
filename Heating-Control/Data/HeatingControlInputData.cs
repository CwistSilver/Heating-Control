using System.Text.Json.Serialization;

namespace Heating_Control.Data;
public class HeatingControlInputData
{
    [JsonPropertyName("outdoorTemperature")]
    public float OutdoorTemperature { get; set; } // AktuelleAussentemperatur

    [JsonPropertyName("predictedOutdoorTemperature")]
    public float PredictedOutdoorTemperature { get; set; } // VorhersageTemperaturMorgen

    [JsonPropertyName("preferredIndoorTemperature")]
    public float PreferredIndoorTemperature { get; set; } // GewuenschteZimmertemperatur
}
