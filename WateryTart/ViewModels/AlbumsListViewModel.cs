using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Reactive;
using WateryTart.MassClient;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;

namespace WateryTart.ViewModels;

public partial class AlbumsListViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    public string? UrlPathSegment { get; } = "AlbumsList";
    public IScreen HostScreen { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }

    public ObservableCollection<Album> Albums { get; set; }
    public Album SelectedAlbum { get; set; }
    public ReactiveCommand<Unit, IRoutableViewModel> SelectedItemChangedCommand { get; }
    [Reactive] public partial string Title { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public AlbumsListViewModel(IMassWsClient massClient, IScreen screen)
    {
        _massClient = massClient;
        HostScreen = screen;
        Albums = new ObservableCollection<Album>();

        //I don't love this, there might be a better way to pass a parameter
        GoNext = ReactiveCommand.CreateFromObservable(() =>
            {
                var vm = WateryTart.App.Container.GetRequiredService<AlbumViewModel>();
                vm.Load(SelectedAlbum);
                var e = screen.Router.Navigate.Execute(vm);
                SelectedAlbum = null;
                return e;
            }
        );

        _massClient.MusicAlbumsLibraryItems(AlbumListHandler);
    }

    public void AlbumListHandler(AlbumsResponse response)
    {
        foreach (var a in response.Result)
            Albums.Add(a);
    }
}