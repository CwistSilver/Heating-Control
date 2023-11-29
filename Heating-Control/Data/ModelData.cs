using System.Text.Json.Serialization;
using Tensorflow;

namespace Heating_Control.Data;
/// <summary>
/// Represents the data model containing the transformer and training options for the neural network.
/// </summary>
public class ModelData
{
    /// <summary>
    /// The model used in the neural network.
    /// </summary>
    [JsonIgnore]
    public required Session Data { get; set; }

    /// <summary>
    /// The training data options for the neural network model.
    /// </summary>
    public required TrainingDataOptions? Options { get; set; }
}
