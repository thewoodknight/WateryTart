using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System.Windows.Input;

namespace WateryTart.Core.Behaviors;

/// <summary>
/// Infinite scroll behavior for vertical list layouts
/// </summary>
public class ListScrollBehavior : Behavior<ListBox>
{
    public static readonly StyledProperty<bool> HasMoreItemsProperty = AvaloniaProperty.Register<ListScrollBehavior, bool>(nameof(HasMoreItems), defaultValue: true);
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<ListScrollBehavior, bool>(nameof(IsLoading));
    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty = AvaloniaProperty.Register<ListScrollBehavior, ICommand?>(nameof(LoadMoreCommand));
    public static readonly StyledProperty<int> ThresholdProperty = AvaloniaProperty.Register<ListScrollBehavior, int>(nameof(Threshold), defaultValue: 10);
    private bool _hasTriggeredForCurrentBatch = false;
    private int _lastKnownItemCount = 0;
    private int _lastTriggeredAtCount = 0;

    public bool HasMoreItems
    {
        get => GetValue(HasMoreItemsProperty);
        set => SetValue(HasMoreItemsProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public ICommand? LoadMoreCommand
    {
        get => GetValue(LoadMoreCommandProperty);
        set => SetValue(LoadMoreCommandProperty, value);
    }

    public int Threshold
    {
        get => GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.ContainerPrepared += OnContainerPrepared;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.ContainerPrepared -= OnContainerPrepared;
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (LoadMoreCommand == null || IsLoading || !HasMoreItems)
            return;

        var totalItems = AssociatedObject?.ItemCount ?? 0;
        if (totalItems == 0)
            return;

        // Reset flag when new items load
        if (totalItems > _lastKnownItemCount)
        {
            _lastKnownItemCount = totalItems;
            _hasTriggeredForCurrentBatch = false;
        }

        if (_hasTriggeredForCurrentBatch)
            return;

        // Require at least 40 items since last trigger
        if (totalItems - _lastTriggeredAtCount < 40)
            return;

        // Check if we're near the end
        var itemsFromEnd = totalItems - e.Index - 1;
        if (itemsFromEnd <= Threshold && LoadMoreCommand.CanExecute(null))
        {
            _hasTriggeredForCurrentBatch = true;
            _lastTriggeredAtCount = totalItems;
            LoadMoreCommand.Execute(null);
        }
    }
}