using Heating_Control.Data;
using Microsoft.ML;

namespace Heating_Control.ML.Trainer;
public interface IHeatingControlTrainer
{
    Task<ITransformer> TrainNeuralNetworkAsync(TrainingDataOptions? options = null);
}
