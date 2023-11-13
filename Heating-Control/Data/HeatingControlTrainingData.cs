using Microsoft.ML.Data;
using System.Text.Json.Serialization;

namespace Heating_Control.Data;
/// <summary>
/// Represents the training data for the heating control model.
/// </summary>
public sealed class HeatingControlTrainingData
{
    /// <summary>
    /// The actual outdoor temperature.
    /// </summary>
    [LoadColumn(0)]
    [JsonPropertyName("outdoorTemperature")]
    public float OutdoorTemperature { get; set; }

    /// <summary>
    /// The predicted outdoor temperature for a future point in time.
    /// </summary>
    [LoadColumn(1)]
    [JsonPropertyName("predictedOutdoorTemperature")]
    public float PredictedOutdoorTemperature { get; set; }

    /// <summary>
    /// The desired temperature inside the building.
    /// </summary>
    [LoadColumn(2)]
    [JsonPropertyName("preferredIndoorTemperature")]
    public float PreferredIndoorTemperature { get; set; }

    /// <summary>
    /// The supply temperature used in the heating system, serving as the label for model training.
    /// </summary>
    [LoadColumn(3)]
    [ColumnName("Label")]
    [JsonPropertyName("supplyTemperature")]
    public float SupplyTemperature { get; set; }
}