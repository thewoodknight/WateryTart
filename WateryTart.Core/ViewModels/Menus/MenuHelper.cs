using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using WateryTart.Core.Services;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.ViewModels.Menus;

public static class MenuHelper
{
    public static IEnumerable<MenuItemViewModel> AddPlayers(IPlayersService playersService, MediaItemBase item)
    {
        var players = new List<MenuItemViewModel>();

        if (playersService?.Players == null)
            return players;


        var playersList = playersService.Players.ToList();
        if (playersList.Count == 0)
            return players;


        foreach (var p in playersList)
        {
            if (p == null)
            {
                continue;
            }

            var capturedPlayer = p; // Capture for closure
            var capturedItem = item; // Capture item too
            var displayName = capturedPlayer.DisplayName ?? "Unknown Player";

            ICommand playerCommand = new AsyncRelayCommand(async () =>
            {
                App.Logger?.LogInformation("Playing on {DisplayName}", displayName);
                playersService.SelectedPlayer = capturedPlayer;
                await playersService.PlayItem(capturedItem, capturedPlayer);

            });

            players.Add(new MenuItemViewModel($"Play on {displayName}", MaterialIconKind.Speaker, playerCommand, true));
        }

        return players;
    }

    public static MenuViewModel BuildStandardPopup(IPlayersService playersService, MediaItemBase item, bool addPlayers = true)
    {
        if (item == null)
        {
            App.Logger?.LogWarning("BuildStandardPopup: item is null");
            return new MenuViewModel([]);
        }

        if (playersService == null)
        {
            App.Logger?.LogWarning("BuildStandardPopup: playersService is null");
            return new MenuViewModel([]);
        }


        ICommand playCommand = new AsyncRelayCommand(async () =>
        {
            await playersService.PlayItem(item);

        });

        var addToLibraryCommand = new RelayCommand(() => App.Logger?.LogDebug("Add to library clicked"));
        var addToFavouritesCommand = new RelayCommand(() => App.Logger?.LogDebug("Add to favourites clicked"));
        var addToPlaylistCommand = new RelayCommand(() => App.Logger?.LogDebug("Add to playlist clicked"));

        var menu = new MenuViewModel(
        [
            new MenuItemViewModel("Add to library", MaterialIconKind.LibraryAdd, addToLibraryCommand),
            new MenuItemViewModel("Add to favourites", MaterialIconKind.FavoriteAdd, addToFavouritesCommand),
            new MenuItemViewModel("Add to playlist",MaterialIconKind.PlaylistAdd, addToPlaylistCommand),
            new MenuItemViewModel("Play", MaterialIconKind.Play, playCommand)
        ]);

        if (addPlayers)
        {
            var playerMenuItems = AddPlayers(playersService, item);
            menu.AddMenuItem(playerMenuItems);
        }

        return menu;
    }
}