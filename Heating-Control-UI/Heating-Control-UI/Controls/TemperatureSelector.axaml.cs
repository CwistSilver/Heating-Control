using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using System;

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

    public static readonly RoutedEvent<RoutedEventArgs> TemperatureChangedEvent =
    RoutedEvent.Register<TemperatureSelector, RoutedEventArgs>(nameof(TemperatureChanged), RoutingStrategies.Bubble);
    public event EventHandler<RoutedEventArgs> TemperatureChanged
    {
        add { AddHandler(TemperatureChangedEvent, value); }
        remove { RemoveHandler(TemperatureChangedEvent, value); }
    }



    public static readonly StyledProperty<int> CurrentTemperatureProperty = AvaloniaProperty.Register<TemperatureSelector, int>(nameof(CurrentTemperature), 23, defaultBindingMode: BindingMode.TwoWay);
    public int CurrentTemperature
    {
        get => GetValue(CurrentTemperatureProperty);
        set => SetValue(CurrentTemperatureProperty, value);
    }

    public static readonly StyledProperty<int> MinTemperatureProperty = AvaloniaProperty.Register<TemperatureSelector, int>(nameof(MinTemperature), 0);
    public int MinTemperature
    {
        get => GetValue(MinTemperatureProperty);
        set => SetValue(MinTemperatureProperty, value);
    }

    public static readonly StyledProperty<int> MaxTemperatureProperty = AvaloniaProperty.Register<TemperatureSelector, int>(nameof(MaxTemperature), 32);
    public int MaxTemperature
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

    public TemperatureSelector()
    {
        InitializeComponent();
        this.GetObservable(TitleProperty).Subscribe(newTitle => TitleLabel.Content = newTitle);
        this.GetObservable(CurrentTemperatureProperty).Subscribe(newValue => CurrentTemperatureText.Text = $"{newValue}{Postfix}");
    }


    private void Add_Click(object? sender, RoutedEventArgs e)
    {
        if (MaxTemperature <= CurrentTemperature)
            return;

        CurrentTemperature++;
        this.OnTemperatureChanged();
    }

    private void Remove_Click(object? sender, RoutedEventArgs e)
    {
        if (MinTemperature >= CurrentTemperature)
            return;

        CurrentTemperature--;
        this.OnTemperatureChanged();
    }

    protected virtual void OnTemperatureChanged()
    {
        RaiseEvent(new RoutedEventArgs(TemperatureChangedEvent));
    }

}