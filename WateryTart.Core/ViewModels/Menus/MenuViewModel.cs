using Material.Icons;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI.SourceGenerators;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.ViewModels.Menus;

public partial class MenuViewModel : ReactiveObject, ISmallViewModelBase
{
    [Reactive] public partial object HeaderItem { get; set; }
    public List<IMenuItemViewModel> MenuItems { get; set; } = new List<IMenuItemViewModel>();
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    public string Title { get; set; } = string.Empty;
    public MaterialIconKind Icon { get; } = MaterialIconKind.Sword;
    public string? UrlPathSegment { get; } = string.Empty;

    public MenuViewModel(IEnumerable<IMenuItemViewModel>? menuItems = null, object headerItem = null)
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