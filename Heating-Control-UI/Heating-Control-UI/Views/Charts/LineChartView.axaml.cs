using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;

namespace Heating_Control_UI;

public partial class LineChartView : UserControl
{
    static LineChartView()
    {
        AffectsRender<LineChartView>(
            ForegroundProperty,
            ChartForegroundProperty,
            ChartBackgroundProperty,
            TemperatureStartProperty,
            TemperatureEndProperty,
            TemperatureSpacingProperty,
            MaxTemperatureProperty,
            TemperaturesProperty);
    }

    public static readonly StyledProperty<IBrush> ChartBackgroundProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(ChartBackground), Brushes.Black);
    public IBrush ChartBackground
    {
        get => GetValue(ChartBackgroundProperty);
        set => SetValue(ChartBackgroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> ChartForegroundProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(ChartForeground), Brushes.Black);
    public IBrush ChartForeground
    {
        get => GetValue(ChartForegroundProperty);
        set => SetValue(ChartForegroundProperty, value);
    }

    public static readonly StyledProperty<int> TemperatureStartProperty = AvaloniaProperty.Register<LineChartView, int>(nameof(TemperatureStart), 20);
    public int TemperatureStart
    {
        get => GetValue(TemperatureStartProperty);
        set => SetValue(TemperatureStartProperty, value);
    }

    public static readonly StyledProperty<int> TemperatureEndProperty = AvaloniaProperty.Register<LineChartView, int>(nameof(TemperatureEnd), -30);
    public int TemperatureEnd
    {
        get => GetValue(TemperatureEndProperty);
        set => SetValue(TemperatureEndProperty, value);
    }

    public static readonly StyledProperty<int> TemperatureSpacingProperty = AvaloniaProperty.Register<LineChartView, int>(nameof(TemperatureSpacing), 10);
    public int TemperatureSpacing
    {
        get => GetValue(TemperatureSpacingProperty);
        set => SetValue(TemperatureSpacingProperty, value);
    }

    public static readonly StyledProperty<int> MaxTemperatureProperty = AvaloniaProperty.Register<LineChartView, int>(nameof(MaxTemperature), 90);
    public int MaxTemperature
    {
        get => GetValue(MaxTemperatureProperty);
        set => SetValue(MaxTemperatureProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<int>> TemperaturesProperty =
        AvaloniaProperty.Register<LineChartView, ObservableCollection<int>>(nameof(Temperatures),
        new ObservableCollection<int>() { 20, 35, 47, 57, 68, 80 });
    public ObservableCollection<int> Temperatures
    {
        get => GetValue(TemperaturesProperty);
        set => SetValue(TemperaturesProperty, value);
    }


    public LineChartView()
    {
        InitializeComponent();
        this.PointerPressed += LineChartView_PointerPressed;
        this.PointerReleased += LineChartView_PointerReleased;
        this.PointerMoved += LineChartView_PointerMoved;
        Temperatures.CollectionChanged += Temperatures_CollectionChanged;
        TemperaturesProperty.Changed.AddClassHandler<LineChartView>((element, e) =>
        {
            Temperatures.CollectionChanged += Temperatures_CollectionChanged;
        });

    }

    private void Temperatures_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void LineChartView_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {


        if (_selectedIndex == -1)
            return;

        var point = e.GetCurrentPoint(sender as Control);

        var x = point.Position.X;
        var y = point.Position.Y;

        Temperatures[_selectedIndex] = PointToCelsius(new Point(x, y));
        //Temperatures.Insert(_selectedIndex, newTemp);
    }

    private int PointToCelsius(Point point)
    {
        var h = Bounds.Height;
        var w = Bounds.Width;

        var yStart = YTextBox.DesiredSize.Height;
        var yEnd = h - MarginBottom;

        // Überprüfen, ob der Punkt außerhalb des Diagrammbereichs liegt
        if (point.Y <= yStart)
            return MaxTemperature;

        if (point.Y >= yEnd)
            return 0;

        // Den Anteil des Punktes zwischen yStart und yEnd berechnen
        var proportion = (point.Y - yStart) / (yEnd - yStart);

        // Diesen Anteil verwenden, um den entsprechenden Temperaturwert zu berechnen
        var celsius = (1 - proportion) * MaxTemperature;

        return (int)celsius;
    }



    int _selectedIndex = -1;
    private void LineChartView_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        _selectedIndex = -1;
    }

    private void LineChartView_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        var x = point.Position.X;
        var y = point.Position.Y;

        _selectedIndex = IsHit(new Point(x, y));
    }

    public override void Render(DrawingContext context)
    {
        if(Temperatures.Count != 6) return;
        context.DrawRectangle(ChartBackground, null, Bounds);
        DrawXLines(context);
        DrawLinesBetweenPoints(context);
        DrawPoints(context);

        base.Render(context);
    }

    public int MarginLines { get; set; } = 40;
    public int MarginBottom { get; set; } = 50;
    public IBrush GridColor { get; set; } = new SolidColorBrush(Color.FromArgb(255, 148, 151, 156));
    private void DrawXLines(DrawingContext context)
    {
        var h = Bounds.Height;
        var w = Bounds.Width - MarginLines * 2;

        var min = Math.Min(TemperatureStart, TemperatureEnd);
        var max = Math.Max(TemperatureStart, TemperatureEnd);
        var lines = Math.Abs(min - max) / TemperatureSpacing;

        var lineSpacingW = w / lines;

        var pen = new Pen(GridColor);
        pen.LineCap = PenLineCap.Round;
        pen.Thickness = 3;

        for (int i = 0; i <= lines; i++)
        {
            var x = MarginLines + i * lineSpacingW;
            context.DrawLine(pen, new Point(x, 0 + YTextBox.DesiredSize.Height), new Point(x, h - MarginBottom));

            var typeface = new Typeface(this.FontFamily.Name, FontStyle.Normal, FontWeight.Bold);
            var de = new CultureInfo("de-DE");

            var temp = max - i * TemperatureSpacing;
            var text = string.Empty;
            var textP = string.Empty;
            if (temp > 0)
            {
                textP = text = "+";
            }


            text += $"{temp}°C";
            textP += $"{temp}";

            var formattedText = new FormattedText(text, de, FlowDirection.LeftToRight, typeface, this.FontSize, GridColor);
            formattedText.TextAlignment = TextAlignment.Left;

            var tempNumberText = temp.ToString().Replace("-", null);
            var formattedTempNumberText = new FormattedText(tempNumberText, de, FlowDirection.LeftToRight, typeface, this.FontSize, GridColor);
            var tempNumberTextGeometry = formattedTempNumberText.BuildGeometry(new Point(0, 0));


            var formattedTempNumberText2 = new FormattedText(textP, de, FlowDirection.LeftToRight, typeface, this.FontSize, GridColor);
            var tempNumberTextGeometry2 = formattedTempNumberText2.BuildGeometry(new Point(0, 0));




            var preWidth = tempNumberTextGeometry2.Bounds.Width - tempNumberTextGeometry.Bounds.Width;
            var numerWidth = tempNumberTextGeometry.Bounds.Width;
            var xOffset = (numerWidth / 2f) + preWidth;
            context.DrawText(formattedText, new Point(x - xOffset, h - tempNumberTextGeometry.Bounds.Height - MarginBottom / 2f));

        }
    }

    public int PointRadius { get; set; } = 18;

    private int IsHit(Point point)
    {
        var currentVector = new Vector2((float)point.X, (float)point.Y);
        var points = GetPoints();
        for (int i = 0; i < points.Length; i++)
        {
            var vector = new Vector2((float)points[i].X, (float)points[i].Y);
            var distance = Vector2.Distance(currentVector, vector);
            if (distance <= PointRadius)
                return i;
        }

        return -1;
    }

    private Point[] GetPoints()
    {
        var boxes = new Point[Temperatures.Count];

        var h = Bounds.Height;
        var w = Bounds.Width - MarginLines * 2;

        var min = Math.Min(TemperatureStart, TemperatureEnd);
        var max = Math.Max(TemperatureStart, TemperatureEnd);
        var lines = Math.Abs(min - max) / TemperatureSpacing;

        var lineSpacingW = w / lines;


        for (int i = 0; i <= lines; i++)
        {
            var tempInP = Temperatures[i] / (float)MaxTemperature;
            var totalMargin = YTextBox.DesiredSize.Height + MarginBottom;
            var difference = h - totalMargin;

            var y = h - difference * tempInP - MarginBottom;
            var x = MarginLines + i * lineSpacingW;

            boxes[i] = new Point(x, y);
        }

        return boxes;
    }


    private void DrawLinesBetweenPoints(DrawingContext context)
    {
        var points = GetPoints();
        var pen = new Pen(ChartForeground);
        pen.Thickness = 5;

        for (int i = 1; i < points.Length; i++)
        {
            context.DrawLine(pen, points[i], points[i - 1]);
            DrawGradiand(context, points[i], points[i - 1]);
        }
    }

    private Color _lineRed = new Color(255, 255, 105, 105);
    private Color _gradientRed = new Color(100, 255, 105, 105);
    //private Color heatRed = new Color(100, 255, 105, 105);
    private void DrawGradiand(DrawingContext context, Point a, Point b)
    {
        var pen = new Pen(ChartForeground);
        pen.Thickness = 5;

        // Gradient unter der Linie zeichnen
        var gradientBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Absolute),
            EndPoint = new RelativePoint(0, Bounds.Height, RelativeUnit.Absolute)
        };
        //double relativeHeight = a.Y / Bounds.Height;rgb(255, 105, 105)
        gradientBrush.GradientStops.Add(new GradientStop(_gradientRed, 0));
        gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

        var polygonPoints = new List<Point>
    {
        b,
        a,
        new Point(a.X, Bounds.Height),
        new Point(b.X, Bounds.Height)
    };

        PolylineGeometry polylineGeometry = new PolylineGeometry(polygonPoints, true);
        // Zeichnen Sie die erstellte Geometrie
        context.DrawGeometry(gradientBrush, null, polylineGeometry);
    }

    private void DrawPoints(DrawingContext context)
    {
        var points = GetPoints();
        var pen = new Pen(ChartForeground);
        pen.LineCap = PenLineCap.Round;
        pen.Thickness = 3;

        for (int i = 0; i < points.Length; i++)
        {
            context.DrawEllipse(this.ChartBackground, pen, new Point(points[i].X, points[i].Y), PointRadius, PointRadius);

            var typeface = new Typeface(this.FontFamily.Name, FontStyle.Normal, FontWeight.ExtraBold);
            var de = new CultureInfo("de-DE");

            var temp = Temperatures[i];
            var text = string.Empty;

            text += $"{temp}°".Replace("-", null); ;

            var formattedText = new FormattedText(text, de, FlowDirection.LeftToRight, typeface, this.FontSize, GridColor);
            var tempNumberTextGeometry = formattedText.BuildGeometry(new Point(0, 0));

            var xOffset = tempNumberTextGeometry.Bounds.Width / 2f;
            context.DrawText(formattedText, new Point(points[i].X - xOffset, points[i].Y - tempNumberTextGeometry.Bounds.Height));
        }
    }
}