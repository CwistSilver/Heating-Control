using Heating_Control.Data;
using Tensorflow;

namespace Heating_Control.ML.Trainer;
/// <summary>
/// Defines a trainer for a heating control system using a neural network.
/// </summary>
public interface IHeatingControlTrainer
{
    /// <summary>
    /// Creates and trains a new neural network asynchronously.
    /// </summary>
    /// <param name="options">The options with which the neural network is to be trained.</param>
    /// <returns>The transformer of the NeuralNetwork</returns>
    Task<NeuralNetworkModel> TrainNeuralNetworkAsync(Session session, TrainingDataOptions? options = null);
}