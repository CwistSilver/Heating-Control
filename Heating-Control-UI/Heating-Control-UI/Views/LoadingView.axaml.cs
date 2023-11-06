using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Heating_Control.ML;
using System;
using System.Threading.Tasks;

namespace Heating_Control_UI;

public partial class LoadingView : UserControl
{
    private readonly IHeatingControlNeuralNetwork? _heatingControlNeuralNetwork;
    public LoadingView()
    {
        InitializeComponent();
    }

    public LoadingView(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        InitializeComponent();
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        this.Loaded += LoadingView_Loaded;
        this.PointerPressed += LoadingView_PointerPressed;
    }

   
    private async void LoadingView_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (_isInizialized)
            await Fade(1_000);
    }

    private bool _isInizialized;
    private bool _isAnimationFinished;
    private async void LoadingView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_heatingControlNeuralNetwork is null) return;

        await _heatingControlNeuralNetwork.Inizialize();
        await OnFinishedInizialize();
    }

    private async Task OnFinishedInizialize()
    {
        _isInizialized = true;

        if (_isAnimationFinished)
            await Fade();
    }

    private async void TemperaturProgressBarControl_Finished(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
        await App.Navigator.PushAsync<HeatingControlView>(pageTransition);
        App.Navigator.DestroyPage(this);
    }

    //private async void UserControl_Loaded_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    //{
    //    if (_heatingControlNeuralNetwork is null) return;

    //    await _heatingControlNeuralNetwork.Inizialize();
    //    await OnFinishedInizialize();
    //}
}