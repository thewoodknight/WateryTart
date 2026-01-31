using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    ISettings _settings;
    public MainWindow()
    {
        this.WhenActivated(disposables =>
        {
            var vm = DataContext as MainWindowViewModel;
            vm.Connect();
            vm.Router.CurrentViewModel.Subscribe((_) =>
            {
                var sv = this.Find<ScrollViewer>("sv");
                if (sv != null)
                    sv.ScrollToHome();
            });
            _settings = App.Container.GetRequiredService<ISettings>();
            if (_settings.WindowWidth != 0)
            {
                Width = _settings.WindowWidth;
                Height = _settings.WindowHeight;
                Position = new Avalonia.PixelPoint((int)_settings.WindowPosX, (int)_settings.WindowPosY);
            }
            Resized += MainWindow_Resized;
            PositionChanged += MainWindow_PositionChanged;
        });


        AvaloniaXamlLoader.Load(this);
    }

    private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        _settings.WindowPosX = e.Point.X;
        _settings.WindowPosY = e.Point.Y;
    }

    private void MainWindow_Resized(object? sender, WindowResizedEventArgs e)
    {
        _settings.WindowWidth = e.ClientSize.Width;
        _settings.WindowHeight = e.ClientSize.Height;
    }


}