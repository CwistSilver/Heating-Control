using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;

namespace Heating_Control_UI;

public partial class SelectorView : UserControl
{
    static SelectorView()
    {
        AffectsRender<SelectorView>(
            CurrentTemperatureProperty,
            TitleProperty,
            PostfixProperty
            );
    }

    public static readonly RoutedEvent<RoutedEventArgs> TemperatureChangedEvent = RoutedEvent.Register<SelectorView, RoutedEventArgs>(nameof(TemperatureChanged), RoutingStrategies.Bubble);
    public event EventHandler<RoutedEventArgs> TemperatureChanged
    {
        add { AddHandler(TemperatureChangedEvent, value); }
        remove { RemoveHandler(TemperatureChangedEvent, value); }
    }


    public static readonly StyledProperty<float> CurrentTemperatureProperty = AvaloniaProperty.Register<SelectorView, float>(nameof(CurrentTemperature), 23f, defaultBindingMode: BindingMode.TwoWay);
    public float CurrentTemperature
    {
        get => GetValue(CurrentTemperatureProperty);
        set => SetValue(CurrentTemperatureProperty, value);
    }

    public static readonly StyledProperty<float> ValueStepProperty = AvaloniaProperty.Register<SelectorView, float>(nameof(ValueStep), 1f, defaultBindingMode: BindingMode.TwoWay);
    public float ValueStep
    {
        get => GetValue(ValueStepProperty);
        set => SetValue(ValueStepProperty, value);
    }

    public static readonly StyledProperty<float> MinValueProperty = AvaloniaProperty.Register<SelectorView, float>(nameof(MinValue), 0);
    public float MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public static readonly StyledProperty<float> MaxValueProperty = AvaloniaProperty.Register<SelectorView, float>(nameof(MaxValue), 32);
    public float MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<SelectorView, string>(nameof(Title), string.Empty);
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> AddButtonAccessabilityTitleProperty = AvaloniaProperty.Register<SelectorView, string>(nameof(AddButtonAccessabilityTitle), string.Empty);
    public string AddButtonAccessabilityTitle
    {
        get => GetValue(AddButtonAccessabilityTitleProperty);
        set => SetValue(AddButtonAccessabilityTitleProperty, value);
    }

    public static readonly StyledProperty<string> RemoveButtonAccessabilityTitleProperty = AvaloniaProperty.Register<SelectorView, string>(nameof(RemoveButtonAccessabilityTitle), string.Empty);
    public string RemoveButtonAccessabilityTitle
    {
        get => GetValue(RemoveButtonAccessabilityTitleProperty);
        set => SetValue(RemoveButtonAccessabilityTitleProperty, value);
    }

    public static readonly StyledProperty<string> PostfixProperty = AvaloniaProperty.Register<SelectorView, string>(nameof(Postfix), string.Empty);
    public string Postfix
    {
        get => GetValue(PostfixProperty);
        set => SetValue(PostfixProperty, value);
    }

    private readonly List<IDisposable> _disposables = new(2);
    public SelectorView()
    {
        InitializeComponent();
        _disposables.Add(this.GetObservable(TitleProperty).Subscribe(newTitle =>
        {
            TitleLabel.Content = newTitle;
            AddButtonAccessabilityTitle = $"{newTitle} wurde auf {CurrentTemperatureText}{Postfix} erhöht.";
            RemoveButtonAccessabilityTitle = $"{newTitle} wurde auf {CurrentTemperatureText}{Postfix} verringert.";
        }));
        _disposables.Add(this.GetObservable(CurrentTemperatureProperty).Subscribe(newValue =>
        {
            CurrentTemperatureText.Text = $"{newValue}{Postfix}";
            AddButtonAccessabilityTitle = $"{Title} wurde auf {newValue}{Postfix} erhöht.";
            RemoveButtonAccessabilityTitle = $"{Title} wurde auf {newValue}{Postfix} verringert.";
            ChangeButtonState();
        }));
        App.GetTopLevel()!.PointerPressed += GetTopLevel_PointerPressed;

        DetachedFromLogicalTree += TemperatureSelector_DetachedFromLogicalTree;
    }

    private void TemperatureSelector_DetachedFromLogicalTree(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        DetachedFromLogicalTree -= TemperatureSelector_DetachedFromLogicalTree;
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
        if (MaxValue <= CurrentTemperature)
            return;

        var newValue = CurrentTemperature + ValueStep;
        CurrentTemperature = Math.Clamp(newValue, MinValue, MaxValue);
        this.OnTemperatureChanged();
    }

    private void Remove_Click(object? sender, RoutedEventArgs e)
    {
        if (MinValue >= CurrentTemperature)
            return;

        var newValue = CurrentTemperature - ValueStep;
        CurrentTemperature = Math.Clamp(newValue, MinValue, MaxValue);
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
            CurrentTemperature = Math.Clamp(floatValue, MinValue, MaxValue);
    }

    private void ChangeButtonState()
    {
        if (MinValue >= CurrentTemperature)
        {
            RemoveButton.IsEnabled = false;
        }
        else
        {
            RemoveButton.IsEnabled = true;
        }

        if (MaxValue <= CurrentTemperature)
        {
            AddButton.IsEnabled = false;
        }
        else
        {
            AddButton.IsEnabled = true;
        }
    }
}