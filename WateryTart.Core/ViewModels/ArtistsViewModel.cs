using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public partial class ArtistsViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;

    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial ObservableCollection<ArtistViewModel> Artists { get; set; } = new();
    public ReactiveCommand<ArtistViewModel, Unit> ClickedCommand { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    public ArtistsViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Title = "Artists";

        ClickedCommand = ReactiveCommand.Create<ArtistViewModel>(item =>
        {
            //var artistViewModel = App.Container.GetRequiredService<ArtistViewModel>();
            //artistViewModel.LoadFromId(item.ItemId, item.Provider);
            screen.Router.Navigate.Execute(item);
        });

        Load();
    }

    private async Task Load()
    {
        var response = await _massClient.ArtistsGetAsync();

        foreach (var a in response.Result)
        {
            Artists.Add(new ArtistViewModel(_massClient, HostScreen, _playersService,a ));
        }
    }
}