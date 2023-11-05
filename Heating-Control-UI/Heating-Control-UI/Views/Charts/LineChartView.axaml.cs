using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            XStartValueProperty,
            XStartValueProperty,
            XSpacingProperty,
            MaxYProperty,
            ShowSignProperty,
            YPostfixProperty,
            ValuePostfixProperty,
            SecondValuesProperty,
            YTitleProperty,
            XTitleProperty,
            ValuesProperty);
    }

    public static readonly StyledProperty<float> ValueRadiusProperty = AvaloniaProperty.Register<LineChartView, float>(nameof(ValueRadius), 18);
    public float ValueRadius
    {
        get => GetValue(ValueRadiusProperty);
        set => SetValue(ValueRadiusProperty, value);
    }

    public static readonly StyledProperty<bool> ShowSignProperty = AvaloniaProperty.Register<LineChartView, bool>(nameof(ShowSign), false);
    public bool ShowSign
    {
        get => GetValue(ShowSignProperty);
        set => SetValue(ShowSignProperty, value);
    }

    public static readonly StyledProperty<string> YPostfixProperty = AvaloniaProperty.Register<LineChartView, string>(nameof(YPostfix), string.Empty);
    public string YPostfix
    {
        get => GetValue(YPostfixProperty);
        set => SetValue(YPostfixProperty, value);
    }

    public static readonly StyledProperty<string> ValuePostfixProperty = AvaloniaProperty.Register<LineChartView, string>(nameof(ValuePostfix), string.Empty);
    public string ValuePostfix
    {
        get => GetValue(ValuePostfixProperty);
        set => SetValue(ValuePostfixProperty, value);
    }

    public static readonly StyledProperty<string> YTitleProperty = AvaloniaProperty.Register<LineChartView, string>(nameof(YTitle), string.Empty);
    public string YTitle
    {
        get => GetValue(YTitleProperty);
        set => SetValue(YTitleProperty, value);
    }


    public static readonly StyledProperty<string> XTitleProperty = AvaloniaProperty.Register<LineChartView, string>(nameof(XTitle), string.Empty);
    public string XTitle
    {
        get => GetValue(XTitleProperty);
        set => SetValue(XTitleProperty, value);
    }

    public static readonly StyledProperty<IBrush> ChartBackgroundProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(ChartBackground), Brushes.Black);
    public IBrush ChartBackground
    {
        get => GetValue(ChartBackgroundProperty);
        set => SetValue(ChartBackgroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> SecondChartBackgroundProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(SecondChartBackground), Brushes.Green);
    public IBrush SecondChartBackground
    {
        get => GetValue(SecondChartBackgroundProperty);
        set => SetValue(SecondChartBackgroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> ChartForegroundProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(ChartForeground), Brushes.Black);
    public IBrush ChartForeground
    {
        get => GetValue(ChartForegroundProperty);
        set => SetValue(ChartForegroundProperty, value);
    }

    public static readonly StyledProperty<float> XStartValueProperty = AvaloniaProperty.Register<LineChartView, float>(nameof(XStartValue), 20);
    public float XStartValue
    {
        get => GetValue(XStartValueProperty);
        set => SetValue(XStartValueProperty, value);
    }

    public static readonly StyledProperty<float> XEndValueProperty = AvaloniaProperty.Register<LineChartView, float>(nameof(XEndValue), -30);
    public float XEndValue
    {
        get => GetValue(XEndValueProperty);
        set => SetValue(XEndValueProperty, value);
    }

    public static readonly StyledProperty<float> XSpacingProperty = AvaloniaProperty.Register<LineChartView, float>(nameof(XSpacing), 10f);
    public float XSpacing
    {
        get => GetValue(XSpacingProperty);
        set => SetValue(XSpacingProperty, value);
    }

    public static readonly StyledProperty<float> MaxYProperty = AvaloniaProperty.Register<LineChartView, float>(nameof(MaxY), 90f);
    public float MaxY
    {
        get => GetValue(MaxYProperty);
        set => SetValue(MaxYProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<float>> ValuesProperty = AvaloniaProperty.Register<LineChartView, ObservableCollection<float>>(nameof(Values), new ObservableCollection<float>() { 20, 35, 47, 57, 68, 80 });
    public ObservableCollection<float> Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<float>> SecondValuesProperty = AvaloniaProperty.Register<LineChartView, ObservableCollection<float>>(nameof(SecondValues), new ObservableCollection<float>());
    public ObservableCollection<float> SecondValues
    {
        get => GetValue(SecondValuesProperty);
        set => SetValue(SecondValuesProperty, value);
    }


    public LineChartView()
    {
        InitializeComponent();
        this.GetObservable(YTitleProperty).Subscribe(newTitle => YTextBox.Text = newTitle);
        this.GetObservable(XTitleProperty).Subscribe(newTitle => XTextBox.Text = newTitle);
        this.GetObservable(ValuesProperty).Subscribe(newCollection =>
        {
            Values.CollectionChanged -= Temperatures_CollectionChanged;
            newCollection.CollectionChanged += Temperatures_CollectionChanged;
            InvalidateVisual();
        });

        this.GetObservable(SecondValuesProperty).Subscribe(newCollection =>
        {
            SecondValues.CollectionChanged -= Temperatures_CollectionChanged;
            newCollection.CollectionChanged += Temperatures_CollectionChanged;
            InvalidateVisual();
        });

        this.PointerPressed += LineChartView_PointerPressed;
        this.PointerReleased += LineChartView_PointerReleased;
        this.PointerMoved += LineChartView_PointerMoved;
        Values.CollectionChanged += Temperatures_CollectionChanged;
        SecondValues.CollectionChanged += Temperatures_CollectionChanged;
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
        if (focuedLine == 0)
        {
            Values[_selectedIndex] = PointToCelsius(new Point(x, y));
        }
        else if (focuedLine == 1)
        {
            SecondValues[_selectedIndex] = PointToCelsius(new Point(x, y));
        }

        //Temperatures.Insert(_selectedIndex, newTemp);
    }

    private float PointToCelsius(Point point)
    {
        var h = Bounds.Height;
        var w = Bounds.Width;

        var yStart = YTextBox.DesiredSize.Height;
        var yEnd = h - MarginBottom;

        // Überprüfen, ob der Punkt außerhalb des Diagrammbereichs liegt
        if (point.Y <= yStart)
            return MaxY;

        if (point.Y >= yEnd)
            return 0;

        // Den Anteil des Punktes zwischen yStart und yEnd berechnen
        var proportion = (point.Y - yStart) / (yEnd - yStart);

        // Diesen Anteil verwenden, um den entsprechenden Temperaturwert zu berechnen
        var celsius = (1 - proportion) * MaxY;

        return (int)celsius;
    }



    int _selectedIndex = -1;
    private void LineChartView_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        _selectedIndex = -1;
    }


    private int focuedLine = 0;
    private void LineChartView_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        var x = point.Position.X;
        var y = point.Position.Y;

        var isHit = IsHit(new Point(x, y));
        if (isHit != -1)
        {
            _selectedIndex = isHit;
            focuedLine = 0;
            InvalidateVisual();
            return;
        }

        var isSecondHit = _selectedIndex = IsSecondHit(new Point(x, y));
        if (isSecondHit != -1)
        {
            focuedLine = 1;
            InvalidateVisual();
        }
    }

    private int TotalXLines()
    {
        var min = Math.Min(XStartValue, XEndValue);
        var max = Math.Max(XStartValue, XEndValue);
        return (int)(Math.Abs(min - max) / XSpacing) + 1;
    }

    public override void Render(DrawingContext context)
    {
        var totalLines = TotalXLines();
        if (Values.Count != totalLines) return;
        //context.DrawRectangle(ChartBackground, null, Bounds);
        DrawXLines(context);
        DrawLinesBetweenPoints(context);


        //DrawSecondXLines(context);



        if (SecondValues.Count == 0)
            DrawPoints(context);
        else
        {
            if (focuedLine == 0)
            {
                DrawSecondPoints(context);
                DrawPoints(context);
            }
            else if (focuedLine == 1)
            {
                DrawPoints(context);
                DrawSecondPoints(context);
            }
        }



        base.Render(context);
    }

    public int MarginLines { get; set; } = 40;
    public int MarginBottom { get; set; } = 50;
    public IBrush GridColor { get; set; } = new SolidColorBrush(Color.FromArgb(255, 148, 151, 156));
    private void DrawXLines(DrawingContext context)
    {
        var h = Bounds.Height;
        var w = Bounds.Width - MarginLines * 2;

        var lowToHeigh = true;

        var min = Math.Min(XStartValue, XEndValue);
        var max = Math.Max(XStartValue, XEndValue);
        if (min != XStartValue)
            lowToHeigh = false;

        var lines = Math.Abs(min - max) / XSpacing;

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

            float temp;
            if (lowToHeigh)
                temp = min + i * XSpacing;
            else
                temp = max - i * XSpacing;

            var text = string.Empty;
            var textP = string.Empty;

            if (ShowSign)
            {
                if (temp > 0)
                {
                    textP = text = "+";
                }
            }

            var tempStringValue = temp.ToString();
            if (!ShowSign)
            {
                if (temp < 0)
                {
                    tempStringValue = temp.ToString().Replace("-", null);
                }
            }


            text += $"{tempStringValue}{YPostfix}";
            textP += $"{tempStringValue}";



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

    private void DrawSecondXLines(DrawingContext context)
    {
        var points = GetSecondPoints();
        var pen = new Pen(new SolidColorBrush(Colors.Green));
        pen.Thickness = 6;

        for (int i = 1; i < points.Length; i++)
        {
            context.DrawLine(pen, points[i], points[i - 1]);
        }
    }

    private int IsHit(Point point)
    {
        var currentVector = new Vector2((float)point.X, (float)point.Y);
        var points = GetPoints();
        for (int i = 0; i < points.Length; i++)
        {
            var vector = new Vector2((float)points[i].X, (float)points[i].Y);
            var distance = Vector2.Distance(currentVector, vector);
            if (distance <= ValueRadius)
                return i;
        }

        return -1;
    }


    private int IsSecondHit(Point point)
    {
        var currentVector = new Vector2((float)point.X, (float)point.Y);
        var points = GetSecondPoints();
        if (points is null) return -1;

        for (int i = 0; i < points.Length; i++)
        {
            var vector = new Vector2((float)points[i].X, (float)points[i].Y);
            var distance = Vector2.Distance(currentVector, vector);
            if (distance <= ValueRadius)
                return i;
        }

        return -1;
    }

    private Point[] GetPoints()
    {
        var boxes = new Point[Values.Count];

        var h = Bounds.Height;
        var w = Bounds.Width - MarginLines * 2;

        var min = Math.Min(XStartValue, XEndValue);
        var max = Math.Max(XStartValue, XEndValue);
        var lines = Math.Abs(min - max) / XSpacing;

        var lineSpacingW = w / lines;


        for (int i = 0; i <= lines; i++)
        {
            if (i >= Values.Count) break;

            var tempInP = Values[i] / (float)MaxY;
            var totalMargin = YTextBox.DesiredSize.Height + MarginBottom;
            var difference = h - totalMargin;

            var y = h - difference * tempInP - MarginBottom;
            var x = MarginLines + i * lineSpacingW;

            boxes[i] = new Point(x, y);
        }

        return boxes;
    }


    private Point[]? GetSecondPoints()
    {
        if (SecondValues.Count == 0) return null;
        var boxes = new Point[SecondValues.Count];

        var h = Bounds.Height;
        var w = Bounds.Width - MarginLines * 2;

        var min = Math.Min(XStartValue, XEndValue);
        var max = Math.Max(XStartValue, XEndValue);
        var lines = Math.Abs(min - max) / XSpacing;

        var lineSpacingW = w / lines;


        for (int i = 0; i <= lines; i++)
        {
            if (i >= SecondValues.Count) break;

            var tempInP = SecondValues[i] / (float)MaxY;
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
        var secondPoints = GetSecondPoints();
        var pen = new Pen(ChartForeground);
        pen.Thickness = 4;

        var secondPen = new Pen(SecondChartBackground);
        secondPen.Thickness = 4;

        for (int i = 1; i < points.Length; i++)
        {
            //DrawGradiand(context, points[i], points[i - 1]);


            if (focuedLine == 0)
            {
                DrawGradiand(context, points[i], points[i - 1]);

                if (secondPoints is not null)
                    context.DrawLine(secondPen, secondPoints[i], secondPoints[i - 1]);

                context.DrawLine(pen, points[i], points[i - 1]);
            }
            else if (focuedLine == 1)
            {
                DrawSecondGradiand(context, secondPoints![i], secondPoints[i - 1]);

                context.DrawLine(pen, points[i], points[i - 1]);
                context.DrawLine(secondPen, secondPoints![i], secondPoints[i - 1]);
            }



        }
    }


    private Color _gradientRed = new(100, 255, 105, 105);
    private void DrawGradiand(DrawingContext context, Point a, Point b)
    {
        var gradientBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Absolute),
            EndPoint = new RelativePoint(0, Bounds.Height, RelativeUnit.Absolute)
        };

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

        context.DrawGeometry(gradientBrush, null, polylineGeometry);
    }


    private void DrawSecondGradiand(DrawingContext context, Point a, Point b)
    {
        var gradientBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Absolute),
            EndPoint = new RelativePoint(0, Bounds.Height, RelativeUnit.Absolute)
        };

        gradientBrush.GradientStops.Add(new GradientStop(Colors.Green, 0));
        gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

        var polygonPoints = new List<Point>
    {
        b,
        a,
        new Point(a.X, Bounds.Height),
        new Point(b.X, Bounds.Height)
    };

        PolylineGeometry polylineGeometry = new PolylineGeometry(polygonPoints, true);

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
            context.DrawEllipse(this.ChartBackground, pen, new Point(points[i].X, points[i].Y), ValueRadius, ValueRadius);

            var typeface = new Typeface(this.FontFamily.Name, FontStyle.Normal, FontWeight.ExtraBold);
            var de = new CultureInfo("de-DE");

            var temp = Values[i];
            var text = string.Empty;

            text += $"{temp}{ValuePostfix}".Replace("-", null); ;

            var formattedText = new FormattedText(text, de, FlowDirection.LeftToRight, typeface, this.FontSize, GridColor);
            var tempNumberTextGeometry = formattedText.BuildGeometry(new Point(0, 0));

            var xOffset = tempNumberTextGeometry.Bounds.Width / 2f;
            context.DrawText(formattedText, new Point(points[i].X - xOffset, points[i].Y - tempNumberTextGeometry.Bounds.Height));
        }
    }


    private void DrawSecondPoints(DrawingContext context)
    {
        var points = GetSecondPoints();
        var pen = new Pen(SecondChartBackground);
        pen.LineCap = PenLineCap.Round;
        pen.Thickness = 3;

        for (int i = 0; i < points.Length; i++)
        {
            context.DrawEllipse(this.ChartBackground, pen, new Point(points[i].X, points[i].Y), ValueRadius, ValueRadius);

            var typeface = new Typeface(this.FontFamily.Name, FontStyle.Normal, FontWeight.ExtraBold);
            var de = new CultureInfo("de-DE");

            var temp = SecondValues[i];
            var text = string.Empty;

            text += $"{temp}{ValuePostfix}".Replace("-", null); ;

            var formattedText = new FormattedText(text, de, FlowDirection.LeftToRight, typeface, this.FontSize, GridColor);
            var tempNumberTextGeometry = formattedText.BuildGeometry(new Point(0, 0));

            var xOffset = tempNumberTextGeometry.Bounds.Width / 2f;
            context.DrawText(formattedText, new Point(points[i].X - xOffset, points[i].Y - tempNumberTextGeometry.Bounds.Height));
        }
    }
}