using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Reactive;
using WateryTart.MassClient;
using WateryTart.MassClient.Models;
using WateryTart.Services;

namespace WateryTart.ViewModels;

public partial class ArtistsViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;

    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial ObservableCollection<Artist> Artists { get; set; } = new();
    public ReactiveCommand<Artist, Unit> ClickedCommand { get; }
    public bool ShowMiniPlayer { get => true; }
    public ArtistsViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Title = "Artists";

        massClient.ArtistsGet((response) =>

        {
            foreach (var a in response.Result)
                Artists.Add(a);
        });

        ClickedCommand = ReactiveCommand.Create<Artist>(item =>
        {
            var artistViewModel = WateryTart.App.Container.GetRequiredService<ArtistViewModel>();
            artistViewModel.LoadFromId(item.ItemId, item.Provider);
            screen.Router.Navigate.Execute(artistViewModel);
        });
    }
}