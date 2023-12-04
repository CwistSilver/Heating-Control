using Heating_Control.Data.Training;

namespace Heating_Control_Trainer;
public static class NeuralNetworkConfigs
{
    public static List<NeuralNetworkConfig> GetNeuralNetworkConfigs()
    {
        return new[]
        {
            NeuralNetwork1_2_1,
            NeuralNetwork1_3
        }.ToList();
    }

    private static NeuralNetworkConfig NeuralNetwork1_2_1 = new()
    {
        Name = "Neural-Network-1-2-1",
        Layers = new List<LayerCreationData>()
            {
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 256},
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 128},
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 64}
            },
        TrainingSize = 10_000,
        Epochs = 200,
        BatchSize = 100,
        ValidationDatas = 2000
    };

    private static NeuralNetworkConfig NeuralNetwork1_3 = new()
    {
        Name = "Neural-Network-1-3",
        Layers = new List<LayerCreationData>()
            {
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 32},
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 16},
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 8}
            },
        TrainingSize = 10_000,
        Epochs = 200,
        BatchSize = 100,
        ValidationDatas = 2000
    };
}
