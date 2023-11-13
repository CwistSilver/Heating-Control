using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Heating_Control_UI.Utilities;
using System;
using System.Threading.Tasks;

namespace Heating_Control_UI;

public class ProgressIndicator : Control
{
    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty.Register<ProgressIndicator, IBrush>(nameof(Foreground), Brushes.Black);
    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    private IBrush _drawingBrush;

    private readonly TimeSpan _intervall = TimeSpan.FromSeconds(1);
    private DateTime _start = DateTime.Now;

    public ProgressIndicator()
    {
        _drawingBrush = Foreground;
        this.GetObservable(ForegroundProperty).Subscribe(newBrush => _drawingBrush = new SolidColorBrush(newBrush.ToColor(127)));
        Dispatcher.UIThread.Post(() => _ = LongRunningTask(), DispatcherPriority.Background);
    }

    private async Task LongRunningTask()
    {
        while (true)
        {
            InvalidateVisual();
            await Task.Delay(10);
        }

    }

    public override void Render(DrawingContext context)
    {
        var duration = DateTime.Now - _start;
        var anaimationPercentage = duration.TotalMilliseconds / (double)_intervall.TotalMilliseconds;

        if (anaimationPercentage >= 2f)        
            _start = DateTime.Now;        

        else if (anaimationPercentage >= 1f)        
            anaimationPercentage = 1f + (1f - anaimationPercentage);        

        anaimationPercentage = Math.Clamp(anaimationPercentage, 0f, 1f);
        var center = new Point(this.Bounds.Width / 2f, this.Bounds.Height / 2f);
        var maxCircleRadius = Math.Min(this.Bounds.Width, this.Bounds.Height) / 2f;
        var circleRadius1 = maxCircleRadius * anaimationPercentage;
        var circleRadius2 = maxCircleRadius * (1f - anaimationPercentage);

        context.DrawEllipse(_drawingBrush, null, center, circleRadius1, circleRadius1);
        context.DrawEllipse(_drawingBrush, null, center, circleRadius2, circleRadius2);

        base.Render(context);
    }
}