using Heating_Control.Data;
using Heating_Control.Data.Training;

namespace Heating_Control.Training;
public interface INeuralNetworkConfigRunner
{
    float Predict(NeuralNetworkConfig neuralNetworkConfig, HeatingControlInputData inputData);
    void Train(NeuralNetworkConfig neuralNetworkConfig);
}