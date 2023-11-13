using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Heating_Control_UI.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;

namespace Heating_Control_UI;
public partial class LineChartView : UserControl
{
    private const int _xMargin = 40;
    public const int _bottomMargin = 80;

    private static readonly CultureInfo _de = new("de-DE");
    private static readonly Color _gradientRed = new(100, 255, 105, 105);

    private int _selectedXIndex = -1;
    private int _selectedGraph = 0;
    private double _chartWidth = 0;
    private double _chartXSpacing = 0;
    private Typeface _currentFaceType = new();

    private Pen? _gridPen = null;
    public Pen GridPen
    {
        get
        {
            if (_gridPen is not null)
                return _gridPen;

            return _gridPen = new Pen() { Thickness = 3, LineCap = PenLineCap.Round, Brush = GridBrush };
        }
    }

    public LinearGradientBrush SecondaryGradiand
    {
        get
        {
            var secondGradiand = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(0, Bounds.Height - _bottomMargin, RelativeUnit.Absolute)
            };

            secondGradiand.GradientStops.Add(new GradientStop(SecondaryBrush.ToColor(200), 0));
            secondGradiand.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

            return secondGradiand;
        }
    }

    public LinearGradientBrush PrimaryGradiand
    {
        get
        {
            var primaryGradiand = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(0, Bounds.Height - _bottomMargin, RelativeUnit.Absolute)
            };

            primaryGradiand.GradientStops.Add(new GradientStop(_gradientRed, 0));
            primaryGradiand.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

            return primaryGradiand;
        }
    }

    private Pen? _primaryPen = null;
    public Pen PrimaryPen
    {
        get
        {
            if (_primaryPen is not null) return _primaryPen;

            return _primaryPen = new Pen(ChartForeground) { LineCap = PenLineCap.Round, Thickness = 3 };
        }
    }

    private Pen? _secondaryPen = null;
    public Pen SecondaryPen
    {
        get
        {
            if (_secondaryPen is not null) return _secondaryPen;

            return _secondaryPen = new Pen(SecondaryBrush) { LineCap = PenLineCap.Round, Thickness = 3 };
        }
    }

    #region Control Propertys
    static LineChartView()
    {
        AffectsRender<LineChartView>(
            ForegroundProperty,
            ChartForegroundProperty,
            ChartBackgroundProperty,
            MaxYProperty,
            ShowSignProperty,
            YPostfixProperty,
            ValuePostfixProperty,
            SecondaryValuesProperty,
            GridBrushProperty,
            YTitleProperty,
            XTitleProperty,
            ValuesProperty,
            XValuesProperty);
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

    public static readonly StyledProperty<bool> IsEditableProperty = AvaloniaProperty.Register<LineChartView, bool>(nameof(IsEditable), false);
    public bool IsEditable
    {
        get => GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
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

    public static readonly StyledProperty<string> SecondYTitleProperty = AvaloniaProperty.Register<LineChartView, string>(nameof(SecondYTitle), string.Empty);
    public string SecondYTitle
    {
        get => GetValue(SecondYTitleProperty);
        set => SetValue(SecondYTitleProperty, value);
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

    public static readonly StyledProperty<IBrush> SecondaryBrushProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(SecondaryBrush), Brushes.Green);
    public IBrush SecondaryBrush
    {
        get => GetValue(SecondaryBrushProperty);
        set => SetValue(SecondaryBrushProperty, value);
    }

    public static readonly StyledProperty<IBrush> ChartForegroundProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(ChartForeground), Brushes.Black);
    public IBrush ChartForeground
    {
        get => GetValue(ChartForegroundProperty);
        set => SetValue(ChartForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> GridBrushProperty = AvaloniaProperty.Register<LineChartView, IBrush>(nameof(GridBrush), new SolidColorBrush(Color.FromArgb(255, 148, 151, 156)));
    public IBrush GridBrush
    {
        get => GetValue(GridBrushProperty);
        set => SetValue(GridBrushProperty, value);
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

    public static readonly StyledProperty<ObservableCollection<float>> XValuesProperty = AvaloniaProperty.Register<LineChartView, ObservableCollection<float>>(nameof(XValues), new ObservableCollection<float>() { 20, 10, 0, -10, -20, -30 });
    public ObservableCollection<float> XValues
    {
        get => GetValue(XValuesProperty);
        set => SetValue(XValuesProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<float>> SecondaryValuesProperty = AvaloniaProperty.Register<LineChartView, ObservableCollection<float>>(nameof(SecondaryValues), new ObservableCollection<float>());
    public ObservableCollection<float> SecondaryValues
    {
        get => GetValue(SecondaryValuesProperty);
        set => SetValue(SecondaryValuesProperty, value);
    }
    #endregion

    public LineChartView()
    {             
        InitializeComponent();

        this.GetObservable(GridBrushProperty).Subscribe(newBrush =>
        {
            XAxisTextBox.Foreground = newBrush;
            _gridPen = null;
        });

        this.GetObservable(SecondaryBrushProperty).Subscribe(newBrush =>
        {
            SecondaryYAxisTextBox.Foreground = newBrush;
            _secondaryPen = null;
        });

        this.GetObservable(ChartForegroundProperty).Subscribe(newBrush =>
        {
            _primaryPen = null;
        });

        this.GetObservable(XTitleProperty).Subscribe(newTitle => XAxisTextBox.Text = newTitle);
        this.GetObservable(YTitleProperty).Subscribe(newTitle => YAxisTextBox.Text = newTitle);
        this.GetObservable(SecondYTitleProperty).Subscribe(newTitle =>
        {
            if (string.IsNullOrEmpty(newTitle))
            {
                Grid.SetColumn(YAxisTextBoxViewbox, 0);
                Grid.SetColumnSpan(YAxisTextBoxViewbox, 2);
            }
            else
            {
                Grid.SetColumn(SecondaryYAxisTextBoxViewbox, 0);
                Grid.SetColumn(YAxisTextBoxViewbox, 1);
                Grid.SetColumnSpan(YAxisTextBoxViewbox, 1);
            }              

            SecondaryYAxisTextBox.Text = newTitle;
        });

        this.GetObservable(XValuesProperty).Subscribe(newCollection =>
        {
            XValues.CollectionChanged -= RedrawOnCollectionChange;
            newCollection.CollectionChanged += RedrawOnCollectionChange;
            InvalidateVisual();
        });

        this.GetObservable(ValuesProperty).Subscribe(newCollection =>
        {
            Values.CollectionChanged -= RedrawOnCollectionChange;
            newCollection.CollectionChanged += RedrawOnCollectionChange;
            InvalidateVisual();
        });
             

        this.GetObservable(SecondaryValuesProperty).Subscribe(newCollection =>
        {
            SecondaryValues.CollectionChanged -= RedrawOnCollectionChange;
            newCollection.CollectionChanged += RedrawOnCollectionChange;
            InvalidateVisual();
        });

        PointerPressed += LineChartView_PointerPressed;
        PointerReleased += LineChartView_PointerReleased;
        PointerMoved += LineChartView_PointerMoved;
        Values.CollectionChanged += RedrawOnCollectionChange;
        XValues.CollectionChanged += RedrawOnCollectionChange;
        SecondaryValues.CollectionChanged += RedrawOnCollectionChange;        
    }

    #region Render Logic
    public override void Render(DrawingContext context)
    {
        if (!IsLoaded) return;

        var totalLines = XValues.Count;
        if (Values.Count != totalLines) return;

        CalculateChartBounds();

        DrawXLines(context);
        DrawLinesBetweenPoints(context);

        if (SecondaryValues.Count == 0)
            DrawPoints(context, Values, PrimaryPen);
        else
        {
            if (_selectedGraph == 0)
            {
                DrawPoints(context, SecondaryValues, SecondaryPen);
                DrawPoints(context, Values, PrimaryPen);
            }
            else if (_selectedGraph == 1)
            {
                DrawPoints(context, Values, PrimaryPen);
                DrawPoints(context, SecondaryValues, SecondaryPen);
            }
        }

        base.Render(context);
    }

    private void DrawXLines(DrawingContext context)
    {
        GridPen.LineCap = PenLineCap.Round;
        GridPen.Thickness = 3;

        for (int i = 0; i < XValues.Count; i++)
        {
            var x = _xMargin + i * _chartXSpacing;
            context.DrawLine(GridPen, new Point(x, 0 + XAxisTextBox.DesiredSize.Height), new Point(x, Bounds.Height - _bottomMargin));
            DrawXValueText(context, x, XValues[i]);
        }
    }

    private void DrawXValueText(DrawingContext context, in double xPos, in float value)
    {
        var textWithPostfix = string.Empty;
        var textWithoutPostfix = string.Empty;

        if (ShowSign && value > 0)
            textWithoutPostfix = textWithPostfix = "+";

        var valueString = value.ToString();
        if (!ShowSign && value < 0)
            valueString = value.ToString().Replace("-", null);

        textWithPostfix += $"{valueString}{YPostfix}";
        textWithoutPostfix += $"{valueString}";

        var formattedTextWithPostfix = new FormattedText(textWithPostfix, _de, FlowDirection.LeftToRight, _currentFaceType, FontSize, GridBrush);

        var valueStringWithoutPrefix = value.ToString().Replace("-", null);
        var valueWithoutPrefixFormatted = new FormattedText(valueStringWithoutPrefix, _de, FlowDirection.LeftToRight, _currentFaceType, FontSize, GridBrush);
        var valueWithoutPrefixGeometry = valueWithoutPrefixFormatted.BuildGeometry(new Point(0, 0));

        var textWithoutPostfixformatted = new FormattedText(textWithoutPostfix, _de, FlowDirection.LeftToRight, _currentFaceType, FontSize, GridBrush);
        var textWithoutPostfixGeometry = textWithoutPostfixformatted.BuildGeometry(new Point(0, 0));

        var prefixWidth = textWithoutPostfixGeometry!.Bounds.Width - valueWithoutPrefixGeometry!.Bounds.Width;
        var numerWidth = valueWithoutPrefixGeometry.Bounds.Width;
        var xOffset = (numerWidth / 2f) + prefixWidth;
        context.DrawText(formattedTextWithPostfix, new Point(xPos - xOffset, Bounds.Height - valueWithoutPrefixGeometry.Bounds.Height - _bottomMargin / 2f));
    }

    private void DrawLinesBetweenPoints(DrawingContext context)
    {
        var points = GetPoints(Values);
        var secondPoints = GetPoints(SecondaryValues);
        PrimaryPen.Thickness = 4;
        SecondaryPen.Thickness = 4;

        for (int i = 1; i < points.Length; i++)
        {

            if (_selectedGraph == 0)
            {
                DrawGradiand(context, points[i], points[i - 1], PrimaryGradiand);

                if (secondPoints is not null && secondPoints.Length != 0)
                    context.DrawLine(SecondaryPen, secondPoints[i], secondPoints[i - 1]);

                context.DrawLine(PrimaryPen, points[i], points[i - 1]);
            }
            else if (_selectedGraph == 1)
            {
                DrawGradiand(context, secondPoints![i], secondPoints[i - 1], SecondaryGradiand);

                context.DrawLine(PrimaryPen, points[i], points[i - 1]);
                context.DrawLine(SecondaryPen, secondPoints![i], secondPoints[i - 1]);
            }
        }
    }

    private void DrawGradiand(DrawingContext context, Point a, Point b, LinearGradientBrush gradientBrush)
    {
        var polygonPoints = new List<Point> { b, a, new Point(a.X, Bounds.Height-_bottomMargin), new Point(b.X, Bounds.Height- _bottomMargin) };
        var polylineGeometry = new PolylineGeometry(polygonPoints, true);
        context.DrawGeometry(gradientBrush, null, polylineGeometry);
    }

    private void DrawPoints(DrawingContext context, in IList<float> values, Pen pen)
    {
        var points = GetPoints(values);
        pen.Thickness = 3;

        for (int i = 0; i < points.Length; i++)
        {
            context.DrawEllipse(this.ChartBackground, pen, new Point(points[i].X, points[i].Y), ValueRadius, ValueRadius);

            var typeface = new Typeface(this.FontFamily.Name, FontStyle.Normal, FontWeight.ExtraBold);
            var de = new CultureInfo("de-DE");

            var temp = values[i];
            var text = string.Empty;

            text += $"{temp}{ValuePostfix}".Replace("-", null); ;

            var formattedText = new FormattedText(text, de, FlowDirection.LeftToRight, typeface, this.FontSize, GridBrush);
            var tempNumberTextGeometry = formattedText.BuildGeometry(new Point(0, 0));

            var xOffset = tempNumberTextGeometry!.Bounds.Width / 2f;
            context.DrawText(formattedText, new Point(points[i].X - xOffset, points[i].Y - tempNumberTextGeometry.Bounds.Height));
        }
    }

    #endregion

    #region utility functions
    private int IsHit(Point point, in IList<float> values)
    {
        var currentVector = new Vector2((float)point.X, (float)point.Y);
        var points = GetPoints(values);
        for (int i = 0; i < points.Length; i++)
        {
            var vector = new Vector2((float)points[i].X, (float)points[i].Y);
            var distance = Vector2.Distance(currentVector, vector);
            if (distance <= ValueRadius)
                return i;
        }

        return -1;
    }

    private Point[] GetPoints(in IList<float> values)
    {
        var boxes = new Point[values.Count];

        for (int i = 0; i <= XValues.Count; i++)
        {
            if (i >= values.Count) break;

            var tempInP = values[i] / (float)MaxY;
            var totalMargin = XAxisTextBox.DesiredSize.Height + _bottomMargin;
            var difference = Bounds.Height - totalMargin;

            var y = Bounds.Height - difference * tempInP - _bottomMargin;
            var x = _xMargin + i * _chartXSpacing;

            boxes[i] = new Point(x, y);
        }

        return boxes;
    }

    private void CalculateChartBounds()
    {
        if (_currentFaceType.FontFamily?.Name != this.FontFamily.Name)
            _currentFaceType = new Typeface(this.FontFamily.Name, FontStyle.Normal, FontWeight.Bold);

        _chartWidth = Bounds.Width - _xMargin * 2;
        _chartXSpacing = _chartWidth / (XValues.Count - 1);
    }
    private float LocalPointToYValue(Point point)
    {
        var h = Bounds.Height;

        var yStart = XAxisTextBox.DesiredSize.Height;
        var yEnd = h - _bottomMargin;

        if (point.Y <= yStart)
            return MaxY;

        if (point.Y >= yEnd)
            return 0;

        var proportion = (point.Y - yStart) / (yEnd - yStart);

        var value = (1 - proportion) * MaxY;

        return (int)value;
    }
    #endregion

    #region Event functions
    private void LineChartView_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e) => _selectedXIndex = -1;
    private void RedrawOnCollectionChange(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => InvalidateVisual();
    private void LineChartView_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (_selectedXIndex == -1)
            return;

        if(!IsEditable) return;

        var point = e.GetCurrentPoint(sender as Control);

        var x = point.Position.X;
        var y = point.Position.Y;
        if (_selectedGraph == 0)
        {
            Values[_selectedXIndex] = LocalPointToYValue(new Point(x, y));
        }
        else if (_selectedGraph == 1)
        {
            SecondaryValues[_selectedXIndex] = LocalPointToYValue(new Point(x, y));
        }
    }

    private void LineChartView_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        var x = point.Position.X;
        var y = point.Position.Y;

        var isHit = IsHit(new Point(x, y), Values);
        if (isHit != -1)
        {
            _selectedXIndex = isHit;
            _selectedGraph = 0;
            InvalidateVisual();
            return;
        }

        var isSecondHit = _selectedXIndex = IsHit(new Point(x, y), SecondaryValues);
        if (isSecondHit != -1)
        {
            _selectedGraph = 1;
            InvalidateVisual();
        }
    }

    private void LineChartView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => InvalidateVisual();   

    #endregion
}