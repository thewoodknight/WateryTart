using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using System;
using System.Linq;
using System.Windows.Input;

namespace WateryTart.Core.Behaviors;

/// <summary>
/// Infinite scroll behavior for grid/WrapPanel layouts
/// </summary>
public class GridScrollBehavior : Behavior<ListBox>
{
    public static readonly StyledProperty<bool> HasMoreItemsProperty = AvaloniaProperty.Register<GridScrollBehavior, bool>(nameof(HasMoreItems), defaultValue: true);
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<GridScrollBehavior, bool>(nameof(IsLoading));
    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty = AvaloniaProperty.Register<GridScrollBehavior, ICommand?>(nameof(LoadMoreCommand));

    private double _estimatedItemHeight = 220;
    private bool _hasPerformedInitialCheck = false;
    private bool _hasTriggeredForCurrentBatch = false;
    private int _lastKnownItemCount = 0;
    private int _lastTriggeredAtCount = 0;
    private IDisposable? _offsetSubscription;
    private ScrollViewer? _scrollViewer;

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

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.ContainerPrepared += OnContainerPrepared;
            AssociatedObject.Loaded += OnListBoxLoaded;

            // Also try to find ScrollViewer after a delay
            Avalonia.Threading.DispatcherTimer.RunOnce(() => TryFindScrollViewer(), TimeSpan.FromMilliseconds(500));
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.ContainerPrepared -= OnContainerPrepared;
            AssociatedObject.Loaded -= OnListBoxLoaded;
        }

        _offsetSubscription?.Dispose();
    }

    private void CheckIfShouldLoadMore()
    {
        if (LoadMoreCommand == null || IsLoading || !HasMoreItems || _hasTriggeredForCurrentBatch)
            return;

        if (_scrollViewer == null)
        {
            return;
        }

        var totalItems = _lastKnownItemCount;
        if (totalItems == 0)
            return;

        var itemsSinceLastTrigger = totalItems - _lastTriggeredAtCount;
        if (_lastTriggeredAtCount > 0 && itemsSinceLastTrigger < 40)
        {
            return;
        }

        var viewportHeight = _scrollViewer.Viewport.Height;
        var extentHeight = _scrollViewer.Extent.Height;
        var currentScroll = _scrollViewer.Offset.Y;
        var remainingScroll = extentHeight - currentScroll - viewportHeight;
        var estimatedRemainingRows = remainingScroll / _estimatedItemHeight;

        if (estimatedRemainingRows <= 3)
        {
            _hasTriggeredForCurrentBatch = true;
            _lastTriggeredAtCount = totalItems;
            LoadMoreCommand.Execute(null);
        }
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        var totalItems = AssociatedObject?.ItemCount ?? 0;
        if (totalItems == 0)
            return;

        // Try to find ScrollViewer if we don't have it yet
        if (_scrollViewer == null && e.Index == 5)
        {
            TryFindScrollViewer();
        }

        if (e.Index == 0 && e.Container is Control container)
        {
            container.LayoutUpdated += (s, args) =>
            {
                if (container.Bounds.Height > 0 && container.Bounds.Height != _estimatedItemHeight)
                {
                    _estimatedItemHeight = container.Bounds.Height;
                }
            };
        }

        if (totalItems > _lastKnownItemCount)
        {
            _lastKnownItemCount = totalItems;
            _hasTriggeredForCurrentBatch = false;

            if (!_hasPerformedInitialCheck)
            {
                _hasPerformedInitialCheck = true;
                Avalonia.Threading.DispatcherTimer.RunOnce(() => CheckIfShouldLoadMore(), TimeSpan.FromMilliseconds(300));
            }
        }
    }

    private void OnListBoxLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TryFindScrollViewer();
    }

    private void OnScrollChanged()
    {
        CheckIfShouldLoadMore();
    }

    private void TryFindScrollViewer()
    {
        if (_scrollViewer != null)
        {
            return;
        }

        // First, try to find a parent ScrollViewer (more likely to have proper constraints)
        var parent = AssociatedObject?.GetVisualAncestors()
            .OfType<ScrollViewer>()
            .FirstOrDefault();

        if (parent != null)
        {
            _scrollViewer = parent;
        }
        else
        {
            // Fall back to internal ScrollViewer
            _scrollViewer = AssociatedObject?.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault();
        }

        if (_scrollViewer != null)
        {
            _offsetSubscription = _scrollViewer.GetObservable(ScrollViewer.OffsetProperty)
                .Subscribe(offset =>
                {
                    OnScrollChanged();
                });
        }
        else
        {
        }
    }
}