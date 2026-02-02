using Avalonia;
using Avalonia.Controls;
using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.VisualTree;
using ReactiveUI;
using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels.Players;

namespace WateryTart.Core.Views.Players;

public partial class BigPlayerView : ReactiveUserControl<BigPlayerViewModel>
{
    private const double SmallDisplayBreakpointWidth = 800.0;
    private const double SmallDisplayBreakpointHeight = 480;
    private bool _isSmallDisplay;

    public BigPlayerView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
                // Subscribe immediately if already attached, otherwise wait
            SubscribeToWindowBounds(disposables);
        });
    }

    private void SubscribeToWindowBounds(CompositeDisposable disposables)
    {
        var window = this.FindAncestorOfType<Window>();
        
        if (window != null)
        {
            // Already attached, subscribe now
            SetupWindowBoundsObservable(window, disposables);
        }
        else
        {
            // Not attached yet, wait for AttachedToVisualTree
            void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
            {
                AttachedToVisualTree -= OnAttached;
                var win = this.FindAncestorOfType<Window>();
                if (win != null)
                {
                    SetupWindowBoundsObservable(win, disposables);
                }
            }
            
            AttachedToVisualTree += OnAttached;
            Disposable.Create(() => AttachedToVisualTree -= OnAttached).DisposeWith(disposables);
        }
    }

    private void SetupWindowBoundsObservable(Window window, CompositeDisposable disposables)
    {
        window.GetObservable(Window.BoundsProperty)
            .Subscribe(bounds =>
            {
                bool wasSmall = _isSmallDisplay;
                _isSmallDisplay = bounds.Width < SmallDisplayBreakpointWidth && bounds.Height < SmallDisplayBreakpointHeight;

                if (wasSmall != _isSmallDisplay && ViewModel != null)
                {
                    ViewModel.IsSmallDisplay = _isSmallDisplay;
                }
                
                // Also update immediately if ViewModel exists
                if (ViewModel != null && wasSmall == _isSmallDisplay)
                {
                    ViewModel.IsSmallDisplay = _isSmallDisplay;
                }
            })
            .DisposeWith(disposables);
    }
}