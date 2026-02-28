using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using System.Windows.Input;

namespace WateryTart.Core.Controls;

public class BindableCommandSlider : Slider
{
    public static readonly StyledProperty<ICommand?> PointerPressedCommandProperty =
        AvaloniaProperty.Register<BindableCommandSlider, ICommand?>(nameof(PointerPressedCommand));

    public static readonly StyledProperty<ICommand?> PointerReleasedCommandProperty =
        AvaloniaProperty.Register<BindableCommandSlider, ICommand?>(nameof(PointerReleasedCommand));

    public ICommand? PointerPressedCommand
    {
        get => GetValue(PointerPressedCommandProperty);
        set => SetValue(PointerPressedCommandProperty, value);
    }

    public ICommand? PointerReleasedCommand
    {
        get => GetValue(PointerReleasedCommandProperty);
        set => SetValue(PointerReleasedCommandProperty, value);
    }

    public BindableCommandSlider()
    {
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (PointerPressedCommand?.CanExecute(Value) == true)
            PointerPressedCommand.Execute(Value);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (PointerReleasedCommand?.CanExecute(Value) == true)
            PointerReleasedCommand.Execute(Value);
    }
}