using Tensorflow;
using Tensorflow.Keras;
using static Tensorflow.KerasApi;

namespace Heating_Control.Data.Training;
public enum ActivationFunction
{
    Relu,
    LeakyReLU,
    Sigmoid
}

public sealed class LayerCreationData
{
    public ActivationFunction ActivationFunction { get; set; }
    public int Neurons { get; set; }
    public float Dropout { get; set; }
    public bool UseBatchNormalization { get; set; }


    public IEnumerable<ILayer> CreateLayers()
    {
        var layers = new List<ILayer>();
        if (ActivationFunction == ActivationFunction.Relu)
        {
            layers.Add(keras.layers.Dense(Neurons, activation: keras.activations.Relu));
        }
        else if (ActivationFunction == ActivationFunction.LeakyReLU)
        {
            layers.add(keras.layers.Dense(Neurons));
            layers.add(keras.layers.LeakyReLU());
        }
        else if (ActivationFunction == ActivationFunction.Sigmoid)
        {
            layers.Add(keras.layers.Dense(Neurons, activation: keras.activations.Sigmoid));
        }

        if (UseBatchNormalization)
            layers.add(keras.layers.BatchNormalization());

        layers.Add(keras.layers.Dropout(Dropout));

        return layers;
    }
}