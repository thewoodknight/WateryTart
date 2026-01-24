using ReactiveUI;
using System.Reactive;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using WateryTart.MassClient;
using WateryTart.Services;
using WateryTart.Settings;

namespace WateryTart.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    private readonly IMassWSClient _massClient;
    private readonly IPlayersService _playersService;
    private readonly ISettings _settings;

    public RoutingState Router { get; } = new();

    public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

    public MainWindowViewModel(IMassWSClient massClient, IPlayersService playersService, ISettings settings)
    {
        _massClient = massClient;
        _playersService = playersService;
        _settings = settings;


        
        //Need to summon this from IOC
        GoNext = ReactiveCommand.CreateFromObservable(
            () => 

                Router.Navigate.Execute(App.Container.GetRequiredService<AlbumsListViewModel>())
        );
    }

    public async void Connect()
    {
        if (string.IsNullOrEmpty(_settings.Credentials.Token))
            Router.Navigate.Execute(App.Container.GetRequiredService<LoginViewModel>());


        _massClient.Connect(_settings.Credentials);

        while (_massClient.IsConnected == false)
          Thread.Sleep(1000);

        _playersService.GetPlayers();
    }
}