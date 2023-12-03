﻿using Heating_Control.Data;
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
    private readonly ILogger<IHeatingControlNeuralNetwork> _logger;

    public HeatingControlNeuralNetwork(IModelStorage modelStorage, ILogger<IHeatingControlNeuralNetwork> logger)
    {
        _modelStorage = modelStorage;
        _logger = logger;
    }

    public HeatingControlPrediction Predict(HeatingControlInputData input)
    {
        var normalizedInput = NormalizeInput(input);

        var inputTensor = np.array(normalizedInput);
        inputTensor = inputTensor.reshape(new Shape(1, -1));

        var prediction = _model!.predict(inputTensor, batch_size: 1);

        var predictionArray = prediction.numpy();
        var floatValue = predictionArray.reshape(-1).ToArray<float>()[0];
        floatValue = Math.Clamp(floatValue, 0f, 1f);

        return new HeatingControlPrediction() { SupplyTemperature = floatValue * 100f };
    }

    private void PostEffect(HeatingControlPrediction prediction)
    {
        //prediction.
    }

    public List<HeatingControlPrediction> Predict(IEnumerable<HeatingControlInputData> inputs)
    {
        var inputArrays = new List<NDArray>();
        foreach (var input in inputs)
        {
            var normalizedInput = NormalizeInput(input);
            inputArrays.Add(np.array(normalizedInput));
        }

        var inputMatrix = np.stack(inputArrays.ToArray());

        var predictions = _model!.predict(inputMatrix, batch_size: inputs.Count());

        var predictionArray = predictions.numpy();
        var flattenedArray = predictionArray.reshape(-1).ToArray<float>();

        var results = new List<HeatingControlPrediction>();
        foreach (var floatValue in flattenedArray)
        {
            var clampedValue = Math.Clamp(floatValue, 0f, 1f);
            results.Add(new HeatingControlPrediction() { SupplyTemperature = clampedValue * 100f });
        }

        return results;
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

    private static float[] NormalizeInput(HeatingControlInputData inputData)
    {
        var nInput = new float[6];
        nInput[0] = Normalize(inputData.OutdoorTemperature, -60f, 60f, 0f, 1f);
        nInput[1] = Normalize(inputData.PredictedOutdoorTemperature, -60f, 60f, 0f, 1f);
        nInput[2] = Normalize(inputData.PreferredIndoorTemperature, 15f, 35f, 0f, 1f);

        nInput[3] = Normalize(inputData.Baseline, 0, 20f, 0f, 1f);
        nInput[4] = Normalize(inputData.Gradient, 0, 10f, 0f, 1f);
        nInput[5] = Normalize(inputData.MaxSupplyTemperature, 70f, 100f, 0f, 1f);

        return nInput;
    }

    private static float Normalize(float val, float valMin, float valMax, float min, float max) => (val - valMin) / (valMax - valMin) * (max - min) + min;
}