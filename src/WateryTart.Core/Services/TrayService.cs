using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using ReactiveUI;
using System;
using System.Diagnostics;
using WateryTart.Core.Settings;

namespace WateryTart.Core.Services;

public interface ITrayService
{
    void CreateTrayIcon();
    void Initialize(Window mainWindow);
    void Dispose();
}

public class TrayService : ITrayService
{
    private TrayIcon? _trayIcon;
    private Window? _mainWindow;
    private IDisposable? _windowStateSubscription;
    private bool _isExiting;
    private readonly ISettings settings;

    public TrayService(ISettings settings)
    {
        this.settings = settings;
    }
    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;
        CreateTrayIcon();
        HookWindowEvents();
    }

    public void CreateTrayIcon()
    {
        if (!settings.TrayIcon || _trayIcon != null)
            return;

        WindowIcon? icon = null;

        try
        {
            var uri = new Uri("avares://WateryTart.Core/Assets/logo_square.ico");
            using var stream = AssetLoader.Open(uri);
            icon = new WindowIcon(stream);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"✗ Failed to load tray icon: {ex.Message}");
        }

        _trayIcon = new TrayIcon
        {
            IsVisible = false,
            Icon = icon
        };

        var showItem = new NativeMenuItem { Header = "Show" };
        showItem.Click += (s, e) => ShowWindow();

        var exitItem = new NativeMenuItem { Header = "Exit" };
        exitItem.Click += (s, e) => ExitApplication();

        _trayIcon.Menu = [showItem, exitItem];

        _trayIcon.Clicked += TrayIcon_Clicked;
    }

    private void TrayIcon_Clicked(object? sender, EventArgs e)
    {
        ShowWindow();
    }

    private void HookWindowEvents()
    {
        if (_mainWindow == null)
            return;

        if (!settings.TrayIcon)
            return;

        _mainWindow.Closing += (s, e) =>
        {
            if (!_isExiting)
            {
                e.Cancel = true;
                MinimizeToTray();
            }
        };

        if (_mainWindow is IReactiveObject reactiveWindow)
        {
            _windowStateSubscription = reactiveWindow
                .WhenAnyValue(w => ((Window)w).WindowState)
                .Subscribe(state =>
                {
                    if (state == WindowState.Minimized && !_isExiting)
                    {
                        MinimizeToTray();
                    }
                });
        }
    }

    private void MinimizeToTray()
    {
        if (_mainWindow == null || _trayIcon == null || !settings.TrayIcon)
            return;

        _mainWindow.Hide();
        _trayIcon.IsVisible = true;
    }

    private void ShowWindow()
    {
        if (_mainWindow == null || _trayIcon == null)
            return;

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        if (_mainWindow != null)
        {
            _isExiting = true;
            _mainWindow.Close();
        }
    }

    public void Dispose()
    {
        _windowStateSubscription?.Dispose();
        _trayIcon?.Dispose();
        _trayIcon = null;
    }
}