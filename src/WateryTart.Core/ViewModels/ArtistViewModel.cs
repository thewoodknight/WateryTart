using Autofac;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Material;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WateryTart.Core.Extensions;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using WateryTart.MusicAssistant.WsExtensions;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels
{
    public partial class ArtistViewModel : ViewModelBase<ArtistViewModel>
    {
        private readonly ProviderService _providerservice;
        [Reactive] public partial ObservableCollection<AlbumViewModel> Albums { get; set; } = new();
        public RelayCommand<Artist> AltMenuCommand { get; }
        [Reactive] public partial Artist? Artist { get; set; }
        public RelayCommand ArtistFullViewCommand { get; }
        public Image? ArtistLogo { get { return Artist?.Metadata?.Images?.FirstOrDefault(i => i.Type == ImageType.Logo); } }
        public Image? ArtistThumb { get { return Artist?.Metadata?.Images?.FirstOrDefault(i => i.Type == ImageType.Thumb); } }
        [Reactive] public partial string InputProviderIcon { get; set; } = App.BlankSvg;
        public RelayCommand PlayArtistCommand { get; }
        public RelayCommand PlayArtistRadioCommand { get; }
        [Reactive] public partial ProviderManifest? Provider { get; set; } = null;
        public ICommand ToggleFavoriteCommand { get; set; }
        public ObservableCollection<Item>? Tracks { get; set; }

        public new string Title { get; set; } = string.Empty;
        public ArtistViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, Artist? artist = null)
            : base(null, massClient, playersService)
        {
            _providerservice = App.Container.Resolve<ProviderService>();
            HostScreen = screen;
            Artist = artist;

            ToggleFavoriteCommand = new RelayCommand(() =>
            {
                if (!Artist!.Favorite)
                    _ = _client.WithWs().AddFavoriteItemAsync(Artist);
                else
                    _ = _client.WithWs().RemoveFavoriteItemAsync(Artist);

                Artist.Favorite = !Artist.Favorite;
            });

            ArtistFullViewCommand = new RelayCommand(() =>
            {
                if (Artist?.ItemId == null || Artist.Provider == null)
                    return;

                LoadFromId(Artist.ItemId, Artist.Provider);
                screen.Router.Navigate.Execute(this);
            });

            AltMenuCommand = new RelayCommand<Artist>(i =>
            {
                var menu = new MenuViewModel(
                [
                    new MenuItemViewModel("Artist Radio", PackIconMaterialKind.RadioTower, new RelayCommand(() => {})),
                    new MenuItemViewModel("Play", PackIconMaterialKind.Play, new RelayCommand(() => { }))
                ]);

                if (_playersService != null && Artist != null)
                    menu.AddMenuItem(MenuHelper.AddPlayers(_playersService, Artist));

                MessageBus.Current.SendMessage<IPopupViewModel>(menu);
            });

            PlayArtistRadioCommand = new RelayCommand(() =>
            {
                if (Artist != null)
                    _ = _playersService?.PlayArtistRadio(Artist);
            });

            PlayArtistCommand = new RelayCommand(() =>
            {
                if (Artist != null)
                    _ = _playersService?.PlayItem(Artist);
            });
        }

        [GenerateTypedAction]
        public void AltMenuClicked()
        {
            var menu = new MenuViewModel(
            [
                new MenuItemViewModel("Artist Radio", PackIconMaterialKind.RadioTower, new RelayCommand(() => {})),
                new MenuItemViewModel("Play", PackIconMaterialKind.Play, new RelayCommand(() => { }))
            ]);

            if (Artist != null && _playersService != null)
                menu.AddMenuItem(MenuHelper.AddPlayers(_playersService, Artist));

            MessageBus.Current.SendMessage<IPopupViewModel>(menu);
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

        private async Task LoadArtist(string id, string provider)
        {
            var artistResponse = await _client.WithWs().GetArtistAsync(id, provider);

            Artist = artistResponse.Result;
            if (Artist?.Name != null)
                Title = Artist.Name;
            this.RaisePropertyChanged(nameof(ArtistLogo));
            this.RaisePropertyChanged(nameof(ArtistThumb));
        }

        private async Task LoadArtistAlbum(string id, string provider)
        {
            Albums.Clear();
            var albumArtistResponse = await _client.WithWs().GetArtistAlbumsAsync(id, provider);
            if (albumArtistResponse.Result != null)
                foreach (var r in albumArtistResponse.Result.OrderByDescending(a => a.Year ?? 0).ThenBy(a => a.Name))
                    Albums.Add(new AlbumViewModel(_client, HostScreen, _playersService!, r));

            var domain = Artist?.ProviderMappings?.FirstOrDefault()?.ProviderDomain;

            if (!string.IsNullOrEmpty(domain))
                Provider = _providerservice.GetProvider(domain);

            if (Provider != null)
                if (!string.IsNullOrEmpty(Provider.IconSvgDark))
                    InputProviderIcon = Provider.IconSvgDark;
                else InputProviderIcon = Provider.IconSvg;
        }
    }
}