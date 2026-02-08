using WateryTart.Core.ViewModels;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;

namespace WateryTart.Core.Extensions;

public static class ItemExtensions
{
    extension(Item item)
    {
        public IViewModelBase? CreateViewModelForItem()
        {
            return item.MediaType switch
            {
                MediaType.Album => CreateAlbumViewModel(item),
                MediaType.Playlist => CreatePlaylistViewModel(item),
                MediaType.Artist => CreateArtistViewModel(item),
                MediaType.Track => CreateTrackViewModel(item),
                MediaType.Genre => null,
                MediaType.Radio => null,
                MediaType.Audiobook => null,
                MediaType.Folder => null,
                MediaType.Podcast => null,
                MediaType.PodcastEpisode => null,
                _ => null
            };
        }
    }

    private static TrackViewModel CreateTrackViewModel(Item item)
    {
        var vm = App.Container.GetRequiredService<TrackViewModel>();
        vm.Track = item;
        //  vm.LoadFromId(item.ItemId, item.Provider);
        return vm;
    }

    private static AlbumViewModel CreateAlbumViewModel(Item item)
    {
        var vm = App.Container.GetRequiredService<AlbumViewModel>();
        vm.Album = item.Album;
        vm.LoadFromId(item.ItemId, item.GetProviderInstance());
        return vm;
    }

    private static PlaylistViewModel CreatePlaylistViewModel(Item item)
    {
        var vm = App.Container.GetRequiredService<PlaylistViewModel>();
        vm.LoadFromId(item.ItemId, item.GetProviderInstance());
        return vm;
    }

    private static ArtistViewModel CreateArtistViewModel(Item item)
    {
        var vm = App.Container.GetRequiredService<ArtistViewModel>();
        vm.LoadFromId(item.ItemId, item.GetProviderInstance());
        return vm;
    }
}