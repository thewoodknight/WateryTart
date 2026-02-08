using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using WateryTart.Core.Extensions;
using WateryTart.Core.Settings;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;

namespace WateryTart.Core.ViewModels;

public partial class RecommendationViewModel : ReactiveObject, IViewModelBase
{
    private readonly IWsClient _massClient;
    private readonly ISettings _settings;
    public string? UrlPathSegment { get; } = "recommendation";
    public IScreen HostScreen { get; }
    [Reactive] public partial string? Title { get; set; }
    [Reactive] public partial Recommendation Recommendation { get; set; } = new Recommendation();
    public RelayCommand<Item> ClickedCommand { get; }
    public bool ShowMiniPlayer { get => true; }
    public bool ShowNavigation => true;

    [Reactive] public partial ObservableCollection<IViewModelBase> Items { get; set; } = new ObservableCollection<IViewModelBase>();

    public RecommendationViewModel(IScreen screen, IWsClient massClient, ISettings settings)
    {
        _massClient = massClient;
        _settings = settings;
        HostScreen = screen;

        ClickedCommand = new RelayCommand<Item>(item =>
        {
            var i = item; //navigate to whatever

            if (i?.ItemId == null || i.Provider == null)
                return;

            switch (i.MediaType)
            {
                case MediaType.Album:

                    var vm = App.Container.GetRequiredService<AlbumViewModel>();

                    vm.LoadFromId(i.ItemId, i.Provider);
                    HostScreen.Router.Navigate.Execute(vm);
                    break;

                case MediaType.Playlist:
                    var playlistViewModel = App.Container.GetRequiredService<PlaylistViewModel>();
                    playlistViewModel.LoadFromId(i.ItemId, i.Provider);
                    HostScreen.Router.Navigate.Execute(playlistViewModel);
                    break;

                case MediaType.Artist:
                    var artistViewModel = App.Container.GetRequiredService<ArtistViewModel>();
                    artistViewModel.LoadFromId(i.ItemId, i.Provider);
                    HostScreen.Router.Navigate.Execute(artistViewModel);
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
        Title = r.Name;
        Recommendation = r;

        var viewModels = new List<IViewModelBase>();
        if (r.Items != null)
            foreach (var item in r.Items)
            {
                IViewModelBase? viewModel = item.CreateViewModelForItem();
                if (viewModel != null)
                    viewModels.Add(viewModel);
            }

        Items = new ObservableCollection<IViewModelBase>(viewModels);
    }
}