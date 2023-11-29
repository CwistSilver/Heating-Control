using Heating_Control.Data;
using Heating_Control.ML.DataProvider;
using Macademy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Macademy.TrainingSuite;

namespace ConsoleApp1;
public class MachineAcademy
{
    // https://github.com/zbendefy/machine.academy
    public async Task TrainAsync(ITrainingDataProvider trainingDataProvider)
    {
        List<TrainingSuite.TrainingData> trainingData = new List<TrainingSuite.TrainingData>();
        var traingDatas = await trainingDataProvider.GenerateAsync(new TrainingDataOptions() { RecordsToGenerate = 1_000 });

        foreach (var traingData in traingDatas)
        {
            float[] trainingInput = new float[3] { traingData.OutdoorTemperature, traingData.PreferredIndoorTemperature, traingData.PredictedOutdoorTemperature };
            float[] desiredOutput = new float[1] { traingData.SupplyTemperature / 90f};
            trainingData.Add(new TrainingSuite.TrainingData(trainingInput, desiredOutput));
        }

        var all = ComputeDeviceFactory.GetComputeDevices();
        var item = all[1];
        //OpenCL.OpenCLComputeDeviceDesc test = null;

        ComputeDevice computeDevice = item.CreateDevice(); //Get an OpenCL device

        var layerConfig = new int[] { 3, 15, 1 };
        var network = Network.CreateNetworkInitRandom(layerConfig, new SigmoidActivation());

        TrainingSuite trainingSuite = new TrainingSuite(trainingData);

        //Set up training configuration
        trainingSuite.config.epochs = 10_000;
        trainingSuite.config.shuffleTrainingData = true;
        trainingSuite.config.miniBatchSize = 50;
        trainingSuite.config.learningRate = 0.01f;
        trainingSuite.config.costFunction = new MeanSquaredErrorFunction();
        trainingSuite.config.regularization = TrainingSuite.TrainingConfig.Regularization.L2;
        trainingSuite.config.regularizationLambda = 0.5f;

        var trainingPromise = network.Train(trainingSuite, computeDevice);

        var p = trainingPromise.GetTotalProgress();
        while (p != 1)
        {
            p = trainingPromise.GetTotalProgress();
            Console.WriteLine($"{p}%");
            await Task.Delay(1000);
        }
     

        trainingPromise.Await();



        var testDatas = await trainingDataProvider.GenerateAsync(new TrainingDataOptions() { RecordsToGenerate = 20 });

        foreach (var data in testDatas)
        {
            Console.WriteLine();
            var inputData = new HeatingControlInputData() { OutdoorTemperature = data.OutdoorTemperature, PredictedOutdoorTemperature = data.PredictedOutdoorTemperature, PreferredIndoorTemperature = data.PreferredIndoorTemperature };
            float[] input = new float[3] { inputData.OutdoorTemperature, inputData.PreferredIndoorTemperature, inputData.PredictedOutdoorTemperature };
            float[] results = network.Compute(input, computeDevice);

            var prediction = results[0] * 90f;
            Console.WriteLine($"Vorhergesagte Vorlauftemperatur für Testdaten: {prediction}");
            Console.WriteLine($"Tatsächliche Vorlauftemperatur der Testdaten: {data.SupplyTemperature}");
        }


    }


}
