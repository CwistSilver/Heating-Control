using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Heating_Control_UI.Controls.Skia;
using Heating_Control_UI.Utilities;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Heating_Control_UI;
public class TemperaturProgressBar : SkiaControl
{
    private const float SvgScale = 0.9f;
    private const float OuterBlur = 6f;
    private const string OuterSvgPath = "M15 13.9997C16.2144 14.9119 17 16.3642 17 18C17 20.7614 14.7614 23 12 23C9.23858 23 7 20.7614 7 18C7 16.3642 7.78555 14.9119 9 13.9997V4C9 2.34315 10.3431 1 12 1C13.6569 1 15 2.34315 15 4V13.9997Z";
    private const string InnerSvgPath = "M13 4V15.1707C14.1652 15.5826 15 16.6938 15 18C15 19.6569 13.6569 21 12 21C10.3431 21 9 19.6569 9 18C9 16.6938 9.83481 15.5826 11 15.1707V4C11 3.44772 11.4477 3 12 3C12.5523 3 13 3.44772 13 4Z";
    private readonly static SKPath OuterPath = SKPath.ParseSvgPathData(OuterSvgPath);
    private readonly static SKPath InnerPath = SKPath.ParseSvgPathData(InnerSvgPath);
    private readonly static TimeSpan _glowTimeSpan = TimeSpan.FromMilliseconds(2_000);
    private readonly static TimeSpan _afterGlowTimeSpan = TimeSpan.FromMilliseconds(2_000);

    private readonly Stopwatch _glowStopwatch = new();
    private readonly Stopwatch _afterGlowStopwatch = new();
    private readonly Stopwatch _controlStart = new();

    private SKColor ThermometerColor
    {
        get => new(229, 97, 62, (byte)(_renderSaveOpacity * 255));
    }

    private readonly FastNoiseLite noise = new();
    private readonly TimeSpan duration = TimeSpan.FromSeconds(3);

    private float _xOffset;
    private double _renderSaveOpacity = 1.0;
    private SKColor _renderSaveForegroundColor;
    private SKColor _renderSaveBackgroundColor;
    private float _renderSavePercentages;
    private bool _isFinished;


    private static SKRect _outerBounds = SKRect.Empty;
    private static SKRect OuterBounds
    {
        get
        {
            if (_outerBounds != SKRect.Empty)
                return _outerBounds;

            OuterPath.GetTightBounds(out _outerBounds);
            return _outerBounds;
        }
    }




