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

    /// <summary>
    /// The maximum supply temperature.
    /// </summary>
    [JsonPropertyName("maxSupplyTemperature")]
    public float MaxSupplyTemperature { get; set; }

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
    public float Gradient { get; set; }

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
    public float Baseline { get; set; }
}
