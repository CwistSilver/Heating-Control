using Heating_Control.Data.Training;

namespace Heating_Control.Training.Trainer;
public interface INeuralNetworkTrainer
{
    void Train(NeuralNetworkConfig neuralNetworkConfig, int startEpoch = 1);
}