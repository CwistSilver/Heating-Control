using System.Text.Json.Serialization;
using Tensorflow.Keras.Engine;

namespace Heating_Control.Data.Training;
public class NeuralNetworkConfig
{
    public required string Name { get; set; }
    public required IEnumerable<LayerCreationData> Layers { get; set; }
    public required int TrainingSize { get; set; }
    public required int Epochs { get; set; }
    public required int BatchSize { get; set; }
    public required int ValidationDatas { get; set; }
    public ModelResults? Results { get; set; }

    [JsonIgnore]
    public IModel? KerasModel { get; set; }

    [JsonIgnore]
    public Sequential? KerasSequentialModel { get; set; }
}
