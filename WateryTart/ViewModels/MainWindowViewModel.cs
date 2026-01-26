using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Sendspin.SDK.Client;
using Sendspin.SDK.Connection;
using Sendspin.SDK.Synchronization;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using ReactiveUI.SourceGenerators;
using WateryTart.MassClient;
using WateryTart.Services;
using WateryTart.Settings;
using WateryTart.Messages;

namespace WateryTart.ViewModels;

public partial class MainWindowViewModel : ReactiveObject, IScreen
{
    private readonly IMassWsClient _massClient;
    
    private readonly ISettings _settings;

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
    [Reactive] public ReactiveCommand<Unit, Unit>  CloseSlideupCommand { get; set; }
    
    [Reactive] public partial bool ShowSlideupMenu { get; set; }

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
            if (vm is not IViewModelBase)
                return;

            var ivmb = ((IViewModelBase)vm);

            ivmb.WhenAnyValue(x => x.Title)
                .BindTo(this, v => v.Title);
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

    public async void Connect()
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
            Thread.Sleep(1000);

        PlayersService.GetPlayers();

        GoHome.Execute();
        //ConnectSendSpinPlayer();
    }

    public async void ConnectSendSpinPlayer()
    {
        // Create dependencies
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var connection = new SendspinConnection(loggerFactory.CreateLogger<SendspinConnection>());
        var clockSync = new KalmanClockSynchronizer(loggerFactory.CreateLogger<KalmanClockSynchronizer>());

        // Create client with device info
        var capabilities = new ClientCapabilities
        {
            ClientName = "My Player",
            ProductName = "Watery Tart",
            Manufacturer = "TemuWolverine",
            SoftwareVersion = "1.0.0"
        };

        var client = new SendspinClientService(
            loggerFactory.CreateLogger<SendspinClientService>(),
            connection,
            clockSync,
            capabilities
        );

        // Connect to server
        await client.ConnectAsync(new Uri("ws://10.0.1.20:8927/sendspin"));


        // Handle events
        client.GroupStateChanged += (sender, group) =>
        {
            Debug.WriteLine($"Now playing: {group.Metadata?.Title}");
        };

        // Send commands
        await client.SendCommandAsync("play");
        await client.SetVolumeAsync(75);
    }
}