using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace WateryTart.Core.Controls;

public class ResponsiveColumnPanel : TemplatedControl
{
    private bool _isWideView;
    private Grid? _contentGridContainer;
    private ContentPresenter? _firstColumnContent;
    private ContentPresenter? _secondColumnContent;

    public static readonly StyledProperty<object?> FirstColumnProperty = AvaloniaProperty.Register<ResponsiveColumnPanel, object?>(nameof(FirstColumn));
    public static readonly StyledProperty<object?> SecondColumnProperty = AvaloniaProperty.Register<ResponsiveColumnPanel, object?>(nameof(SecondColumn));
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<ResponsiveColumnPanel, object?>(nameof(Header));
    public static readonly StyledProperty<double> BreakpointProperty = AvaloniaProperty.Register<ResponsiveColumnPanel, double>(nameof(Breakpoint), 600.0);

    public object? FirstColumn
    {
        get => GetValue(FirstColumnProperty);
        set => SetValue(FirstColumnProperty, value);
    }

    public object? SecondColumn
    {
        get => GetValue(SecondColumnProperty);
        set => SetValue(SecondColumnProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public double Breakpoint
    {
        get => GetValue(BreakpointProperty);
        set => SetValue(BreakpointProperty, value);
    }

    public ResponsiveColumnPanel()
    {
        SizeChanged += OnSizeChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Unsubscribe from previous events if any
        _firstColumnContent?.SizeChanged -= OnFirstColumnSizeChanged;

        // Cache template element references
        _contentGridContainer = e.NameScope.Find<Grid>("ContentGridContainer");
        _firstColumnContent = e.NameScope.Find<ContentPresenter>("FirstColumnContent");
        _secondColumnContent = e.NameScope.Find<ContentPresenter>("SecondColumnContent");

        // Subscribe to first column size changes
        _firstColumnContent?.SizeChanged += OnFirstColumnSizeChanged;

        // Initialize _isWideView based on current width
        _isWideView = Bounds.Width > Breakpoint;
        UpdateLayout();
    }

    private void OnFirstColumnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // When in wide view, match the second column height to the first column
        if (_isWideView && _secondColumnContent is not null)
        {
            _secondColumnContent.MaxHeight = e.NewSize.Height;
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        bool shouldBeWide = e.NewSize.Width > Breakpoint;
        _isWideView = shouldBeWide;
        UpdateLayout();
    }

    private new void UpdateLayout()
    {
        if (_contentGridContainer is null) return;

        _contentGridContainer.ColumnDefinitions.Clear();
        _contentGridContainer.RowDefinitions.Clear();

        if (_isWideView)
        {
            // Two-column layout
            _contentGridContainer.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _contentGridContainer.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _contentGridContainer.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            if (_firstColumnContent is not null)
                Grid.SetColumn(_firstColumnContent, 0);

            if (_secondColumnContent is not null)
            {
                Grid.SetColumn(_secondColumnContent, 1);
                Grid.SetRow(_secondColumnContent, 0);
                
                // Apply current first column height as max height
                if (_firstColumnContent is not null)
                {
                    _secondColumnContent.MaxHeight = _firstColumnContent.Bounds.Height;
                }
            }
        }
        else
        {
            // Single-column layout (stack vertically)
            _contentGridContainer.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _contentGridContainer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _contentGridContainer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            if (_firstColumnContent is not null)
            {
                Grid.SetColumn(_firstColumnContent, 0);
                Grid.SetRow(_firstColumnContent, 0);
            }

            if (_secondColumnContent is not null)
            {
                Grid.SetColumn(_secondColumnContent, 0);
                Grid.SetRow(_secondColumnContent, 1);
                
                // Remove height restriction in single column mode
                _secondColumnContent.MaxHeight = double.PositiveInfinity;
            }
        }
    }
}