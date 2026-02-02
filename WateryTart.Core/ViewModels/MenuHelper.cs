using Avalonia.Controls;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public static class MenuHelper
{
    public static MenuViewModel BuildStandardPopup(IPlayersService _playersService, MediaItemBase item, bool AddPlayers = true)
    {
        var menu = new MenuViewModel(
        [
            new MenuItemViewModel("Add to library", string.Empty, ReactiveCommand.Create<Unit>(r => {})),
            new MenuItemViewModel("Add to favourites", string.Empty, ReactiveCommand.Create<Unit>(r => { })),
            new MenuItemViewModel("Add to playlist", string.Empty, ReactiveCommand.Create<Unit>(r => { })),
            new MenuItemViewModel("Play", string.Empty, ReactiveCommand.Create<Unit>(r => { }))
        ]);

        if (AddPlayers)
            menu.AddMenuItem(MenuHelper.AddPlayers(_playersService, item));

        return menu;
    }

    public static IEnumerable<MenuItemViewModel> AddPlayers(IPlayersService _playersService, MediaItemBase item)
    {
        List<MenuItemViewModel> players = new List<MenuItemViewModel>();
        foreach (var p in _playersService.Players)
        {
            players.Add(new Menus.MenuItemViewModel($"\tPlay on {p.DisplayName}", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {
                _playersService.PlayItem(item, p);
            })));
        }

        return players;
    }
}