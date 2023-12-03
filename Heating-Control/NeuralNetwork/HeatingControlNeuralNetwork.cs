using Heating_Control.Data;
using Heating_Control.Storage;
using Microsoft.Extensions.Logging;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;

namespace Heating_Control.NeuralNetwork;
public sealed class HeatingControlNeuralNetwork : IHeatingControlNeuralNetwork
{
    private const float MaxSupplyTemperature = 100f;

    private IModel? _model;
    private readonly IModelStorage _modelStorage;
    private readonly ILogger<IHeatingControlNeuralNetwork> _logger;

    public HeatingControlNeuralNetwork(IModelStorage modelStorage, ILogger<IHeatingControlNeuralNetwork> logger)
    {
        _modelStorage = modelStorage;
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
        var normalizedInput = NormalizeInput(input);

        var inputTensor = np.array(normalizedInput);
        inputTensor = inputTensor.reshape(new Shape(1, -1));

        var prediction = _model!.predict(inputTensor, batch_size: 1);

        var predictionArray = prediction.numpy();
        return PostEffect(input, ref predictionArray.reshape(-1).ToArray<float>()[0]); ;
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
        var inputArray = inputs.ToArray();
        for (int i = 0; i < flattenedArray.Length; i++)
        {
            results.Add(PostEffect(inputArray[i], ref flattenedArray[i]));
        }

        return results;
    }



    private static HeatingControlPrediction PostEffect(HeatingControlInputData input, ref float prediction)
    {
        prediction = Math.Clamp(prediction, 0f, 1f) * MaxSupplyTemperature;

        var temperature = input.OutdoorTemperature;
        var insideOutsideTemperatureDifference = Math.Abs(input.PredictedOutdoorTemperature - input.OutdoorTemperature);
        if (input.PredictedOutdoorTemperature < temperature)
            temperature -= insideOutsideTemperatureDifference * 0.5f;

        if (temperature >= input.PreferredIndoorTemperature)
            return new HeatingControlPrediction() { SupplyTemperature = 0 };

        return new HeatingControlPrediction() { SupplyTemperature = prediction };
    }

    private static float[] NormalizeInput(HeatingControlInputData inputData)
    {
        var nInput = new float[6];
        nInput[0] = Normalize(inputData.OutdoorTemperature, -60f, 60f, 0f, 1f);
        nInput[1] = Normalize(inputData.PredictedOutdoorTemperature, -60f, 60f, 0f, 1f);
        nInput[2] = Normalize(inputData.PreferredIndoorTemperature, 15f, 35f, 0f, 1f);

        nInput[3] = Normalize(inputData.Baseline, 0, 20f, 0f, 1f);
        nInput[4] = Normalize(inputData.Gradient, 0, 10f, 0f, 1f);
        nInput[5] = Normalize(inputData.MaxSupplyTemperature, 70f, MaxSupplyTemperature, 0f, 1f);

        return nInput;
    }

    private static float Normalize(float val, float valMin, float valMax, float min, float max) => (val - valMin) / (valMax - valMin) * (max - min) + min;
}