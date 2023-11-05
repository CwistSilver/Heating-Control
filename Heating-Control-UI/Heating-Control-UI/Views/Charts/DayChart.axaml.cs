using Avalonia.Controls;
using Heating_Control_UI.Utilities;
using Heating_Control_UI.ViewModels;

namespace Heating_Control_UI;

public partial class DayChart : PageControl
{
    public DayChart(DayChartModel dayChartModel)
    {
        InitializeComponent();
        DataContext = dayChartModel;
    }

    public DayChart()
    {
        InitializeComponent();
        DataContext = new DayChartModel();
    }
}