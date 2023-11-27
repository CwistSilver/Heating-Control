using Heating_Control.ML;
using Heating_Control.ML.DataProvider;
using Heating_Control.ML.Storage;
using Heating_Control.ML.Trainer;
using Heating_Control.NeuralNetwork;
using Heating_Control.NeuralNetwork.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Heating_Control;
/// <summary>
/// Provides a centralized configuration method for setting up heating control services.
/// This class is responsible for adding all the necessary service dependencies to the service collection.
/// </summary>
public static class HeatingControlEntry
{
    /// <summary>
    /// Configures and adds essential heating control services to the provided service collection.
    /// </summary>
    /// <param name="services">The service collection to which the heating control services will be added.</param>
    public static void ConfigureServices(ServiceCollection services)
    {
        services.AddTransient<IModelStorage, ModelStorage>();
        services.AddTransient<ITrainingDataProvider, TrainingDataProvider>();
        services.AddTransient<IHeatingControlTrainer, TensorflowTrainer>();
        services.AddSingleton<IModelCreator, ModelCreator>();

        // TESTSSS
        services.AddSingleton<TensorflowNeuralNetwork>();




        services.AddSingleton<IHeatingControlNeuralNetwork, HeatingControlNeuralNetwork>();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }
}