    public static readonly RoutedEvent<RoutedEventArgs> FinishedEvent = RoutedEvent.Register<TemperaturProgressBar, RoutedEventArgs>(nameof(Finished), RoutingStrategies.Direct);
    public event EventHandler<RoutedEventArgs> Finished
    {
        add { AddHandler(FinishedEvent, value); }
        remove { RemoveHandler(FinishedEvent, value); }
    }


    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(Foreground), Brushes.Black);
    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> BackgroundProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(Background), Brushes.Green);
    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }


    public static readonly StyledProperty<float> PercentagesProperty = AvaloniaProperty.Register<LineChartView, float>(nameof(Percentages), 0f);
    public float Percentages
    {
        get => GetValue(PercentagesProperty);
        set => SetValue(PercentagesProperty, value);
    }

    private readonly List<IDisposable> _disposables = new();

    public TemperaturProgressBar()
    {
        _disposables.Add(this.GetObservable(OpacityProperty).Subscribe(newValue => _renderSaveOpacity = newValue));
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        InvalidateSkia = TimeSpan.FromMilliseconds(10);
    }


    protected override void OnDispose()
    {
        _afterGlowStopwatch.Stop();
        _glowStopwatch.Stop();

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    protected virtual void OnFinish()
    {
        if (_isFinished)
            return;

        RaiseEvent(new RoutedEventArgs(FinishedEvent));
        _isFinished = true;
    }


    protected override void OnBeforRender()
    {
        var foregroundColor = ((ImmutableSolidColorBrush)Foreground).Color;
        _renderSaveForegroundColor = new SKColor(foregroundColor.R, foregroundColor.G, foregroundColor.B, (byte)(_renderSaveOpacity * 255));

        var backgroundColor = ((ImmutableSolidColorBrush)Background).Color;
        _renderSaveBackgroundColor = new SKColor(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A);

        Percentages = GetProgress(duration, _controlStart, autoRestart: false);
        if (Percentages == 1f)
            OnFinish();

        _renderSavePercentages = Percentages;
    }

    protected override void RenderSkia(SKCanvas canvas)
    {
        using var paintBackground = new SKPaint
        {
            Color = _renderSaveBackgroundColor,
            Style = SKPaintStyle.Fill,
        };
        canvas.DrawRect(canvas.LocalClipBounds, paintBackground);

        var glowPercent = GetProgress(_glowTimeSpan, _glowStopwatch, true);

        using var paintOutline = new SKPaint
        {
            Color = _renderSaveForegroundColor,
            StrokeWidth = 0.5f + (glowPercent * 0.1f),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            StrokeCap = SKStrokeCap.Round,
        };

        using var paintFillMargin = new SKPaint
        {
            Color = ThermometerColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };


        var afterGlow = 0f;
        var afterGlowStroke = 0f;
        if (_renderSavePercentages >= 0.95f)
        {
            var p = GetProgress(_afterGlowTimeSpan, _afterGlowStopwatch, autoRestart: false);
            afterGlow = p * 6f;
            afterGlowStroke = p * 1.5f;
        }

        if (_renderSavePercentages >= 0.10)
        {
            var strokeGrow = Math.Clamp((_renderSavePercentages / 0.50f), 0f, 1f);
            paintFillMargin.Style = SKPaintStyle.StrokeAndFill;
            paintFillMargin.StrokeWidth = afterGlowStroke + ((0.2f + (glowPercent * 0.15f)) * strokeGrow);
        }

        DrawLoadingRec(canvas, paintFillMargin, 1f - _renderSavePercentages);


        paintFillMargin.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Outer, afterGlow + 1f + (glowPercent * (1.5f * _renderSavePercentages)));
        DrawLoadingRec(canvas, paintFillMargin, 1f - _renderSavePercentages);


        paintOutline.Style = SKPaintStyle.Stroke;
        DrawOuterPath(canvas, paintOutline);

        paintOutline.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Outer, OuterBlur);
        DrawOuterPath(canvas, paintOutline);
    }


    private static void DrawOuterPath(SKCanvas canvas, SKPaint paint)
    {
        canvas.Save();
        canvas.Translate(canvas.LocalClipBounds.Width / 2, canvas.LocalClipBounds.Height / 2);
        canvas.Scale(SvgScale * canvas.LocalClipBounds.Height / OuterBounds.Height);
        canvas.Translate(-OuterBounds.MidX, -OuterBounds.MidY);

        canvas.DrawPath(OuterPath, paint);
        canvas.ResetMatrix();
        canvas.Restore();
    }

    private void DrawLoadingRec(SKCanvas canvas, SKPaint paint, float p)
    {
        canvas.Save();

        using var wavesPath = CreateInnerWavesPath(p);

        canvas.Translate(canvas.LocalClipBounds.Width / 2, canvas.LocalClipBounds.Height / 2);
        canvas.Scale(SvgScale * canvas.LocalClipBounds.Height / OuterBounds.Height);
        canvas.Translate(-OuterBounds.MidX, -OuterBounds.MidY);

        using var clippedPath = wavesPath.Op(InnerPath, SKPathOp.Intersect);
        canvas.DrawPath(clippedPath, paint);

        canvas.Restore();
    }


    private SKPath CreateInnerWavesPath(float p)
    {
        var height = OuterBounds.Height * p;
        var path = new SKPath();

        var points = new List<SKPoint>
            {
                new SKPoint(OuterBounds.Left, OuterBounds.Bottom),
                new SKPoint(OuterBounds.Left, OuterBounds.Top)
            };

        var rPoints = 100;
        var step = OuterBounds.Width / rPoints;
        var range = height * 0.02f;
        for (int i = 0; i < rPoints; i++)
        {
            var xValue = i * step + OuterBounds.Left;
            var yValue = GetNoise(40f, xValue) * range + height;
            points.Add(new SKPoint(xValue, yValue));
        }
        _xOffset += 1f;

        points.Add(new SKPoint(OuterBounds.Right, OuterBounds.Top));
        points.Add(new SKPoint(OuterBounds.Right, OuterBounds.Bottom));


        path.AddPoly(points.ToArray());

        path.Close();
        return path;
    }

    private float GetNoise(float frequency, float xValue) => noise.GetNoise(frequency * xValue + _xOffset, 0);
    private static float GetProgress(TimeSpan duration, Stopwatch stopwatch, bool upAndDown = false, bool autoRestart = true)
    {
        if (!stopwatch.IsRunning)
            stopwatch.Start();

        var percentage = Math.Clamp((float)(stopwatch.Elapsed.TotalSeconds / duration.TotalSeconds), 0f, 1f);
        if (upAndDown)
        {
            var value = percentage * 2f;
            if (value >= 2f)
            {
                if (autoRestart)
                    stopwatch.Restart();
                return 0f;
            }
            else if (value >= 1f)
            {
                return 1f - (value - 1f);

            }

            return value;
        }


        if (percentage >= 1f)
        {
            if (autoRestart)
                stopwatch.Restart();
            return 1f;
        }
        return percentage;
    }
}