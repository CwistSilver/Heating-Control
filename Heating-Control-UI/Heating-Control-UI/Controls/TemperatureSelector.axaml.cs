using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;

namespace Heating_Control_UI;

public partial class TemperatureSelector : UserControl
{
    static TemperatureSelector()
    {
        AffectsRender<TemperatureSelector>(
            CurrentTemperatureProperty,
            TitleProperty,
            PostfixProperty
            );
    }

    public static readonly RoutedEvent<RoutedEventArgs> TemperatureChangedEvent = RoutedEvent.Register<TemperatureSelector, RoutedEventArgs>(nameof(TemperatureChanged), RoutingStrategies.Bubble);
    public event EventHandler<RoutedEventArgs> TemperatureChanged
    {
        add { AddHandler(TemperatureChangedEvent, value); }
        remove { RemoveHandler(TemperatureChangedEvent, value); }
    }


    public static readonly StyledProperty<float> CurrentTemperatureProperty = AvaloniaProperty.Register<TemperatureSelector, float>(nameof(CurrentTemperature), 23f, defaultBindingMode: BindingMode.TwoWay);
    public float CurrentTemperature
    {
        get => GetValue(CurrentTemperatureProperty);
        set => SetValue(CurrentTemperatureProperty, value);
    }

    public static readonly StyledProperty<float> ValueStepProperty = AvaloniaProperty.Register<TemperatureSelector, float>(nameof(ValueStep), 1f, defaultBindingMode: BindingMode.TwoWay);
    public float ValueStep
    {
        get => GetValue(ValueStepProperty);
        set => SetValue(ValueStepProperty, value);
    }

    public static readonly StyledProperty<float> MinTemperatureProperty = AvaloniaProperty.Register<TemperatureSelector, float>(nameof(MinTemperature), 0);
    public float MinTemperature
    {
        get => GetValue(MinTemperatureProperty);
        set => SetValue(MinTemperatureProperty, value);
    }

    public static readonly StyledProperty<float> MaxTemperatureProperty = AvaloniaProperty.Register<TemperatureSelector, float>(nameof(MaxTemperature), 32);
    public float MaxTemperature
    {
        get => GetValue(MaxTemperatureProperty);
        set => SetValue(MaxTemperatureProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<TemperatureSelector, string>(nameof(Title), string.Empty);
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> PostfixProperty = AvaloniaProperty.Register<TemperatureSelector, string>(nameof(Postfix), string.Empty);
    public string Postfix
    {
        get => GetValue(PostfixProperty);
        set => SetValue(PostfixProperty, value);
    }

    private readonly List<IDisposable> _disposables = new(2);
    public TemperatureSelector()
    {
        InitializeComponent();
        _disposables.Add(this.GetObservable(TitleProperty).Subscribe(newTitle => TitleLabel.Content = newTitle));
        _disposables.Add(this.GetObservable(CurrentTemperatureProperty).Subscribe(newValue => CurrentTemperatureText.Text = $"{newValue}{Postfix}"));
        App.GetTopLevel()!.PointerPressed += GetTopLevel_PointerPressed;

        DetachedFromLogicalTree += TemperatureSelector_DetachedFromLogicalTree;
    }

    private void TemperatureSelector_DetachedFromLogicalTree(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        this.DetachedFromLogicalTree -= TemperatureSelector_DetachedFromLogicalTree;
        App.GetTopLevel()!.PointerPressed -= GetTopLevel_PointerPressed;
        

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    private void GetTopLevel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (ValueTextBox.IsVisible)
            CloseTextBox();
    }

    private void Add_Click(object? sender, RoutedEventArgs e)
    {
        if (MaxTemperature <= CurrentTemperature)
            return;

        var newValue = CurrentTemperature + ValueStep;
        CurrentTemperature = Math.Clamp(newValue, MinTemperature, MaxTemperature);
        this.OnTemperatureChanged();
    }

    private void Remove_Click(object? sender, RoutedEventArgs e)
    {
        if (MinTemperature >= CurrentTemperature)
            return;

        var newValue = CurrentTemperature - ValueStep;
        CurrentTemperature = Math.Clamp(newValue, MinTemperature, MaxTemperature);
        this.OnTemperatureChanged();
    }

    protected virtual void OnTemperatureChanged()
    {
        RaiseEvent(new RoutedEventArgs(TemperatureChangedEvent));
    }

    private void TextBlock_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        ValueTextBox.Text = CurrentTemperature.ToString();
        ValueTextBox.IsVisible = true;
        ValueTextBox.Focus();
    }

    private void TextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        CloseTextBox();
    }

    private void TextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
            CloseTextBox();
    }

    private void CloseTextBox()
    {
        ValueTextBox.IsVisible = false;
        if (float.TryParse(ValueTextBox.Text, out var floatValue))
            CurrentTemperature = Math.Clamp(floatValue, MinTemperature, MaxTemperature);
    }


}