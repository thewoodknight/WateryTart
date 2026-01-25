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
        {
            Router.Navigate.Execute(App.Container.GetRequiredService<LoginViewModel>());
            return;
        }

        _massClient.Connect(_settings.Credentials);

        while (_massClient.IsConnected == false)
            Thread.Sleep(1000);

        _playersService.GetPlayers();
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