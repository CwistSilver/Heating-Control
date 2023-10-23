using Heating_Control;
using Heating_Control.Data;
using Heating_Control.ML;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Heating_Control_Console;
internal class Program
{
    static ServiceCollection services = new ServiceCollection();
    static async Task Main(string[] args)
    {


        HeatingControlEntry.ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        var heatingControlNeuralNetwork = serviceProvider.GetRequiredService<IHeatingControlNeuralNetwork>();

        await heatingControlNeuralNetwork.Inizialize();

        for (int i = 0; i < 20; i++)
        {
            var input = HeatingControlInputData.CreateRandom(out var supplyTemperature);
            Console.WriteLine($"Get Temperature for: {JsonSerializer.Serialize(input)}");
            var prediction = heatingControlNeuralNetwork.Predict(input);
            Console.WriteLine($"SupplyTemperature: {supplyTemperature} | SupplyTemperature (AI prediction): {prediction.SupplyTemperature}");
            Console.WriteLine();
        }
    }
}