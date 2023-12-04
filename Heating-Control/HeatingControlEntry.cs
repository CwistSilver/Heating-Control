using Heating_Control.NeuralNetwork;
using Heating_Control.NeuralNetwork.PostEffect;
using Heating_Control.Normalizer;
using Heating_Control.Storage;
using Heating_Control.Training;
using Heating_Control.Training.Compiler;
using Heating_Control.Training.DataProvider;
using Heating_Control.Training.Storage;
using Heating_Control.Training.Trainer;
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
        services.AddSingleton<IHeatingControlNeuralNetwork, HeatingControlNeuralNetwork>();

        services.AddTransient<INeuralNetworkConfigRunner, NeuralNetworkConfigRunner>();
        services.AddTransient<ITrainingStorage, TrainingStorage>();
        services.AddTransient<ITrainingDataProvider, TrainingDataProvider>();
        services.AddTransient<INeuralNetworkTrainer, NeuralNetworkTrainer>();
        services.AddTransient<IModelCompiler, ModelCompiler>();
        services.AddSingleton<IDataNormalizer, DataNormalizer>();
        services.AddSingleton<IPredictionPostEffect, PredictionPostEffect>();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }
}