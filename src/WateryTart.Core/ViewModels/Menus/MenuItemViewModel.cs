using ReactiveUI;
using System.Windows.Input;
using IconPacks.Avalonia.Material;
using WateryTart.Core.Messages;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels.Menus;

public partial class MenuItemViewModel : ReactiveObject, ISmallViewModelBase, IMenuItemViewModel
{
    public bool Indented { get; }
    private readonly ICommand? _clickedCommand = null;
    private readonly PackIconMaterialKind _icon;
    private readonly string _title; public PackIconMaterialKind Icon => _icon;
    public string Title => _title;

    public MenuItemViewModel(string title, PackIconMaterialKind? icon, ICommand? clickedCommand, bool? indented = false)
    {
        Indented = indented ?? false;

        _title = title;
        if (icon != null)
            _icon = (PackIconMaterialKind)icon;

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