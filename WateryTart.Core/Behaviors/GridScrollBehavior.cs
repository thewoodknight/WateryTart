using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace WateryTart.Core.Behaviors;

/// <summary>
/// Infinite scroll behavior for grid/WrapPanel layouts
/// </summary>
public class GridScrollBehavior : Behavior<ListBox>
{
    private int _lastKnownItemCount = 0;
    private int _lastTriggeredAtCount = 0;
    private bool _hasTriggeredForCurrentBatch = false;
    private bool _hasPerformedInitialCheck = false;
    private ScrollViewer? _scrollViewer;
    private IDisposable? _offsetSubscription;
    private double _estimatedItemHeight = 220;

    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty =
        AvaloniaProperty.Register<GridScrollBehavior, ICommand?>(nameof(LoadMoreCommand));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<GridScrollBehavior, bool>(nameof(IsLoading));

    public static readonly StyledProperty<bool> HasMoreItemsProperty =
        AvaloniaProperty.Register<GridScrollBehavior, bool>(nameof(HasMoreItems), defaultValue: true);

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

    protected override void OnAttached()
    {
        base.OnAttached();
        Debug.WriteLine($"[Grid] OnAttached");
        
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

    private void OnListBoxLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine($"[Grid] OnListBoxLoaded fired");
        TryFindScrollViewer();
    }

    private void TryFindScrollViewer()
    {
        if (_scrollViewer != null)
        {
            Debug.WriteLine($"[Grid] ScrollViewer already found");
            return;
        }

        Debug.WriteLine($"[Grid] Attempting to find ScrollViewer...");
        
        // First, try to find a parent ScrollViewer (more likely to have proper constraints)
        var parent = AssociatedObject?.GetVisualAncestors()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
        
        if (parent != null)
        {
            _scrollViewer = parent;
            Debug.WriteLine($"[Grid] Found PARENT ScrollViewer");
        }
        else
        {
            // Fall back to internal ScrollViewer
            _scrollViewer = AssociatedObject?.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault();
            Debug.WriteLine($"[Grid] Found internal ScrollViewer (fallback)");
        }

        Debug.WriteLine($"[Grid] ScrollViewer found: {_scrollViewer != null}");

        if (_scrollViewer != null)
        {
            Debug.WriteLine($"[Grid] Initial Viewport: {_scrollViewer.Viewport}, Extent: {_scrollViewer.Extent}");
            
            // Check if scrolling is actually possible
            if (_scrollViewer.Viewport.Height >= _scrollViewer.Extent.Height - 1)
            {
                Debug.WriteLine($"[Grid] ⚠️ WARNING: Viewport equals Extent - no scrolling possible! ListBox may not be height-constrained.");
            }
            
            _offsetSubscription = _scrollViewer.GetObservable(ScrollViewer.OffsetProperty)
                .Subscribe(offset =>
                {
                    Debug.WriteLine($"[Grid] Scroll offset changed: {offset.Y}");
                    OnScrollChanged();
                });
            
            // Also observe Extent changes (when items load)
            _scrollViewer.GetObservable(ScrollViewer.ExtentProperty)
                .Subscribe(extent =>
                {
                    Debug.WriteLine($"[Grid] Extent changed: {extent.Height}, Viewport: {_scrollViewer.Viewport.Height}");
                });
        }
        else
        {
            Debug.WriteLine($"[Grid] ScrollViewer NOT found in visual tree");
            Debug.WriteLine($"[Grid] AssociatedObject: {AssociatedObject?.GetType().Name}");
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
            Debug.WriteLine($"[Grid] Attempting to find ScrollViewer from ContainerPrepared");
            TryFindScrollViewer();
        }

        if (e.Index == 0 && e.Container is Control container)
        {
            container.LayoutUpdated += (s, args) =>
            {
                if (container.Bounds.Height > 0 && container.Bounds.Height != _estimatedItemHeight)
                {
                    _estimatedItemHeight = container.Bounds.Height;
                    Debug.WriteLine($"[Grid] Item height: {_estimatedItemHeight}");
                }
            };
        }

        if (totalItems > _lastKnownItemCount)
        {
            _lastKnownItemCount = totalItems;
            _hasTriggeredForCurrentBatch = false;
            Debug.WriteLine($"[Grid] New batch: {totalItems} items");

            if (!_hasPerformedInitialCheck)
            {
                _hasPerformedInitialCheck = true;
                Debug.WriteLine($"[Grid] Scheduling initial check...");
                Avalonia.Threading.DispatcherTimer.RunOnce(() => CheckIfShouldLoadMore(), TimeSpan.FromMilliseconds(300));
            }
        }
    }

    private void OnScrollChanged()
    {
        CheckIfShouldLoadMore();
    }

    private void CheckIfShouldLoadMore()
    {
        Debug.WriteLine($"[Grid] CheckIfShouldLoadMore called: Command={LoadMoreCommand != null}, IsLoading={IsLoading}, HasMore={HasMoreItems}, Triggered={_hasTriggeredForCurrentBatch}");
        
        if (LoadMoreCommand == null || IsLoading || !HasMoreItems || _hasTriggeredForCurrentBatch)
            return;

        if (_scrollViewer == null)
        {
            Debug.WriteLine($"[Grid] ScrollViewer is null, cannot check");
            return;
        }
            
        var totalItems = _lastKnownItemCount;
        if (totalItems == 0)
            return;

        var itemsSinceLastTrigger = totalItems - _lastTriggeredAtCount;
        if (_lastTriggeredAtCount > 0 && itemsSinceLastTrigger < 40)
        {
            Debug.WriteLine($"[Grid] Not enough items since last trigger: {itemsSinceLastTrigger}");
            return;
        }

        var viewportHeight = _scrollViewer.Viewport.Height;
        var extentHeight = _scrollViewer.Extent.Height;
        var currentScroll = _scrollViewer.Offset.Y;
        var remainingScroll = extentHeight - currentScroll - viewportHeight;
        var estimatedRemainingRows = remainingScroll / _estimatedItemHeight;

        Debug.WriteLine($"[Grid] Check: Viewport={viewportHeight:F0}, Extent={extentHeight:F0}, Scroll={currentScroll:F0}, Remaining rows={estimatedRemainingRows:F1}");

        if (estimatedRemainingRows <= 3)
        {
            Debug.WriteLine($"[Grid] ✓✓✓ TRIGGERING LOAD! ✓✓✓");
            _hasTriggeredForCurrentBatch = true;
            _lastTriggeredAtCount = totalItems;
            LoadMoreCommand.Execute(null);
        }
    }
}