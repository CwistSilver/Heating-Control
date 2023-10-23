using Heating_Control.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace Heating_Control.Models;
public class TestML
{
    private static MLContext context;
    public static ITransformer model;
    public static IDataView dataView;
    private static string _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "model.zip"); // Pfad, um das trainierte Modell zu speichern
    public static void Train()
    {
        // 1. Trainingsdaten aus JSON-Datei lesen
        //var trainingData = LoadTrainingData();
        if (File.Exists(_modelPath))
        {
            Console.WriteLine("Machine Learning model already exists, do you want to retrain the Machine Learning model?");
            Console.WriteLine("(Press Y to tain the Machine Learning model again)");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.Y)
            {
                Console.WriteLine("Train Machine Learning model.");
                TrainModel();
                Console.WriteLine("Modelltraining abgeschlossen und gespeichert!");
                SaveModel(context, model, dataView.Schema);
            }
            else
            {
                LoadModel();
            }

        }
        else
        {
            Console.WriteLine("Train Machine Learning model.");
            TrainModel();
            Console.WriteLine("Modelltraining abgeschlossen und gespeichert!");
            SaveModel(context, model, dataView.Schema);
        }


        // Modell speichern (optional, aber hilfreich, wenn Sie das Modell später wiederverwenden möchten)
      

  

        // Test einer Vorhersage mit dem trainierten Modell
        TestModel(context, model);
    }

    private static void TrainModel()
    {
        var testDataCount = 2_000_000;
        Console.WriteLine($"Create {testDataCount} TestData");
        var trainingData = RandomTestDataGenerator.CreateRandomData(testDataCount);
        Console.WriteLine($"Finished to Create TestData");
        // 2. Daten in IDataView konvertieren
        context = new MLContext();
        dataView = context.Data.LoadFromEnumerable(trainingData);

        // 3. Datenpipeline und Modelltraining
        //var pipeline = context.Transforms.Concatenate("Features", "OutdoorTemperature", "PredictedOutdoorTemperature", "PreferredIndoorTemperature")
        //  .Append(context.Transforms.NormalizeMinMax("Features"))
        //  .Append(context.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

        var pipeline = context.Transforms.Concatenate("Features", "OutdoorTemperature", "PredictedOutdoorTemperature", "PreferredIndoorTemperature")
    .Append(context.Transforms.NormalizeMinMax("Features"))
    .Append(context.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));

        Console.WriteLine($"Start Training.");
        var start = DateTime.Now;
        model = pipeline.Fit(dataView);
        var end = DateTime.Now - start;
        Console.WriteLine($"Finished! Duration: {end}");
    }

    private static void TestModel(MLContext context, ITransformer model)
    {
        var predictionEngine = context.Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(model);


        for (int i = 0; i < 20; i++)
        {
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

    private static DataViewSchema LoadModel()
    {
        Console.Write("Load Model from ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(TerminalURL(Path.GetFileName(_modelPath), _modelPath));
        Console.ResetColor();
        Console.WriteLine();
        context = new MLContext();
        model =  context.Model.Load(_modelPath, out var inputSchema);
      
        return inputSchema;
     
    }

    private static string TerminalURL(string caption, string url) => $"\u001B]8;;{url}\a{caption}\u001B]8;;\a";
}
