using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Linq;
using System.Reflection;

namespace Heating_Control_UI;
public class SvgImage : Control
{
    static SvgImage()
    {
        AffectsRender<LineChartView>(ForegroundProperty);
    }

    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty.Register<SvgImage, IBrush>(nameof(Foreground), Brushes.Black);
    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    private SKColor _renderSaveForegroundColor;
    private Stretch _renderSaveStretch = Stretch.None;
    private string _renderSaveSource;
    private double _renderSaveOpacity = 1.0;

    public static readonly StyledProperty<string> SourceProperty = AvaloniaProperty.Register<SvgImage, string>(nameof(Source), string.Empty);
    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<SvgImage, Stretch>(nameof(Stretch), Stretch.Fill);
    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }



    RenderingLogic2 renderingLogic;
    private event Action<SKCanvas, int> _skiaRenderAction;

    public SvgImage()
    {
        this.GetObservable(OpacityProperty).Subscribe(newValue => _renderSaveOpacity = newValue);
        this.GetObservable(SourceProperty).Subscribe(newValue => CreateSvgPath(newValue));
        this.GetObservable(StretchProperty).Subscribe(newValue => _renderSaveStretch = newValue);

        renderingLogic = new RenderingLogic2();
        renderingLogic.RenderCall2 += RenderSkia;
        //_skiaRenderAction += RenderSkia;

        Unloaded += SvgImage_Unloaded;
    }

    private void SvgImage_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _svgImage.Picture.Dispose();
    }

    long calledRender = 0;
    long calledSkiaRender = 0;
    public override void Render(DrawingContext context)
    {
        calledRender++;
        var foregroundColor = ((ImmutableSolidColorBrush)Foreground).Color;
        _renderSaveForegroundColor = new SKColor(foregroundColor.R, foregroundColor.G, foregroundColor.B, (byte)(_renderSaveOpacity * 255));
        _renderSaveSource = Source;

        renderingLogic.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        renderingLogic.NeedRedraw = true;
        //var test = new RenderingLogic2();
        //test.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        //test.RenderCall2 += RenderSkia;
        context.Custom(renderingLogic);
    }
    private void RenderSkia(SKCanvas canvas, int id)
    {
        //if(renderingLogic.Id != id)
        //{
        //    int i = 0;
        //}
        calledSkiaRender++;
        if (string.IsNullOrEmpty(_renderSaveSource)) return;

        if (_renderSaveStretch != Stretch.None)
            ScaleCanvas(canvas);

        SKPaint sKPaint = new SKPaint();
        //sKPaint.Color = SKColors.White;
        sKPaint.IsAntialias = true;
        canvas.DrawPicture(_svgImage.Picture, sKPaint);

    }

    private readonly SkiaSharp.Extended.Svg.SKSvg _svgImage = new();
    private void CreateSvgPath(string source)
    {
        if (string.IsNullOrEmpty(source)) return;
        var assembly = Assembly.GetCallingAssembly();

        var names = assembly.GetManifestResourceNames();
        var assemblySourceName = source.ToLower().Replace('/', '.');
        var resourceName = names.FirstOrDefault(name => name.ToLower().Contains(assemblySourceName));
        if (string.IsNullOrEmpty(resourceName)) return;

        using var assemblyStream = assembly.GetManifestResourceStream(resourceName)!;
        var box = _svgImage.Load(assemblyStream);
        box.Dispose();

        if (_svgImage.Picture is null) return;
        _imageSize = new Size(_svgImage.ViewBox.Width, _svgImage.ViewBox.Height);
    }

    private Size _imageSize = new Size(0, 0);

    private void ScaleCanvas(SKCanvas canvas)
    {
        float imageHeight = (float)_imageSize.Height;
        float imageWidth = (float)_imageSize.Width;

        var destHeight = (float)Bounds.Height;
        var destWidth = (float)Bounds.Width;
        if (destHeight == float.NaN || destHeight == 0)
            destHeight = imageHeight;
        if (destWidth == float.NaN || destWidth == 0)
            destWidth = imageWidth;


        float scaleX = 1;
        float scaleY = 1;

        switch (_renderSaveStretch)
        {
            case Stretch.None:
                return;

            case Stretch.Fill:
                scaleX = destWidth / imageWidth;
                scaleY = destHeight / imageHeight;
                break;

            case Stretch.UniformToFill:
                var scaleToFill = Math.Max(destWidth / imageWidth, destHeight / imageHeight);
                scaleX = scaleY = scaleToFill;
                break;

            case Stretch.Uniform:
                var scaleUniform = Math.Min(destWidth / imageWidth, destHeight / imageHeight);
                scaleX = scaleY = scaleUniform;
                break;
        }


        canvas.Scale(scaleX, scaleY);
    }

}


public class RenderingLogic2 : ICustomDrawOperation
{
    public RenderingLogic2()
    {
        Id = instance = instance + 1;
    }

    public bool NeedRedraw {  get; set; } = true;

    private static int instance = 0;
    public int Id { get; private set; }

    public Action<SKCanvas, int> RenderCall2;
    public Rect Bounds { get; set; }

    public void Dispose() { }

    public bool Equals(ICustomDrawOperation? other) => other == this;

    public bool HitTest(Point p) { return false; }

    public void Render(ImmediateDrawingContext context)
    {
        //if (!NeedRedraw) return;

        if (Id == 2)
        {
            int i = 0;
        }

        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null)
        {
            return;
        }
        else
        {
           
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;
            RenderCall2?.Invoke(canvas, Id);
            NeedRedraw = false;
        }
    }
}