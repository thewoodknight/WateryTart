using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WateryTart.Core.Messages;
using WateryTart.Core.Playback;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Players;
using WateryTart.Service.MassClient;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels;

public partial class MainWindowViewModel : ReactiveObject, IScreen, IActivatableViewModel
{
    private readonly IMassWsClient _massClient;
    private readonly SendSpinClient _sendSpinClient;
    private readonly ISettings _settings;
    public ViewModelActivator Activator { get; }
    [Reactive] public partial ReactiveCommand<Unit, Unit> CloseSlideupCommand { get; set; }
    public IColourService ColourService { get; }
    [Reactive] public partial IViewModelBase CurrentViewModel { get; set; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;
    public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoMusic { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoPlayers { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoSearch { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
    [Reactive] public partial bool IsMiniPlayerVisible { get; set; }
    [Reactive] public partial MiniPlayerViewModel MiniPlayer { get; set; }
    [Reactive] public partial IPlayersService PlayersService { get; set; }
    public RoutingState Router { get; } = new();
    public ISettings Settings => _settings;
    [Reactive] public partial bool ShowSlideupMenu { get; set; }
    [Reactive] public partial ReactiveObject SlideupMenu { get; set; }
    [Reactive] public partial string Title { get; set; }

    public MainWindowViewModel(IMassWsClient massClient, IPlayersService playersService, ISettings settings, IColourService colourService, SendSpinClient sendSpinClient)
    {
        _massClient = massClient;
        PlayersService = playersService;
        _settings = settings;
        _sendSpinClient = sendSpinClient;
        ColourService = colourService;
        ShowSlideupMenu = false;

        // Create observables that check if we're already on the target page
        var canNavigateToHome = Router.CurrentViewModel
            .Select(vm => vm is not HomeViewModel);

        var canNavigateToMusic = Router.CurrentViewModel
            .Select(vm => vm is not LibraryViewModel);

        var canNavigateToSearch = Router.CurrentViewModel
            .Select(vm => vm is not SearchViewModel);

        var canNavigateToSettings = Router.CurrentViewModel
            .Select(vm => vm is not SettingsViewModel);

        var canNavigateToPlayers = Router.CurrentViewModel
            .Select(vm => vm is not PlayersViewModel);

        GoHome = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<HomeViewModel>()), canNavigateToHome);

        GoMusic = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<LibraryViewModel>()), canNavigateToMusic);

        GoSearch = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<SearchViewModel>()), canNavigateToSearch);

        GoSettings = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<SettingsViewModel>()), canNavigateToSettings);

        GoPlayers = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<PlayersViewModel>()), canNavigateToPlayers);

        Router.CurrentViewModel.Subscribe(vm =>
        {
            if (vm is IViewModelBase ivmb)
            {
                ivmb.WhenAnyValue(x => x.Title)
                    .BindTo(this, v => v.Title);

                CurrentViewModel = ivmb;
                MiniPlayer = App.Container.GetRequiredService<MiniPlayerViewModel>();
            }
        });

        // Subscribe to changes in CurrentViewModel and SelectedPlayer to update IsMiniPlayerVisible
        this.WhenAnyValue(
            x => x.CurrentViewModel.ShowMiniPlayer,
            x => x.PlayersService.SelectedPlayer,
            (showMiniPlayer, selectedPlayer) => showMiniPlayer && selectedPlayer != null)
            .Subscribe(isVisible => IsMiniPlayerVisible = isVisible);

        MessageBus.Current.Listen<FromLoginMessage>().Subscribe(x => { Connect(); });

        MessageBus.Current.Listen<MenuViewModel>()
            .Subscribe(
                x =>
                {
                    SlideupMenu = x;
                    ShowSlideupMenu = true;
                }
            );

        MessageBus.Current.Listen<CloseMenuMessage>()
            .Subscribe(
                x =>
                {
                    ShowSlideupMenu = false;
                    SlideupMenu = null; //
                });
    }

    [GenerateTypedAction]
    public void CloseMenuClicked()
    {
        Console.WriteLine("The menu should close");
        ShowSlideupMenu = false;
    }

    public async Task Connect()
    {
        if (string.IsNullOrEmpty(_settings.Credentials?.Token))
        {
            Router.Navigate.Execute(App.Container.GetRequiredService<LoginViewModel>());
            return;
        }

        var connected = await _massClient.Connect(_settings.Credentials);

        if (!connected)
        {
            return;
        }

        await PlayersService.GetPlayers();

        GoHome.Execute();

        _sendSpinClient.ConnectAsync(_settings.Credentials.BaseUrl);
    }
}