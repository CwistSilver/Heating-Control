using System.Text.Json.Serialization;

namespace Heating_Control.Data.Training;
public sealed class HeatingControlTrainingData : HeatingControlInputData
{
    /// <summary>
    /// The supply temperature used in the heating system, serving as the label for model training.
    /// </summary>
    [JsonPropertyName("supplyTemperature")]
    public float SupplyTemperature { get; set; }
}
