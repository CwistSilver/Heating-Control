using Heating_Control.Data;

namespace Heating_Control.ML;
/// <summary>
/// Represents a neural network for controlling a heating system, providing functionality for initialization, training, and prediction.
/// </summary>
public interface IHeatingControlNeuralNetwork
{
    /// <summary>
    /// Returns the last options used to train the AI.
    /// </summary>
    TrainingDataOptions? UsedTrainingDataOptions { get; }

    /// <summary>
    /// Initializes the neural network.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="FileLoadException">Fails to load the AI model.</exception>
    void Inizialize();

    /// <summary>
    /// Trains or renews the neural network asynchronously.
    /// </summary>
    /// <param name="options">The options with which the neural network is to be trained.</param>
    Task TrainModel(TrainingDataOptions? options = null);

    /// <summary>
    /// Predicts heating control settings based on the provided 'HeatingControlInputData'.
    /// </summary>
    /// <param name="inputData">The input data required for heating control prediction.</param>
    /// <returns>A <see cref="HeatingControlInputData"/> , which contains the predicted values based on the <paramref name="inputData"/>.</returns>
    /// <exception cref="InvalidOperationException">If the neural network has not been initialized beforehand.</exception>
    HeatingControlPrediction Predict(HeatingControlInputData inputData);
}