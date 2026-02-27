using IconPacks.Avalonia.Material;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI.SourceGenerators;
using WateryTart.MusicAssistant.Models;
using WateryTart.Core.ViewModels.Popups;

namespace WateryTart.Core.ViewModels.Menus;

public partial class MenuViewModel : ReactiveObject, IPopupViewModel
{
    [Reactive] public partial object? HeaderItem { get; set; }
    public List<IMenuItemViewModel> MenuItems { get; set; } = new List<IMenuItemViewModel>();
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    public string Title { get; set; } = string.Empty;
    public PackIconMaterialKind Icon { get; } = PackIconMaterialKind.Sword;
    public string? UrlPathSegment { get; } = string.Empty;

    public string Message => throw new System.NotImplementedException();

    public MenuViewModel(IEnumerable<IMenuItemViewModel>? menuItems = null, object? headerItem = null)
    {
        HeaderItem = headerItem;
        MenuItems = menuItems?.ToList() ?? [];
    }

    public void AddMenuItem(IMenuItemViewModel menuItem)
    {
        MenuItems.Add(menuItem);
    }

    public void AddMenuItem(IEnumerable<IMenuItemViewModel?> menuItems)
    {
        foreach (var item in menuItems)
        {
            if (item != null)
                MenuItems.Add(item);
        }
    }
}