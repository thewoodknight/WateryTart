using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Sendspin.SDK.Client;
using Sendspin.SDK.Connection;
using Sendspin.SDK.Synchronization;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading;
using ReactiveUI.SourceGenerators;
using WateryTart.MassClient;
using WateryTart.Services;
using WateryTart.Settings;

namespace WateryTart.ViewModels;

public partial class MainWindowViewModel : ReactiveObject, IScreen
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    private readonly ISettings _settings;

    public RoutingState Router { get; } = new();

    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;
    public ReactiveCommand<Unit, IRoutableViewModel> GoHome { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoMusic { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoSearch { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoSettings { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoPlayers { get; }

    [Reactive] public partial string Title { get; set; }


    public MainWindowViewModel(IMassWsClient massClient, IPlayersService playersService, ISettings settings)
    {
        _massClient = massClient;
        _playersService = playersService;
        _settings = settings;

        GoHome = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<HomeViewModel>()));
        GoMusic = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<LibraryViewModel>()));
        GoSearch = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<SearchViewModel>()));
        GoSettings = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<SettingsViewModel>()));
        GoPlayers = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(App.Container.GetRequiredService<PlayersViewModel>()));
        Router.CurrentViewModel.Subscribe((vm) =>
        {
            if (vm is not IViewModelBase)
                return;

            var ivmb = ((IViewModelBase)vm);

            ivmb.WhenAnyValue(x => x.Title)
                .BindTo(this, v => v.Title);
        });
    }

    public async void Connect()
    {
        if (string.IsNullOrEmpty(_settings.Credentials.Token))
        {
            Router.Navigate.Execute(App.Container.GetRequiredService<LoginViewModel>());
            return;
        }

        _massClient.Connect(_settings.Credentials);

        while (_massClient.IsConnected == false)
            Thread.Sleep(1000);

        _playersService.GetPlayers();

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