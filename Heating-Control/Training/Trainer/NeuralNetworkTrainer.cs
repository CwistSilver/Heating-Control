using Heating_Control.Data.Training;
using Heating_Control.Normalizer;
using Heating_Control.Training.Compiler;
using Heating_Control.Training.DataProvider;
using Heating_Control.Training.Storage;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;
using Tensorflow.Util;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;


namespace Heating_Control.Training.Trainer;
public sealed class NeuralNetworkTrainer : INeuralNetworkTrainer
{
    private readonly ITrainingDataProvider _trainingDataProvider;
    private readonly ITrainingStorage _trainingStorage;
    private readonly IModelCompiler _modelCompiler;
    private readonly IDataNormalizer _dataNormalizer;

    public NeuralNetworkTrainer(ITrainingDataProvider trainingDataProvider, ITrainingStorage trainingStorage, IModelCompiler modelCompiler, IDataNormalizer dataNormalizer)
    {
        _trainingDataProvider = trainingDataProvider;
        _trainingStorage = trainingStorage;
        _modelCompiler = modelCompiler;
        _dataNormalizer = dataNormalizer;
        tf.device("/gpu:0");
    }

    public void Train(NeuralNetworkConfig neuralNetworkConfig, int startEpoch = 1)
    {
        InitializeModel(neuralNetworkConfig);

        _trainingStorage.SaveState(neuralNetworkConfig, new TrainingState() { CurrentEpoch = 1 });

        float bestValLoss = float.MaxValue;

        string savePath = _trainingStorage.GetModelFilePath(neuralNetworkConfig);

        var validationData = CreateValidationData(neuralNetworkConfig);


        if (startEpoch != 1)
        {
            

            var newModel = _trainingStorage.Load(neuralNetworkConfig);
            if (newModel is not null)
            {
                neuralNetworkConfig.KerasSequentialModel = (Sequential)newModel;
                _modelCompiler.CompileModel(neuralNetworkConfig.KerasSequentialModel);
            }

        }

        for (int epoch = startEpoch; epoch <= neuralNetworkConfig.Epochs; epoch++)
        {
            Console.WriteLine($"{Environment.NewLine}{epoch / (float)neuralNetworkConfig.Epochs * 100f}%");

            var (inputsArray, outputsArray) = CreateTriningData(neuralNetworkConfig);

            // Trainieren für eine Epoche
            var history = neuralNetworkConfig.KerasSequentialModel!.fit(inputsArray, outputsArray, batch_size: neuralNetworkConfig.BatchSize, initial_epoch: epoch, epochs: epoch + 1, validation_data: validationData);
            _trainingStorage.SaveState(neuralNetworkConfig, new TrainingState() { CurrentEpoch = epoch + 1 });

            inputsArray.Dispose();
            outputsArray.Dispose();

            // Überprüfen des Validierungsverlustes
            float valLoss = history.history["val_loss"][0];
            if (valLoss < bestValLoss)
            {
                bestValLoss = valLoss;
                neuralNetworkConfig.KerasSequentialModel.save(savePath);
            }

        }
        _trainingStorage.SaveState(neuralNetworkConfig, new TrainingState() { CurrentEpoch = neuralNetworkConfig.Epochs, Finished = true });

        Console.WriteLine($"Bestes Modell gespeichert unter: {savePath}");
    }

    private void InitializeModel(NeuralNetworkConfig neuralNetworkConfig)
    {
        // Erstellen des Modells
        neuralNetworkConfig.KerasSequentialModel = keras.Sequential();
        neuralNetworkConfig.KerasSequentialModel.add(keras.Input(6));

        foreach (var layer in neuralNetworkConfig.Layers)
        {
            foreach (var innerLayer in layer.CreateLayers())
            {
                neuralNetworkConfig.KerasSequentialModel.add(innerLayer);
            }
        }
        neuralNetworkConfig.KerasSequentialModel.add(keras.layers.Dense(1, activation: keras.activations.Linear)); // Ausgabeschicht

        // Kompilieren des Modells
        _modelCompiler.CompileModel(neuralNetworkConfig.KerasSequentialModel);
    }



    private ValidationDataPack CreateValidationData(NeuralNetworkConfig neuralNetworkConfig)
    {
        var validTrainingData = _trainingDataProvider.Generate(neuralNetworkConfig.ValidationDatas);
        var validInputsList = new List<float>(neuralNetworkConfig.ValidationDatas);
        var validOutputsList = new List<float>(neuralNetworkConfig.ValidationDatas);

        foreach (var item in validTrainingData)
        {
            validInputsList.AddRange(_dataNormalizer.NormalizeInput(item));
            validOutputsList.Add(_dataNormalizer.NormalizeOutput(item.SupplyTemperature));
        }

        // Konvertierung in NumPy Arrays
        var validInputs = np.array(validInputsList.ToArray());
        var validOutputs = np.array(validOutputsList.ToArray());
        validInputs = validInputs.reshape(new Shape(neuralNetworkConfig.ValidationDatas, -1));
        validOutputs = validOutputs.reshape(new Shape(neuralNetworkConfig.ValidationDatas, -1));
        return new ValidationDataPack((validInputs, validOutputs));
    }

    private (NDArray inputsArray, NDArray outputArray) CreateTriningData(NeuralNetworkConfig neuralNetworkConfig)
    {
        var validTrainingData = _trainingDataProvider.Generate(neuralNetworkConfig.TrainingSize);
        var inputsList = new List<float>(neuralNetworkConfig.TrainingSize * 6);
        var outputsList = new List<float>(neuralNetworkConfig.TrainingSize);

        foreach (var item in validTrainingData)
        {
            inputsList.AddRange(_dataNormalizer.NormalizeInput(item));
            outputsList.Add(_dataNormalizer.NormalizeOutput(item.SupplyTemperature));
        }

        // Konvertierung in NumPy Arrays
        var inputsArray = np.array(inputsList.ToArray());
        var outputsArray = np.array(outputsList.ToArray());
        inputsArray = inputsArray.reshape(new Shape(neuralNetworkConfig.TrainingSize, -1));
        outputsArray = outputsArray.reshape(new Shape(neuralNetworkConfig.TrainingSize, -1));

        return (inputsArray, outputsArray);
    }
}