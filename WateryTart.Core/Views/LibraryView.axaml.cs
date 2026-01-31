using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class LibraryView : ReactiveUserControl<LibraryViewModel>
{
    public LibraryView()
    {
        InitializeComponent();
    }
}