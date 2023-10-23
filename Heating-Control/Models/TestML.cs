using Heating_Control.Data;
using Microsoft.ML;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace Heating_Control.Models;
public class TestML
{
    private static string _dataPath = "C:\\Users\\Gabriel.Zerbe\\Downloads\\Vorlauftemperatur.json"; // Pfad zur JSON-Datei
    private static string _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "model.zip"); // Pfad, um das trainierte Modell zu speichern
    public static void Train()
    {
        // 1. Trainingsdaten aus JSON-Datei lesen
        //var trainingData = LoadTrainingData();
        var trainingData = RandomTestDataGenerator.CreateRandomData();

        // 2. Daten in IDataView konvertieren
        var context = new MLContext();
        IDataView dataView = context.Data.LoadFromEnumerable(trainingData);

        // 3. Datenpipeline und Modelltraining
        //var pipeline = context.Transforms.Concatenate("Features", "OutdoorTemperature", "PredictedOutdoorTemperature", "PreferredIndoorTemperature")
        //  .Append(context.Transforms.NormalizeMinMax("Features"))
        //  .Append(context.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

        var pipeline = context.Transforms.Concatenate("Features", "OutdoorTemperature", "PredictedOutdoorTemperature", "PreferredIndoorTemperature")
    .Append(context.Transforms.NormalizeMinMax("Features"))
    .Append(context.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));


        var model = pipeline.Fit(dataView);

        // Modell speichern (optional, aber hilfreich, wenn Sie das Modell später wiederverwenden möchten)
        SaveModel(context, model, dataView.Schema);

        Console.WriteLine("Modelltraining abgeschlossen und gespeichert!");

        // Test einer Vorhersage mit dem trainierten Modell
        TestModel(context, model);
    }

    private static void TestModel(MLContext context, ITransformer model)
    {
        var predictionEngine = context.Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(model);


        for (int i = 0; i < 20; i++)
        {
            //var input = new HeatingControlInputData
            //{
            //    OutdoorTemperature = 10,
            //    PredictedOutdoorTemperature = 10,
            //    PreferredIndoorTemperature = 20 + i
            //};
            var input = HeatingControlInputData.CreateRandom(out var supplyTemperature);
            Console.WriteLine($"Get Temperature for: {JsonSerializer.Serialize(input)}");
            var prediction = predictionEngine.Predict(input);
            Console.WriteLine($"SupplyTemperature: {supplyTemperature} | SupplyTemperature (AI prediction): {prediction.SupplyTemperature}");
            Console.WriteLine();
        }
     
    }

    private static void SaveModel(MLContext context, ITransformer model, DataViewSchema inputSchema)
    {
        context.Model.Save(model, inputSchema, _modelPath);

        Console.Write("Saved Model to ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(TerminalURL(Path.GetFileName(_modelPath), _modelPath));
        Console.ResetColor();
        Console.WriteLine();
    }

    private static List<HeatingControlTrainingData> LoadTrainingData()
    {
        Console.Write("Load Training Data '");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(TerminalURL(Path.GetFileName(_dataPath), _dataPath));
        Console.ResetColor();
        Console.Write($"' frm directory {Path.GetDirectoryName(_dataPath)}.");
        Console.WriteLine();
        var fileContent = File.ReadAllText(_dataPath);
        // 1. Trainingsdaten aus JSON-Datei lesen
        return JsonSerializer.Deserialize<List<HeatingControlTrainingData>>(fileContent);
    }

    private static string TerminalURL(string caption, string url) => $"\u001B]8;;{url}\a{caption}\u001B]8;;\a";
}
