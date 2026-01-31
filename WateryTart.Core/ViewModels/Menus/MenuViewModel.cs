using ReactiveUI;
using System.Collections.Generic;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient;

namespace WateryTart.Core.ViewModels.Menus;

public partial class MenuViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IScreen _screen;
    private readonly IPlayersService _playersService;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; set; }
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    public List<MenuItemViewModel> MenuItems { get; set; }

    public MenuViewModel(IEnumerable<MenuItemViewModel> menuItems = null)
    {
        MenuItems = [];
        if (menuItems != null)
            MenuItems.AddRange(menuItems);
    }

    public void AddMenuItem(MenuItemViewModel menuItem)
    {
        MenuItems.Add(menuItem);
    }

    public void AddMenuItem(IEnumerable<MenuItemViewModel> menuItems)
    {
        MenuItems.AddRange(menuItems);
    }
}