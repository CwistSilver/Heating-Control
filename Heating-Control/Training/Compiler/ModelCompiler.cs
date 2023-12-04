using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;

namespace Heating_Control.Training.Compiler;
public sealed class ModelCompiler : IModelCompiler
{
    public void CompileModel(IModel model)
    {
        model.compile(optimizer: keras.optimizers.Adam(),
                        loss: keras.losses.MeanAbsoluteError(),
                        new[] { "mean_absolute_error" });
    }
}
