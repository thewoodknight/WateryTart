using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Windows.Input;

namespace WateryTart.Core.Behaviors;

public class InfiniteScrollBehavior : Behavior<ListBox>
{
    private int _lastKnownItemCount = 0;
    private int _highestIndexPrepared = -1;
    private int _lastTriggeredAtCount = 0;
    private bool _hasTriggeredForCurrentBatch = false;

    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty =
        AvaloniaProperty.Register<InfiniteScrollBehavior, ICommand?>(nameof(LoadMoreCommand));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<InfiniteScrollBehavior, bool>(nameof(IsLoading));

    public static readonly StyledProperty<bool> HasMoreItemsProperty =
        AvaloniaProperty.Register<InfiniteScrollBehavior, bool>(nameof(HasMoreItems), defaultValue: true);

    public static readonly StyledProperty<int> ThresholdProperty =
        AvaloniaProperty.Register<InfiniteScrollBehavior, int>(nameof(Threshold), defaultValue: 10);

    public static readonly StyledProperty<double> ThresholdPercentageProperty =
        AvaloniaProperty.Register<InfiniteScrollBehavior, double>(nameof(ThresholdPercentage), defaultValue: 0.20);

    public ICommand? LoadMoreCommand
    {
        get => GetValue(LoadMoreCommandProperty);
        set => SetValue(LoadMoreCommandProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool HasMoreItems
    {
        get => GetValue(HasMoreItemsProperty);
        set => SetValue(HasMoreItemsProperty, value);
    }

    public int Threshold
    {
        get => GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    public double ThresholdPercentage
    {
        get => GetValue(ThresholdPercentageProperty);
        set => SetValue(ThresholdPercentageProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.ContainerPrepared += OnContainerPrepared;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.ContainerPrepared -= OnContainerPrepared;
        }
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (LoadMoreCommand == null)
            return;

        if (IsLoading || !HasMoreItems)
            return;

        var totalItems = AssociatedObject?.ItemCount ?? 0;
        if (totalItems == 0)
            return;

        var itemIndex = e.Index;

        // Track highest index we've prepared
        if (itemIndex > _highestIndexPrepared)
        {
            _highestIndexPrepared = itemIndex;
        }

        // Check if new batch loaded (item count increased significantly)
        if (totalItems > _lastKnownItemCount)
        {
            _lastKnownItemCount = totalItems;
            _hasTriggeredForCurrentBatch = false;
        }

        // Prevent multiple triggers for the same batch
        if (_hasTriggeredForCurrentBatch)
            return;

        // Only trigger if we've loaded at least 40 new items since last trigger
        // This prevents rapid re-triggering (page size is 50, so this ensures we wait for most of the page)
        var itemsSinceLastTrigger = totalItems - _lastTriggeredAtCount;
        if (itemsSinceLastTrigger < 40)
            return;

        // Calculate threshold based on total items
        var threshold = Math.Max(Threshold, (int)(totalItems * ThresholdPercentage));
        
        // Check distance from highest prepared index to end (not current index)
        // This works for both list and grid layouts
        var itemsFromEnd = totalItems - _highestIndexPrepared - 1;

        // Trigger when within threshold of the end
        if (itemsFromEnd <= threshold && LoadMoreCommand.CanExecute(null))
        {
            _hasTriggeredForCurrentBatch = true;
            _lastTriggeredAtCount = totalItems;
            LoadMoreCommand.Execute(null);
        }
    }
}