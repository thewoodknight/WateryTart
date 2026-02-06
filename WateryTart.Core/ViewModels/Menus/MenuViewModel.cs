using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient;

namespace WateryTart.Core.ViewModels.Menus;

public partial class MenuViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    private readonly IScreen _screen;
    public IScreen HostScreen { get; }
    public List<MenuItemViewModel> MenuItems { get; set; } = new List<MenuItemViewModel>();
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    public string Title { get; set; }
    public string? UrlPathSegment { get; }

    public MenuViewModel(IEnumerable<MenuItemViewModel> menuItems = null)
    {
        MenuItems = menuItems?.ToList() ?? [];
    }

    public void AddMenuItem(MenuItemViewModel menuItem)
    {
        MenuItems.Add(menuItem);
    }

    public void AddMenuItem(IEnumerable<MenuItemViewModel> menuItems)
    {
        foreach (var item in menuItems)
        {
            if (item != null)
                MenuItems.Add(item);
        }
    }
}