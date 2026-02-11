using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Diagnostics;
using WateryTart.Core.Extensions;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;
public partial class MainWindow : Window
{
    private ISettings? _settings;
    private ITrayService? _trayService;
    private MainWindowViewModel vm;
    public MainWindow()
    {

        this.PointerPressed += (s, e) =>
        {
            if (e.Properties.IsXButton1Pressed)
            {
                vm?.GoBack.ExecuteIfCan(null); ;
            }

        };

        this.Activated += (s, e) =>
        {
            vm = Host.DataContext as MainWindowViewModel;
            if (vm == null)
                return;

            _settings = App.Container.GetRequiredService<ISettings>();
            if (_settings.WindowWidth != 0)
            {
                Width = _settings.WindowWidth;
                Height = _settings.WindowHeight;
                Position = new Avalonia.PixelPoint((int)_settings.WindowPosX, (int)_settings.WindowPosY);
            }

            Resized += MainWindow_Resized;
            PositionChanged += MainWindow_PositionChanged;

            // Initialize tray service
            try
            {
                _trayService = new TrayService();
                _trayService.Initialize(this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tray service initialization failed: {ex.Message}");
            }
        };
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        _trayService?.Dispose();
        base.OnClosed(e);
    }

    private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        _settings?.WindowPosX = e.Point.X;
        _settings?.WindowPosY = e.Point.Y;
    }

    private void MainWindow_Resized(object? sender, WindowResizedEventArgs e)
    {
        _settings?.WindowWidth = e.ClientSize.Width;
        _settings?.WindowHeight = e.ClientSize.Height;
    }
}