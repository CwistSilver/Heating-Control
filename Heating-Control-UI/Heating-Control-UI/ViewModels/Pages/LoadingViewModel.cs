using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Heating_Control.ML;
using Heating_Control_UI.Views.Pages;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Heating_Control_UI.ViewModels.Pages;
public class LoadingViewModel : ViewModelBase
{
    private const string DayChartKey = nameof(DayChart);
    private const string HeatingControlViewKey = nameof(HeatingControlView);

    private readonly IHeatingControlNeuralNetwork? _heatingControlNeuralNetwork;
    private bool _isInizialized;
    private bool _isAnimationFinished;

    public LoadingViewModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        _ = LoadNeuralNetwork();
    }

    public async Task SkipRequest()
    {
        if (_isInizialized)
            await Fade(1_000);
    }

    private async Task LoadNeuralNetwork()
    {
        if (_heatingControlNeuralNetwork is null) return;

        try
        {
            _heatingControlNeuralNetwork.Inizialize();
        }catch
        {
            await _heatingControlNeuralNetwork.TrainModel();
        }
        await OnFinishedInizializeNeuralNetwork();
    }

    public async Task OnFinishedInizializeNeuralNetwork()
    {
        _isInizialized = true;

        if (_isAnimationFinished)
            await Fade();
    }

    public async Task OnFinishedAnimation()
    {
        if (_isAnimationFinished) return;
        _isAnimationFinished = true;

        if (_isInizialized)
            await Fade();
    }

    private async Task Fade(double duration = 1_000)
    {
        _isAnimationFinished = true;
        var pageTransition = new CrossFade(TimeSpan.FromMilliseconds(duration));
        pageTransition.FadeOutEasing = new QuinticEaseOut();
        pageTransition.FadeInEasing = new QuinticEaseIn();

        var lastOpendPage = App.Storage.Get(nameof(SelectedMode), SelectedMode);
        switch (lastOpendPage)
        {
            case DayChartKey:
                await App.Navigator.PushAsync<DayChart>(pageTransition);
                break;
            case HeatingControlViewKey:
                await App.Navigator.PushAsync<HeatingControlView>(pageTransition);
                break;
        }

        App.Navigator.DestroyPage<LoadingView>();
    }
}
