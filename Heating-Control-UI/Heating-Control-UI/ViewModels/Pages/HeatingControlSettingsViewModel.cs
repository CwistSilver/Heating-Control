using Heating_Control.Data;
using Heating_Control.ML;
using Heating_Control_UI.Utilities.Navigation;
using ReactiveUI;
using System.Threading.Tasks;

namespace Heating_Control_UI.ViewModels.Pages;
public class HeatingControlSettingsViewModel : ViewModelBase
{
    private float _maxSupplyTemperature = 90f;
    public float MaxSupplyTemperature
    {
        get => _maxSupplyTemperature;
        set => this.RaiseAndSetIfChanged(ref _maxSupplyTemperature, value);
    }
    private float _gradient = 1.5f;
    public float Gradient
    {
        get => _gradient;
        set => this.RaiseAndSetIfChanged(ref _gradient, value);
    }

    private float _baseline = 1.5f;
    public float Baseline
    {
        get => _baseline;
        set => this.RaiseAndSetIfChanged(ref _baseline, value);
    }

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        set => this.RaiseAndSetIfChanged(ref _isSaving, value);
    }


    private readonly IHeatingControlNeuralNetwork? _heatingControlNeuralNetwork;
    public HeatingControlSettingsViewModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        if (heatingControlNeuralNetwork is null) return;
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;

        LoadCurrentSettings();
    }

    public HeatingControlSettingsViewModel() { }

    private void LoadCurrentSettings()
    {
        if (_heatingControlNeuralNetwork?.UsedTrainingDataOptions is not null)
        {
            Baseline = _heatingControlNeuralNetwork.UsedTrainingDataOptions.Baseline;
            Gradient = _heatingControlNeuralNetwork.UsedTrainingDataOptions.Gradient;
            MaxSupplyTemperature = _heatingControlNeuralNetwork.UsedTrainingDataOptions.MaxSupplyTemperature;
        }
    }

    public async void CancelAction() => await App.Navigator.PopAsync(PageNavigator.DefaultHorizontalSlideTransition);

    public async void Save()
    {
        if (_heatingControlNeuralNetwork is null) return;

        IsSaving = true;
        try
        {
            var options = new TrainingDataOptions()
            {
                Baseline = _baseline,
                Gradient = _gradient,
                MaxSupplyTemperature = _maxSupplyTemperature,
            };

            var tasks = new Task[2]
            {
            Task.Delay(3_000),
            _heatingControlNeuralNetwork.TrainModel(options)
            };

            await Task.WhenAll(tasks);
            await App.Navigator.PopAsync(PageNavigator.DefaultHorizontalSlideTransition);
        }
        finally
        {
            IsSaving = false;
        }
    }
}

