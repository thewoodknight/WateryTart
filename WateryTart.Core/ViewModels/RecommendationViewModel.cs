using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using WateryTart.Core.Settings;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public partial class RecommendationViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly ISettings _settings;
    private readonly IScreen _screen;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial Recommendation Recommendation { get; set; }
    public ReactiveCommand<Item, Unit> ClickedCommand { get; }
    public bool ShowMiniPlayer { get => true; }
    public bool ShowNavigation => true;
    public RecommendationViewModel(IScreen screen, IMassWsClient massClient, ISettings settings)
    {
        _massClient = massClient;
        _settings = settings;
        _screen = screen;

        ClickedCommand = ReactiveCommand.Create<Item>(item =>
        {
            var i = item; //navigate to whatever

            switch (i.MediaType)
            {
                case MediaType.Album:

                    var vm = App.Container.GetRequiredService<AlbumViewModel>();

                    vm.LoadFromId(item.ItemId, item.Provider);
                    screen.Router.Navigate.Execute(vm);
                    break;

                case MediaType.Playlist:
                    var playlistViewModel = App.Container.GetRequiredService<PlaylistViewModel>();
                    playlistViewModel.LoadFromId(item.ItemId, item.Provider);
                    screen.Router.Navigate.Execute(playlistViewModel);
                    break;

                case MediaType.Artist:
                    var artistViewModel = App.Container.GetRequiredService<ArtistViewModel>();
                    artistViewModel.LoadFromId(item.ItemId, item.Provider);
                    screen.Router.Navigate.Execute(artistViewModel);
                    break;

                case MediaType.Genre:
                    break;

                case MediaType.Radio: break;
                case MediaType.Track: break;

                case MediaType.Audiobook: break;
                case MediaType.Folder: break;
                case MediaType.Podcast: break;
                case MediaType.PodcastEpisode: break;
            }
        });
    }

    public void SetRecommendation(Recommendation r)
    {
        if (r == null)
            return;

        Title = r.Name;
        Recommendation = r;
    }
}