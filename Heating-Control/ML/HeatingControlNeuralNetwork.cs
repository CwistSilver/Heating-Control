using Heating_Control.Data;
using Heating_Control.ML.Storage;
using Heating_Control.ML.Trainer;
using Microsoft.ML;

namespace Heating_Control.ML;
public sealed class HeatingControlNeuralNetwork : IHeatingControlNeuralNetwork
{
    public TrainingDataOptions? UsedTrainingDataOptions { get; private set; }

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
        if (_predictionEngine is null)
            throw new InvalidOperationException("Neural network has not been initialized yet");

        var predictionResult = _predictionEngine.Predict(inputData);
        return predictionResult;
    }

    public void Inizialize()
    {
        if (_transformer is not null)
            throw new InvalidOperationException("Neural network has already been Inizialize");

        var modelData = _modelStorage.Load() ?? throw new FileLoadException("Neural network could not be loaded");
        _transformer = modelData.Transformer;
        UsedTrainingDataOptions = modelData.Options;

        _predictionEngine = new MLContext().Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(_transformer);
    }

    public async Task TrainModel(TrainingDataOptions? options = null)
    {
        _transformer = await _heatingControlTrainer.TrainNeuralNetworkAsync(options);
        _predictionEngine = new MLContext().Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(_transformer);
        UsedTrainingDataOptions = options is null ? new TrainingDataOptions() : options;
    }
}