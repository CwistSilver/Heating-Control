using Heating_Control.Data;

namespace Heating_Control.NeuralNetwork;
/// <summary>
/// Represents a neural network for controlling a heating system, providing functionality for initialization, training, and prediction.
/// </summary>
public interface IHeatingControlNeuralNetwork
{
    /// <summary>
    /// Initializes the neural network.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="FileLoadException">Fails to load the AI model.</exception>
    void Inizialize();

    /// <summary>
    /// Predicts <see cref="HeatingControlPrediction"/> based on the specified <paramref name="inputData"/>.
    /// </summary>
    /// <param name="inputData">The input data required for heating control prediction.</param>
    /// <returns>A <see cref="HeatingControlInputData"/>, which contains the predicted values based on the <paramref name="inputData"/>.</returns>
    /// <exception cref="InvalidOperationException">If the neural network has not been initialized beforehand.</exception>
    HeatingControlPrediction Predict(HeatingControlInputData inputData);

    /// <summary>
    /// Predicts several <see cref="HeatingControlPrediction"/> based on the specified <paramref name="inputDatas"/>.
    /// </summary>
    /// <param name="inputDatas">The input datas required for heating control prediction.</param>
    /// <returns>several <see cref="HeatingControlInputData"/>, which contains the predicted values based on the <paramref name="inputDatas"/>.</returns>
    /// <exception cref="InvalidOperationException">If the neural network has not been initialized beforehand.</exception>
    List<HeatingControlPrediction> Predict(IEnumerable<HeatingControlInputData> inputDatas);
}