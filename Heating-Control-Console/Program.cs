using System.Text.Json;
namespace Heating_Control_Console;
internal class Program
{
    //private static string _dataPath = "./data.json"; // Pfad zur JSON-Datei
    //private static string _modelPath = "./model.zip"; // Pfad, um das trainierte Modell zu speichern

    static void Main(string[] args)
    {
        //// 1. Trainingsdaten aus JSON-Datei lesen
        //var trainingData = JsonSerializer.Deserialize<List<HeatingSystemTrainingData>>(File.ReadAllText(_dataPath));

        //// 2. Daten in IDataView konvertieren
        //var context = new MLContext();
        //IDataView dataView = context.Data.LoadFromEnumerable(trainingData);

        //// 3. Datenpipeline und Modelltraining
        //var pipeline = context.Transforms.Concatenate("Features", "OutdoorTemperature", "PredictedOutdoorTemperature", "PreferredIndoorTemperature")
        //                                    .Append(context.Regression.Trainers.FastTreeTweedie(labelColumnName: "SupplyTemperature", featureColumnName: "Features"));

        //var model = pipeline.Fit(dataView);

        //// Modell speichern (optional, aber hilfreich, wenn Sie das Modell später wiederverwenden möchten)
        //context.Model.Save(model, dataView.Schema, _modelPath);

        //Console.WriteLine("Modelltraining abgeschlossen und gespeichert!");

        //// Test einer Vorhersage mit dem trainierten Modell
        //var predictionEngine = context.Model.CreatePredictionEngine<HeatingSystemInputData, HeatingSystemPrediction>(model);

        //var input = new HeatingSystemInputData
        //{
        //    OutdoorTemperature = 5,
        //    PredictedOutdoorTemperature = -2,
        //    PreferredIndoorTemperature = 23
        //};

        //var prediction = predictionEngine.Predict(input);
        //Console.WriteLine($"Vorlauftemperatur: {prediction.SupplyTemperature}");
    }
}