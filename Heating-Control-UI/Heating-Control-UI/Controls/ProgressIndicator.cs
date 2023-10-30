using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace Heating_Control_UI;

public class ProgressIndicator : Control
{

    private TimeSpan _intervall = TimeSpan.FromSeconds(1);
    private DateTime _start = DateTime.Now;

    public ProgressIndicator()
    {
        //InitializeComponent();
        _brush = new SolidColorBrush(new Color(100, 255, 0, 0));

        Dispatcher.UIThread.Post(() => LongRunningTask(), DispatcherPriority.Background);
    }

    private async Task LongRunningTask()
    {
        while (true)
        {
            this.InvalidateVisual();
            await Task.Delay(10);
        }

    }

    private IBrush _brush;

    public override void Render(DrawingContext context)
    {
        var time = DateTime.Now - _start;
        var p = time.TotalMilliseconds / (double)_intervall.TotalMilliseconds;

        if (p >= 2f)
        {
            _start = DateTime.Now;

        }
        else if (p >= 1f)
        {
            p = 1f + (1f - p);
        }

        p = Math.Clamp(p, 0f, 1f);
        var center = new Avalonia.Point(this.Bounds.Width / 2f, this.Bounds.Height / 2f);
        var maxRadius = Math.Min(this.Bounds.Width, this.Bounds.Height) / 2f;
        var currentRadius = maxRadius * p;
        var currentRadius2 = maxRadius * (1f - p);

        context.DrawEllipse(_brush, null, center, currentRadius, currentRadius);
        context.DrawEllipse(_brush, null, center, currentRadius2, currentRadius2);

        base.Render(context);
    }
}