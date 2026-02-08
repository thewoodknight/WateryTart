using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Models.Enums;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels
{
    public partial class ArtistViewModel : ReactiveObject, IViewModelBase
    {
        private readonly IMassWsClient _massClient;
        private readonly IPlayersService _playersService;
        [Reactive] public partial ObservableCollection<AlbumViewModel> Albums { get; set; } = new();
        public RelayCommand<Artist> AltMenuCommand { get; }
        [Reactive] public partial Artist? Artist { get; set; }
        public RelayCommand ArtistFullViewCommand { get; }
        public Image? ArtistLogo { get { return Artist?.Metadata?.Images?.FirstOrDefault(i => i.Type == ImageType.Logo); } }
        public Image? ArtistThumb { get { return Artist?.Metadata?.Images?.FirstOrDefault(i => i.Type == ImageType.Thumb); } }
        public IScreen HostScreen { get; }
        [Reactive] public partial bool IsBioExpanded { get; set; } = false;
        public RelayCommand PlayArtistRadioCommand { get; }
        public bool ShowMiniPlayer => true;
        public bool ShowNavigation => true;
        [Reactive] public partial string Title { get; set; }
        public RelayCommand ToggleBioCommand { get; }
        public ObservableCollection<Item>? Tracks { get; set; }
        public string? UrlPathSegment { get; } = "Artist/ID";

        public ArtistViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService, Artist? artist = null)
        {
            _massClient = massClient;
            _playersService = playersService;
            HostScreen = screen;
            Title = "";
            Artist = artist;

            ArtistFullViewCommand = new RelayCommand(() =>
            {
                if (Artist?.ItemId == null || Artist.Provider == null)
                    return;

                LoadFromId(Artist.ItemId, Artist.Provider);
                screen.Router.Navigate.Execute(this);
            });

            ToggleBioCommand = new RelayCommand(() =>
            {
                IsBioExpanded = !IsBioExpanded;
            });

            AltMenuCommand = new RelayCommand<Artist>(i =>
            {
                var menu = new MenuViewModel(
                [
                    new MenuItemViewModel("Artist Radio", MaterialIconKind.Radio, new RelayCommand(() => {})),
                    new MenuItemViewModel("Play", MaterialIconKind.Play, new RelayCommand(() => { }))
                ]);

                menu.AddMenuItem(MenuHelper.AddPlayers(_playersService, Artist));

                MessageBus.Current.SendMessage(menu);
            });

            PlayArtistRadioCommand = new RelayCommand(() =>
            {
                if (Artist != null)
                    _playersService.PlayArtistRadio(Artist);
            });
        }

        [GenerateTypedAction]
        public void AltMenuClicked()
        {
            if (Artist == null)
                return;

            var menu = new MenuViewModel(
            [
            new MenuItemViewModel("Artist Radio", MaterialIconKind.Radio, new RelayCommand(() => {})),
                new MenuItemViewModel("Play", MaterialIconKind.Play, new RelayCommand(() => { }))
            ]);


            menu.AddMenuItem(MenuHelper.AddPlayers(_playersService, Artist));

            MessageBus.Current.SendMessage(menu);
        }

        [GenerateTypedAction]
        public void ArtistFullViewClicked()
        {
            if (Artist?.ItemId == null || Artist.Provider == null)
                return;

            LoadFromId(Artist.ItemId, Artist.Provider);
            HostScreen.Router.Navigate.Execute(this);
        }

        public void LoadFromId(string id, string provider)
        {
            Tracks = [];

            _ = LoadArtistAlbum(id, provider);
            _ = LoadArtist(id, provider);
        }

        [GenerateTypedAction]
        public void ToggleBioClicked()
        {
            IsBioExpanded = !IsBioExpanded;
        }

        private async Task LoadArtist(string id, string provider)
        {
            var artistResponse = await _massClient.ArtistGetAsync(id, provider);

            Artist = artistResponse.Result;
            if (Artist?.Name != null)
                Title = Artist.Name;
            this.RaisePropertyChanged(nameof(ArtistLogo));
            this.RaisePropertyChanged(nameof(ArtistThumb));
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