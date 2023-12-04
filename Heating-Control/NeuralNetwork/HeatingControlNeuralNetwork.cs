using Heating_Control.Data;
using Heating_Control.NeuralNetwork.PostEffect;
using Heating_Control.Normalizer;
using Heating_Control.Storage;
using Microsoft.Extensions.Logging;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;

namespace Heating_Control.NeuralNetwork;
public sealed class HeatingControlNeuralNetwork : IHeatingControlNeuralNetwork
{
    private IModel? _model;
    private readonly IModelStorage _modelStorage;
    private readonly IPredictionPostEffect _predictionPostEffect;
    private readonly IDataNormalizer _dataNormalizer;
    private readonly ILogger<IHeatingControlNeuralNetwork> _logger;

    public HeatingControlNeuralNetwork(IModelStorage modelStorage, IPredictionPostEffect predictionPostEffect,IDataNormalizer dataNormalizer, ILogger<IHeatingControlNeuralNetwork> logger)
    {
        _modelStorage = modelStorage;
        _predictionPostEffect = predictionPostEffect;
        _dataNormalizer = dataNormalizer;
        _logger = logger;
    }

    public void Inizialize()
    {
        if (_model is not null)
        {
            _logger.LogError("Attempted to initialize the neural network, but it has already been initialized.");
            throw new InvalidOperationException("Neural network has already been initialized");
        }

        var modelData = _modelStorage.Load() ?? throw new FileLoadException("Neural network could not be loaded");
        _model = modelData;

        _logger.LogInformation("Neural network initialized successfully.");
    }

    public HeatingControlPrediction Predict(HeatingControlInputData input)
    {
        var normalizedInput = _dataNormalizer.NormalizeInput(input);

        var inputTensor = np.array(normalizedInput);
        inputTensor = inputTensor.reshape(new Shape(1, -1));

        var prediction = _model!.predict(inputTensor, batch_size: 1);

        var predictionArray = prediction.numpy();
        return _predictionPostEffect.AddPostEffect(input, predictionArray.reshape(-1).ToArray<float>()[0]); ;
    }

    public List<HeatingControlPrediction> Predict(IEnumerable<HeatingControlInputData> inputs)
    {
        var inputArrays = new List<NDArray>();
        foreach (var input in inputs)
        {
            var normalizedInput = _dataNormalizer.NormalizeInput(input);
            inputArrays.Add(np.array(normalizedInput));
        }

        var inputMatrix = np.stack(inputArrays.ToArray());

        var predictions = _model!.predict(inputMatrix, batch_size: inputs.Count());

        var predictionArray = predictions.numpy();
        var flattenedArray = predictionArray.reshape(-1).ToArray<float>();

        var results = new List<HeatingControlPrediction>();
        var inputArray = inputs.ToArray();
        for (int i = 0; i < flattenedArray.Length; i++)
        {
            results.Add(_predictionPostEffect.AddPostEffect(inputArray[i], flattenedArray[i]));
        }

        return results;
    }
}