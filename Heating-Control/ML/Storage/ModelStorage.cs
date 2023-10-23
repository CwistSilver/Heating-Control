using Microsoft.ML;

namespace Heating_Control.ML.Storage;
public sealed class ModelStorage : IModelStorage
{
    private const string FileName = "HeatingControl.ml";
    private readonly string _storagePath;
    public ModelStorage()
    {
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);
    }

    public ITransformer? Load()
    {
        try
        {
            var context = new MLContext();
            return context.Model.Load(_storagePath, out _);
        }
        catch
        {
            return null;
        }
    }
    public void Save(ITransformer transformer, DataViewSchema inputSchema)
    {
        var context = new MLContext();
        context.Model.Save(transformer, inputSchema, _storagePath);
    }
}
