using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Heating_Control_UI.Controls;
using Heating_Control_UI.Utilities;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;

namespace Heating_Control_UI;
public class SvgImage : SkiaControl
{
    static SvgImage()
    {
        AffectsRender<TemperatureSelector>(
            SourceProperty,
            StretchProperty
            );
    }

    public static readonly StyledProperty<string> SourceProperty =
         AvaloniaProperty.Register<SvgImage, string>(nameof(Source));

    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly StyledProperty<Stretch> StretchProperty =
       AvaloniaProperty.Register<SvgImage, Stretch>(nameof(Stretch), Stretch.None);

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }


    private Size _imageSize;


    private readonly List<IDisposable> _disposables = new();
    public SvgImage()
    {
        var sourceDisposable = this.GetObservable(SourceProperty).Subscribe(OnSourceChanged);
        var stretchDisposably = this.GetObservable(StretchProperty).Subscribe(OnStretchChanged);

        _disposables.Add(sourceDisposable);
        _disposables.Add(stretchDisposably);
    }

    protected override void RenderSkia(SKCanvas canvas)
    {
        if (_sourceStream is null) return;

        if (_renderSaveStretch != Stretch.None)
            ScaleCanvas(canvas);

        SKPaint sKPaint = new SKPaint();
        sKPaint.IsAntialias = true;
        if (_svgImage.Picture is null)
        {
            var point = new SKPoint(0, 0);

            canvas.DrawBitmap(_resourceBitmap, point, sKPaint);
        }
        else
        {
            canvas.DrawPicture(_svgImage.Picture, sKPaint);
        }
    }

    protected override void OnDispose()
    {
        if (_sourceStream is not null)
            _sourceStream.Dispose();

        if (_resourceBitmap is not null)
            _resourceBitmap.Dispose();

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }


    private Stream? _sourceStream;
    private readonly SkiaSharp.Extended.Svg.SKSvg _svgImage = new();

    private Stretch _renderSaveStretch = Stretch.None;
    private SKBitmap? _resourceBitmap;

    private void OnStretchChanged(Stretch stretch)
    {
        _renderSaveStretch = Stretch;
    }

    private void OnSourceChanged(string source)
    {
        if (string.IsNullOrEmpty(Source)) return;

        var fileExtension = Path.GetExtension(Source);

        if (fileExtension is null) return;

        _sourceStream = GlobalCache.GetStream(Source);
        if (_sourceStream is null) return;

        switch (fileExtension)
        {
            case ".png":
                _resourceBitmap = SKBitmap.Decode(_sourceStream);

                if (_resourceBitmap is null) return;
                _imageSize = new Size(_resourceBitmap.Info.Width, _resourceBitmap.Info.Height);


                break;

            case ".svg":
                _svgImage.Load(_sourceStream);

                if (_svgImage.Picture is null) return;
                _imageSize = new Size(_svgImage.Picture.CullRect.Width, _svgImage.Picture.CullRect.Height);
                break;
        }

        if (SkiaBounds.Height == 0 || SkiaBounds.Height == double.NaN)
        {
            SkiaBounds = new Rect(SkiaBounds.X, SkiaBounds.Y, SkiaBounds.Width, _imageSize.Height);
            MinHeight = _imageSize.Height;
        }

        if (SkiaBounds.Width == 0 || SkiaBounds.Width == double.NaN)
        {
            SkiaBounds = new Rect(SkiaBounds.X, SkiaBounds.Y, _imageSize.Width, SkiaBounds.Height);
            MinWidth = _imageSize.Width;
        }
    }

    private void ScaleCanvas(SKCanvas canvas)
    {
        var imageHeight = (float)_imageSize.Height;
        var imageWidth = (float)_imageSize.Width;

        var destHeight = (float)Bounds.Height;
        var destWidth = (float)Bounds.Width;
        if (destHeight == float.NaN || destHeight == 0)
            destHeight = imageHeight;
        if (destWidth == float.NaN || destWidth == 0)
            destWidth = imageWidth;


        var scaleX = 1f;
        var scaleY = 1f;

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
