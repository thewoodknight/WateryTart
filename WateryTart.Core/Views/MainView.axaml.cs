using Avalonia.Controls;
using System;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void MainView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        _ = vm.Connect();
        vm.Router.CurrentViewModel.Subscribe((_) =>
        {
            var sv = this.Find<ScrollViewer>("sv");
            sv?.ScrollToHome();
        });
    }
}