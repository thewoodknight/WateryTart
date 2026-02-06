using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Models.Enums;

namespace WateryTart.Core.ViewModels
{
    public partial class ArtistViewModel : ReactiveObject, IViewModelBase
    {
        public string? UrlPathSegment { get; }
        public IScreen HostScreen { get; }
        private readonly IMassWsClient _massClient;
        private readonly IPlayersService _playersService;
        public bool ShowMiniPlayer => true;
        public bool ShowNavigation => true;
        [Reactive] public partial string Title { get; set; }
        [Reactive] public partial Artist Artist { get; set; }
        [Reactive] public partial ObservableCollection<AlbumViewModel> Albums { get; set; } = new();
        [Reactive] public partial bool IsBioExpanded { get; set; } = false;
        public Image ArtistLogo { get { return Artist?.Metadata?.Images?.FirstOrDefault(i => i.type == ImageType.Logo); } }

        public Image ArtistThumb { get { return Artist?.Metadata?.Images?.FirstOrDefault(i => i.type == ImageType.Thumb); } }

        public ReactiveCommand<Artist, Unit> AltMenuCommand { get; }
        public ReactiveCommand<Unit, Unit> ArtistFullViewCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleBioCommand { get; }

        public ReactiveCommand<Unit, Unit> PlayArtistRadioCommand { get; }
        public ObservableCollection<Item> Tracks { get; set; }

        public ArtistViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService, Artist artist = null)
        {
            _massClient = massClient;
            _playersService = playersService;
            HostScreen = screen;
            Title = "";
            Artist = artist;

            ArtistFullViewCommand = ReactiveCommand.Create(() =>
            {
                LoadFromId(Artist.ItemId, Artist.Provider);
                screen.Router.Navigate.Execute(this);
            });

            ToggleBioCommand = ReactiveCommand.Create(() =>
            {
                IsBioExpanded = !IsBioExpanded;
            });

            AltMenuCommand = ReactiveCommand.Create<Artist>(i =>
            {
                var menu = new MenuViewModel(
                [
                    new MenuItemViewModel("Artist Radio", string.Empty, ReactiveCommand.Create<Unit>(r => {})),
                    new MenuItemViewModel("Play", string.Empty, ReactiveCommand.Create<Unit>(r => { }))
                ]);

                menu.AddMenuItem(MenuHelper.AddPlayers(_playersService, artist));

                MessageBus.Current.SendMessage(menu);
            });

            PlayArtistRadioCommand = ReactiveCommand.Create(() =>
            {
                _playersService.PlayArtistRadio(Artist);
            });
        }

        public void LoadFromId(string id, string provider)
        {
            Tracks = new ObservableCollection<Item>();

            LoadArtistAlbum(id, provider);
            LoadArtist(id, provider);
        }

        private async Task LoadArtist(string id, string provider)
        {
            var artistResponse = await _massClient.ArtistGetAsync(id, provider);

            Artist = artistResponse.Result;
            Title = Artist.Name;
            this.RaisePropertyChanged("ArtistLogo");
            this.RaisePropertyChanged("ArtistThumb");
        }

        private async Task LoadArtistAlbum(string id, string provider)
        {
            Albums.Clear();
            var albumArtistResponse = await _massClient.ArtistAlbumsAsync(id, provider);
            if (albumArtistResponse.Result != null)
                foreach (var r in albumArtistResponse.Result.OrderByDescending(a => a.Year ?? 0).ThenBy(a => a.Name))
                    Albums.Add(new AlbumViewModel(_massClient, HostScreen, _playersService, r));
        }
    }
}