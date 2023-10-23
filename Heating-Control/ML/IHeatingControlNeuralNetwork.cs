using Heating_Control.Data;

namespace Heating_Control.ML;
public interface IHeatingControlNeuralNetwork
{
    Task Inizialize();

    HeatingControlPrediction Predict(HeatingControlInputData inputData);
}
