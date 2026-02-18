using ReactiveUI;
using System.Windows.Input;
using Material.Icons;
using WateryTart.Core.Messages;
using Xaml.Behaviors.SourceGenerators;
using CommunityToolkit.Mvvm.Input;

namespace WateryTart.Core.ViewModels.Menus;

public partial class MenuItemViewModel : ReactiveObject, ISmallViewModelBase, IMenuItemViewModel
{
    public bool Indented { get; }
    private readonly ICommand? _clickedCommand = null;
    private readonly MaterialIconKind _icon;
    private readonly string _title; public MaterialIconKind Icon => _icon;
    public string Title => _title;

    public MenuItemViewModel(string title, MaterialIconKind? icon, ICommand? clickedCommand, bool? indented = false)
    {
        Indented = indented != null ? indented.Value : false;

        _title = title;
        if (icon != null)
            _icon = (MaterialIconKind)icon;

        if (clickedCommand != null)
        _clickedCommand = clickedCommand;
    }

    [GenerateTypedAction]
    public void MenuItemClicked()
    {
        if (_clickedCommand == null)
            return;
        _clickedCommand.Execute(null);
        MessageBus.Current.SendMessage(new CloseMenuMessage());
    }
}