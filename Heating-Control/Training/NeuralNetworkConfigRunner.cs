using Heating_Control.Data;
using Heating_Control.Data.Training;
using Heating_Control.Training.Compiler;
using Heating_Control.Training.DataProvider;
using Heating_Control.Training.Storage;
using Heating_Control.Training.Trainer;
using System.Diagnostics;
using System.Text.Json;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;

namespace Heating_Control.Training;
public class NeuralNetworkConfigRunner : INeuralNetworkConfigRunner
{
    private readonly ITrainingStorage _trainingStorage;
    private readonly INeuralNetworkTrainer _neuralNetworkTrainer;
    private readonly IModelCompiler _modelCompiler;

    public NeuralNetworkConfigRunner(ITrainingStorage trainingStorage, INeuralNetworkTrainer neuralNetworkTrainer, IModelCompiler modelCompiler)
    {
        _trainingStorage = trainingStorage;
        _neuralNetworkTrainer = neuralNetworkTrainer;
        _modelCompiler = modelCompiler;
    }


    public void Train(NeuralNetworkConfig neuralNetworkConfig)
    {
        var stopwatch = Stopwatch.StartNew();

        TrainingState? state = _trainingStorage.LoadState(neuralNetworkConfig);
        if (state is not null && state.Finished)
            state = null;

        if (state is null)
            _neuralNetworkTrainer.Train(neuralNetworkConfig);
        else
        {
            _neuralNetworkTrainer.Train(neuralNetworkConfig, state.CurrentEpoch);
        }

        stopwatch.Stop();
        CalculatePrecision(neuralNetworkConfig, stopwatch.Elapsed);
    }

    private void CalculatePrecision(NeuralNetworkConfig neuralNetworkConfig, TimeSpan trainDuration)
    {
        var trainingDataProvider = new TrainingDataProvider();
        var dataSet = trainingDataProvider.Generate(300);

        var predictions = new List<float>();
        var actualValues = new List<float>();

        foreach (var data in dataSet)
        {
            var prediction = Predict(neuralNetworkConfig, data);
            predictions.Add(prediction);
            actualValues.Add(data.SupplyTemperature);
        }

        // Berechnung der Abweichungen
        var deviations = predictions.Zip(actualValues, (predicted, actual) => Math.Abs(predicted - actual)).ToList();

        neuralNetworkConfig.Results = new ModelResults
        {
            ReadableDuration = ToReadableString(trainDuration),
            Duration = trainDuration,
            Median = (float)Median(deviations),
            Average = deviations.Average(),
            MaxDeviation = deviations.Max(),
            MinDeviation = deviations.Min()
        };

        Console.WriteLine($"Median Deviation: {neuralNetworkConfig.Results.Median}");
        Console.WriteLine($"Average Deviation: {neuralNetworkConfig.Results.Average}");
        Console.WriteLine($"Max Deviation: {neuralNetworkConfig.Results.MaxDeviation}");
        Console.WriteLine($"Min Deviation: {neuralNetworkConfig.Results.MinDeviation}");

        EvaluateModel(neuralNetworkConfig, dataSet);

        var savePath = _trainingStorage.GetModelConfigDataFilePath(neuralNetworkConfig);
        var jsonText = JsonSerializer.Serialize(neuralNetworkConfig, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(savePath, jsonText);

        Console.WriteLine($"{Environment.NewLine}Save Path: {savePath}");
    }

    private void EvaluateModel(NeuralNetworkConfig neuralNetworkConfig, IEnumerable<HeatingControlTrainingData> dataSet)
    {
        if (neuralNetworkConfig.KerasModel is null)
        {
            neuralNetworkConfig.KerasModel = (Sequential)_trainingStorage.Load(neuralNetworkConfig);
            if (neuralNetworkConfig.KerasModel is null)
                throw new Exception("Failed to load!");
        }

        _modelCompiler.CompileModel(neuralNetworkConfig.KerasModel);

        var testInputsList = new List<float>();
        var testOutputsList = new List<float>();

        foreach (var item in dataSet)
        {
            testInputsList.AddRange(NormalizeInput(item));
            testOutputsList.Add(NormalizeOutput(item.SupplyTemperature));
        }

        // Umwandeln in NumPy Arrays
        var testInputsArray = np.array(testInputsList.ToArray()).reshape(new Shape(dataSet.Count(), -1));
        var testOutputsArray = np.array(testOutputsList.ToArray()).reshape(new Shape(dataSet.Count(), -1));

        // Bewertung des Modells
        var evaluation = neuralNetworkConfig.KerasModel.evaluate(testInputsArray, testOutputsArray);
        foreach (var item in evaluation)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
            neuralNetworkConfig.Results!.EvaluateResults.Add(item.Key, item.Value);
        }

    }

    private static string ToReadableString(TimeSpan span)
    {
        string formatted = string.Format("{0}{1}{2}{3}",
            span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
            span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
            span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
            span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

        if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

        if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

        return formatted;
    }

    private static double Median(IEnumerable<float> source)
    {
        var sortedList = source.OrderBy(number => number).ToList();
        int itemIndex = sortedList.Count / 2;

        if (sortedList.Count % 2 == 0)
        {
            return (sortedList[itemIndex] + sortedList[itemIndex - 1]) / 2;
        }
        else
        {
            return sortedList[itemIndex];
        }
    }



    public float Predict(NeuralNetworkConfig neuralNetworkConfig, HeatingControlInputData inputData)
    {
        if (neuralNetworkConfig.KerasModel is null)
        {
            neuralNetworkConfig.KerasModel = (Sequential)_trainingStorage.Load(neuralNetworkConfig);
            if (neuralNetworkConfig.KerasModel is null)
                throw new Exception("Failed to load!");
        }

        var normalizedInput = NormalizeInput(inputData);

        // Konvertieren des eindimensionalen Arrays in ein Tensorflow Tensor
        var inputTensor = np.array(normalizedInput);
        inputTensor = inputTensor.reshape(new Shape(1, -1)); // Umschichten zu einer Form mit einer Zeile

        var prediction = neuralNetworkConfig.KerasModel!.predict(inputTensor, batch_size: 1);

        // Konvertierung des Tensor-Ergebnisses in ein float-Array
        var predictionArray = prediction.numpy();
        var floatValue = predictionArray.reshape(-1).ToArray<float>()[0];
        floatValue = Math.Clamp(floatValue, 0f, 1f);

        return floatValue * 100f;
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

    private static float Normalize(float val, float valMin, float valMax, float min, float max)
    {
        return (((val - valMin) / (valMax - valMin)) * (max - min)) + min;
    }

    private static float NormalizeOutput(float supplyTemperature)
    {
        return supplyTemperature / 100f;
    }
}
