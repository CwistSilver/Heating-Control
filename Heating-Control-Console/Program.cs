using Heating_Control.Models;
using System.Text.Json;
namespace Heating_Control_Console;
internal class Program
{
    //private static string _dataPath = "./data.json"; // Pfad zur JSON-Datei
    //private static string _modelPath = "./model.zip"; // Pfad, um das trainierte Modell zu speichern

    static void Main(string[] args)
    {
        TestML.Train();
    }
}