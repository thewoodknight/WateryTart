using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WateryTart.MassClient;
using WateryTart.Messages;
using WateryTart.Services;
using WateryTart.Settings;
using WateryTart.ViewModels.Menus;
using WateryTart.ViewModels.Players;

namespace WateryTart.ViewModels;

public partial class MainWindowViewModel : ReactiveObject, IScreen
{
    private readonly IMassWsClient _massClient;
    private readonly ISettings _settings;
    public ISettings Settings { get { return _settings;  } }
    public RoutingState Router { get; } = new();
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;
    public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoMusic { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoSearch { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoPlayers { get; }

    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial IPlayersService PlayersService { get; set; }

    [Reactive] public partial ReactiveObject SlideupMenu { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> CloseSlideupCommand { get; set; }

    [Reactive] public partial bool ShowSlideupMenu { get; set; }

    [Reactive] public partial MiniPlayerViewModel MiniPlayer { get; set; }

    [Reactive]
    public partial IViewModelBase CurrentViewModel { get; set; }

    public MainWindowViewModel(IMassWsClient massClient, IPlayersService playersService, ISettings settings)
    {
        _massClient = massClient;
        PlayersService = playersService;
        _settings = settings;
        ShowSlideupMenu = false;

        GoHome = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<HomeViewModel>()));
        GoMusic = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<LibraryViewModel>()));
        GoSearch = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<SearchViewModel>()));
        GoSettings = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<SettingsViewModel>()));
        GoPlayers = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<PlayersViewModel>()));
        CloseSlideupCommand = ReactiveCommand.Create<Unit>(_ => ShowSlideupMenu = false);

        Router.CurrentViewModel.Subscribe((vm) =>
        {
            if (vm is IViewModelBase ivmb)
            {
                ivmb.WhenAnyValue(x => x.Title)
                    .BindTo(this, v => v.Title);

                CurrentViewModel = ivmb;

                MiniPlayer = App.Container.GetRequiredService<MiniPlayerViewModel>();
            }
        });

        MessageBus.Current.Listen<MenuViewModel>()
            .Subscribe(x =>
            {
                SlideupMenu = x;
                ShowSlideupMenu = true;
            });

        MessageBus.Current.Listen<CloseMenuMessage>()
            .Subscribe(x =>
            {
                ShowSlideupMenu = false;
            });
    }

    public async Task Connect()
    {
        /* This shouldn't be set too early otherwise the UI hangs
            Needs to be tied to a message to open/close the menu
         */
        SlideupMenu = App.Container.GetRequiredService<SearchViewModel>();

        if (string.IsNullOrEmpty(_settings.Credentials.Token))
        {
            Router.Navigate.Execute(App.Container.GetRequiredService<LoginViewModel>());
            return;
        }

        _massClient.Connect(_settings.Credentials);

        while (_massClient.IsConnected == false)
            await Task.Delay(1000);

        PlayersService.GetPlayers();

        GoHome.Execute();
    }
}