using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels.Menus;

public static class MenuHelper
{
    public static IEnumerable<MenuItemViewModel> AddPlayers(IPlayersService playersService, MediaItemBase item, ReactiveCommand<Unit, Unit> playCommand = null)
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

            var playerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                Debug.WriteLine($"Playing on {displayName}");
                playersService.SelectedPlayer = capturedPlayer;
                await playersService.PlayItem(capturedItem, capturedPlayer);
            });

            players.Add(new MenuItemViewModel($"\tPlay on {displayName}", string.Empty, playerCommand));
        }


        return players;
    }

    public static MenuViewModel BuildStandardPopup(IPlayersService playersService, MediaItemBase item, bool addPlayers = true)
    {
        if (item == null)
        {
            Debug.WriteLine("BuildStandardPopup: item is null");
            return new MenuViewModel([]);
        }

        if (playersService == null)
        {
            Debug.WriteLine("BuildStandardPopup: playersService is null");
            return new MenuViewModel([]);
        }

        // ✅ Create commands with proper error handling
        var playCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                Debug.WriteLine($"Playing item: {item.Name}");
                await playersService.PlayItem(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error playing item: {ex.Message}");
            }
        });

        // ✅ CRITICAL: Subscribe to command errors
        playCommand.ThrownExceptions.Subscribe(ex =>
        {
            Debug.WriteLine($"❌ Play command error: {ex.Message}");
        });

        var addToLibraryCommand = ReactiveCommand.Create(() =>
            Debug.WriteLine("Add to library clicked"));
        addToLibraryCommand.ThrownExceptions.Subscribe(ex =>
            Debug.WriteLine($"❌ Add to library error: {ex.Message}"));

        var addToFavouritesCommand = ReactiveCommand.Create(() =>
            Debug.WriteLine("Add to favourites clicked"));
        addToFavouritesCommand.ThrownExceptions.Subscribe(ex =>
            Debug.WriteLine($"❌ Add to favourites error: {ex.Message}"));

        var addToPlaylistCommand = ReactiveCommand.Create(() =>
            Debug.WriteLine("Add to playlist clicked"));
        addToPlaylistCommand.ThrownExceptions.Subscribe(ex =>
            Debug.WriteLine($"❌ Add to playlist error: {ex.Message}"));

        var menu = new MenuViewModel(
        [
            new MenuItemViewModel("Add to library", string.Empty, addToLibraryCommand),
            new MenuItemViewModel("Add to favourites", string.Empty, addToFavouritesCommand),
            new MenuItemViewModel("Add to playlist", string.Empty, addToPlaylistCommand),
            new MenuItemViewModel("Play", string.Empty, playCommand)
        ]);

        if (addPlayers)
        {
            var playerMenuItems = AddPlayers(playersService, item);
            menu.AddMenuItem(playerMenuItems);
        }

        return menu;
    }
}