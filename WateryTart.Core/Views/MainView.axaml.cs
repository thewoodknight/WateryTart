using Avalonia.Controls;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
    }


    private void MainView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        vm.Connect();
    }
}