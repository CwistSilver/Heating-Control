using Heating_Control.Data;

namespace Heating_Control.NeuralNetwork.PostEffect;
public interface IPredictionPostEffect
{
    HeatingControlPrediction AddPostEffect(HeatingControlInputData input, float prediction);
}