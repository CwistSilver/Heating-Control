using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using System;
using Splat;
using System.Threading.Tasks;
using Heating_Control.Data;
using Heating_Control.ML;
using Heating_Control_UI.Utilities;

namespace Heating_Control_UI.ViewModels;
public class HeatingControlSettingsViewModel : ViewModelBase
{

    private readonly IHeatingControlNeuralNetwork _heatingControlNeuralNetwork;

    public HeatingControlSettingsViewModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        if (heatingControlNeuralNetwork is null) return;
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;

        CreatCommands();
    }


    public HeatingControlSettingsViewModel()
    {
        CreatCommands();
    }

    private void CreatCommands()
    {
        if (_heatingControlNeuralNetwork.UsedTrainingDataOptions is not null)
        {
            Baseline = _heatingControlNeuralNetwork.UsedTrainingDataOptions.Baseline;
            Gradient = _heatingControlNeuralNetwork.UsedTrainingDataOptions.Gradient;
            MaxSupplyTemperature = _heatingControlNeuralNetwork.UsedTrainingDataOptions.MaxSupplyTemperature;
        }

        Save = ReactiveCommand.CreateFromTask(SaveImpl);
        Save.IsExecuting.ToProperty(this, x => x.IsSaving, out _isSaving);
        Save.ThrownExceptions.Subscribe(ex => this.Log().ErrorException("Something went wrong", ex));
    }



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


    public async void CancelAction()
    {
        //App.Navigator.Pop();
        await App.Navigator.PopAsync(PageNavigator.DefaultHorizontalSlideTransition);
    }

    public ReactiveCommand<Unit, Unit> Save { get; private set; }

    ObservableAsPropertyHelper<bool> _isSaving;
    public bool IsSaving { get { return _isSaving.Value; } }

    public async Task SaveImpl()
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
            _heatingControlNeuralNetwork.Inizialize(options, true)
        };

        await Task.WhenAll(tasks);
        await App.Navigator.PopAsync(PageNavigator.DefaultHorizontalSlideTransition);

    }
}

