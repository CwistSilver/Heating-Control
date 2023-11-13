using Heating_Control_UI.ViewModels.Pages;

namespace Heating_Control_UI.Views.Pages;
public partial class DayChart : PageControl
{
    public DayChart(DayChartModel dayChartModel)
    {
        InitializeComponent();
        DataContext = dayChartModel;
        this.NavigatedTo += DayChart_NavigatedTo;
    }

    public DayChart()
    {
        InitializeComponent();
        DataContext = new DayChartModel();     
    }

    private void DayChart_NavigatedTo(object? sender, System.EventArgs e)
    {
        if (this.DataContext is DayChartModel viewModel)
            viewModel.NavigatedTo();
    }
}