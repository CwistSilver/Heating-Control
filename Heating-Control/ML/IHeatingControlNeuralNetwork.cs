﻿using Heating_Control.Data;

namespace Heating_Control.ML;
public interface IHeatingControlNeuralNetwork
{
    Task Inizialize(TrainingDataOptions? options = null, bool retrain = false);

    HeatingControlPrediction Predict(HeatingControlInputData inputData);
}
