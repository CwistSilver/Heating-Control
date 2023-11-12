using Heating_Control_UI.Utilities;
using Heating_Control_UI.ViewModels;

namespace Heating_Control_UI;

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