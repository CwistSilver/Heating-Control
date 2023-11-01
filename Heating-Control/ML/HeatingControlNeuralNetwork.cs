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
        if (_transformer is null || _predictionEngine is null)
            throw new Exception("Neural network has not been initialized yet");

        var predictionResult = _predictionEngine.Predict(inputData);
        return predictionResult;
    }

    public async Task Inizialize(TrainingDataOptions? options = null, bool retrain = false)
    {
        if (retrain)
        {
            await TrainModel(options);
            return;
        }

        if (_transformer is not null)
            throw new Exception("Neural network has already been Inizialize");

        var modelData = _modelStorage.Load();
        if (modelData is null)
        {
            await TrainModel(options);
            return;
        }

        _transformer = modelData.Transformer;
        UsedTrainingDataOptions = modelData.Options;

        _predictionEngine = new MLContext().Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(_transformer);
    }

    private async Task TrainModel(TrainingDataOptions? options)
    {
        _transformer = await _heatingControlTrainer.TrainNeuralNetworkAsync(options);
        _predictionEngine = new MLContext().Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(_transformer);
        UsedTrainingDataOptions = options is null ? new TrainingDataOptions() : options;
    }
}
