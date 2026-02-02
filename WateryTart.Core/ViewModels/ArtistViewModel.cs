using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

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
        public Image ArtistLogo { get { return Artist?.Metadata?.images?.FirstOrDefault(i => i.type == "logo"); } }

        public Image ArtistThumb { get { return Artist?.Metadata?.images?.FirstOrDefault(i => i.type == "thumb"); } }

        public ReactiveCommand<Artist, Unit> AltMenuCommand { get; }
        public ReactiveCommand<Unit, Unit> ArtistFullViewCommand { get; }
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
            foreach (var r in albumArtistResponse.Result.OrderByDescending(a => a.Year).ThenBy(a => a.Name))
                Albums.Add(new AlbumViewModel(_massClient, HostScreen, _playersService, r));
        }
    }
}