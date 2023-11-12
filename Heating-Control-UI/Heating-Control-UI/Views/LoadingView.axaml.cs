using Heating_Control_UI.Utilities;
using Heating_Control_UI.ViewModels;

namespace Heating_Control_UI;

public partial class LoadingView : PageControl
{
    public LoadingView()
    {
        InitializeComponent();
    }

    public LoadingView(LoadingViewModel loadingViewModel)
    {
        InitializeComponent();
        DataContext = loadingViewModel;
    }


    private async void LoadingView_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (DataContext is LoadingViewModel loadingViewModel)
            await loadingViewModel.SkipRequest();
    }

    private async void TemperaturProgressBarControl_Finished(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is LoadingViewModel loadingViewModel)
            await loadingViewModel.OnFinishedAnimation();
    }

}