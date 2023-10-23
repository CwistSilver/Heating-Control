using Heating_Control.ML;
using Heating_Control.ML.DataProvider;
using Heating_Control.ML.Storage;
using Heating_Control.ML.Trainer;
using Microsoft.Extensions.DependencyInjection;

namespace Heating_Control;
public static class HeatingControlEntry
{
    public static void ConfigureServices(ServiceCollection services)
    {
        services.AddTransient<IModelStorage, ModelStorage>();
        services.AddTransient<ITrainingDataProvider, TrainingDataProvider>();
        services.AddTransient<IHeatingControlTrainer, HeatingControlTrainer>();

        services.AddSingleton<IHeatingControlNeuralNetwork, HeatingControlNeuralNetwork>();
    }
}
