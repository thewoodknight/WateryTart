using Avalonia.Controls;
using ReactiveUI.Avalonia;
using System.Diagnostics.CodeAnalysis;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;
public partial class LibraryView : ReactiveUserControl<LibraryViewModel>
{
    public LibraryView()
    {
        InitializeComponent();
    }
}
