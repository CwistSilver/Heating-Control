using Heating_Control.Data;
using Heating_Control.ML.Storage;
using Heating_Control.ML.Trainer;
using Microsoft.ML;

namespace Heating_Control.ML;
public sealed class HeatingControlNeuralNetwork : IHeatingControlNeuralNetwork
{
    private ITransformer? _transformer;
    private PredictionEngine<HeatingControlInputData, HeatingControlPrediction>? _predictionEngine;

    private readonly IHeatingControlTrainer _heatingControlTrainer;
    private readonly IModelStorage _modelStorage;

    public HeatingControlNeuralNetwork(IHeatingControlTrainer heatingControlTrainer, IModelStorage modelStorage)
    {
        _heatingControlTrainer = heatingControlTrainer;
        _modelStorage = modelStorage;
    }

    public HeatingControlPrediction Predict(HeatingControlInputData inputData)
    {
        if (_transformer is null || _predictionEngine is null)
            throw new Exception("Neural network has not been initialized yet");

        var predictionResult = _predictionEngine.Predict(inputData);
        return predictionResult;
    }

    public async Task Inizialize()
    {
        if (_transformer is not null)
            throw new Exception("Neural network has already been Inizialize");

        _transformer = _modelStorage.Load();
        _transformer ??= await _heatingControlTrainer.TrainNeuralNetworkAsync();

        _predictionEngine = new MLContext().Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(_transformer);
    }
}
