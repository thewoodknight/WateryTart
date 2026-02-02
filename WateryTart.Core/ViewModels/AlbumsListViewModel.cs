using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public partial class AlbumsListViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    public string? UrlPathSegment { get; } = "AlbumsList";
    public IScreen HostScreen { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }

    public ObservableCollection<AlbumViewModel> Albums { get; set; }
    public AlbumViewModel SelectedAlbum { get; set; }
    public ReactiveCommand<Unit, IRoutableViewModel> SelectedItemChangedCommand { get; }
    [Reactive] public partial string Title { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public AlbumsListViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Albums = new ObservableCollection<AlbumViewModel>();

        //I don't love this, there might be a better way to pass a parameter
        GoNext = ReactiveCommand.CreateFromObservable(() =>
            {
                var e = screen.Router.Navigate.Execute(SelectedAlbum);
                SelectedAlbum.Load(SelectedAlbum.Album);
                SelectedAlbum = null;
                return e;
            }
        );

        Load();
    }

    public async Task Load()
    {
        Albums.Clear();
        var response = await _massClient.MusicAlbumsLibraryItemsAsync();
        foreach (var a in response.Result)
        {
            Albums.Add(new AlbumViewModel(_massClient, HostScreen, _playersService, a));
        }
           
    }
}