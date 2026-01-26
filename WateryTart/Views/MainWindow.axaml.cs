using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using WateryTart.ViewModels;

namespace WateryTart;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        this.WhenActivated(disposables =>
        {
            var vm = DataContext as MainWindowViewModel;
            vm.Connect();


        });
        AvaloniaXamlLoader.Load(this);
    }
}