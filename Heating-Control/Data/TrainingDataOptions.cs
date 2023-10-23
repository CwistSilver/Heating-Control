using System.Text.Json.Serialization;

namespace Heating_Control.Data;
/// <summary>
/// Represents the options used for generating training data.
/// </summary>
public sealed class TrainingDataOptions
{
    /// <summary>
    /// The number of records to generate.
    /// </summary>
    [JsonPropertyName("recordsToGenerate")]
    public uint RecordsToGenerate { get; init; } = 1_000;

    /// <summary>
    /// The imprecision applied to the generation of the supply temperature in percent.
    /// </summary>
    [JsonPropertyName("supplyTemperatureInaccuracy")]
    public uint SupplyTemperatureInaccuracy { get; init; } = 5;

    /// <summary>
    /// The offset range for the predicted outdoor temperature.
    /// </summary>
    [JsonPropertyName("predictedOutdoorTemperatureOffsetRange")]
    public TemperatureRange PredictedOutdoorTemperatureOffsetRange { get; init; } = new TemperatureRange() { Min = -10.00f, Max = 10.00f };

    /// <summary>
    /// Represents the gradient or "slope" in the mathematical formula used to model heating behavior.<br/>
    /// It describes how sharply the supply temperature responds to changes in other variables.
    /// <para>
    /// Learn more about the influence of the 
    /// <a href="https://www.heizung.de/ratgeber/diverses/heizkennlinie-definition-und-einflussfaktoren.html#:~:text=Einflussfaktor%201%C2%A0%E2%80%93%20Die%20Neigung%20der%20Heizkennlinie" target="_blank">
    /// Gradient
    /// </a>
    /// </para>    
    /// </summary> 
    [JsonPropertyName("gradient")]
    public float Gradient { get; init; } = 1.5f;

    /// <summary>
    /// Represents the baseline or "offset" in the mathematical formula.<br/>
    /// This is a constant value that establishes a starting or reference point for the supply temperature, independent of other variables.
    /// <para>
    /// Learn more about the influence of the 
    /// <a href="https://www.heizung.de/ratgeber/diverses/heizkennlinie-definition-und-einflussfaktoren.html#:~:text=Einflussfaktor%202%C2%A0%E2%80%93%20Das%20Niveau%20der%20Heizkennlinie" target="_blank">
    /// Baseline
    /// </a>
    /// </para>    
    /// </summary>
    [JsonPropertyName("baseline")]
    public float Baseline { get; init; } = 1.5f;

    /// <summary>
    /// The maximum supply temperature.
    /// </summary>
    [JsonPropertyName("maxSupplyTemperature")]
    public float MaxSupplyTemperature { get; init; } = 90f;

    /// <summary>
    /// The range for the preferred indoor temperature.
    /// </summary>
    [JsonPropertyName("preferredIndoorTemperatureRange")]
    public TemperatureRange PreferredIndoorTemperatureRange { get; init; } = new TemperatureRange() { Min = 19.0f, Max = 28.0f };

    /// <summary>
    /// The range for the outdoor temperature.
    /// </summary>
    [JsonPropertyName("outdoorTemperatureRange")]
    public TemperatureRange OutdoorTemperatureRange { get; init; } = new TemperatureRange() { Min = -20.0f, Max = 20.0f };
}
