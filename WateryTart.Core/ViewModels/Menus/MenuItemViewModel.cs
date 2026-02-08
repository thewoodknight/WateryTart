using ReactiveUI;
using System.Windows.Input;
using Material.Icons;
using WateryTart.Core.Messages;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels.Menus;

public partial class MenuItemViewModel : ReactiveObject, ISmallViewModelBase, IMenuItemViewModel
{
    public bool Indented { get; }
    private readonly ICommand _clickedCommand;
    private readonly MaterialIconKind _icon;
    private readonly string _title; public MaterialIconKind Icon => _icon;
    public string Title => _title;

    public MenuItemViewModel(string title, MaterialIconKind? icon, ICommand clickedCommand, bool indented = false)
    {
        Indented = indented;
        _title = title;
        if (icon != null)
        _icon = (MaterialIconKind)icon;
        _clickedCommand = clickedCommand;
    }

    [GenerateTypedAction]
    public void MenuItemClicked()
    {
        _clickedCommand.Execute(null);
        MessageBus.Current.SendMessage(new CloseMenuMessage());
    }
}