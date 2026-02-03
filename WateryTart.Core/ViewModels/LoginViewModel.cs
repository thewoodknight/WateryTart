using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Runtime;
using System.Threading.Tasks;
using WateryTart.Core.Services.Discovery;
using WateryTart.Core.Settings;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models.Auth;

namespace WateryTart.Core.ViewModels;

public class FromLoginMessage()
{

}
public partial class LoginViewModel : ReactiveObject, IViewModelBase, IDisposable
{
    private readonly IMassWsClient _massClient;
    private readonly ISettings _settings;
    private readonly IScreen _screen;
    private readonly IMassServerDiscovery _discovery;
    private bool _disposed;

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; set; }
    public bool ShowMiniPlayer { get; }
    public bool ShowNavigation { get; }

    [Reactive] public partial string Server { get; set; }

    [Reactive] public partial string Username { get; set; }

    [Reactive] public partial string Password { get; set; }

    [Reactive] public partial string ErrorMessage { get; set; }

    [Reactive] public partial bool HasError { get; set; }

    [Reactive] public partial bool IsLoading { get; set; }

    [Reactive] public partial bool IsScanning { get; set; }

    [Reactive] public partial DiscoveredServer? SelectedServer { get; set; }

    [Reactive] public partial bool IsHomeAssistantAddon { get; set; }

    public ObservableCollection<DiscoveredServer> DiscoveredServers { get; } = new();

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshServersCommand { get; }

    public LoginViewModel(IScreen screen, IMassWsClient massClient, ISettings settings, IMassServerDiscovery? discovery = null)
    {
        _massClient = massClient;
        _settings = settings;
        _screen = screen;
        _discovery = discovery ?? new MassServerDiscovery();

        LoginCommand = ReactiveCommand.CreateFromTask(ExecuteLogin);
        RefreshServersCommand = ReactiveCommand.CreateFromTask(RefreshServers);

        // Subscribe to discovery events
        _discovery.ServerDiscovered += OnServerDiscovered;
        _discovery.ServerLost += OnServerLost;

        // Watch for server selection changes
        this.WhenAnyValue(x => x.SelectedServer)
            .Subscribe(server =>
            {
                if (server != null)
                {
                    Server = server.ConnectionUrl;
                    IsHomeAssistantAddon = server.IsHomeAssistantAddon;
                }
                else
                {
                    IsHomeAssistantAddon = false;
                }
            });

        // Start discovery immediately
#pragma warning disable CS4014 // Fire-and-forget intentional - starts async discovery on construction
        _ = StartDiscoveryAsync();
#pragma warning restore CS4014
    }

    private async Task StartDiscoveryAsync()
    {
        try
        {
            IsScanning = true;
            await _discovery.StartDiscoveryAsync();

            // Do an initial scan
            var servers = await _discovery.ScanAsync(TimeSpan.FromSeconds(3));
            foreach (var server in servers)
            {
                if (!DiscoveredServers.Any(s => s.Address == server.Address && s.Port == server.Port))
                {
                    DiscoveredServers.Add(server);
                }
            }

            // Auto-select first server if none selected and we found servers
            if (SelectedServer == null && DiscoveredServers.Count > 0)
            {
                SelectedServer = DiscoveredServers[0];
            }
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task RefreshServers()
    {
        try
        {
            IsScanning = true;
            HasError = false;

            DiscoveredServers.Clear();
            var servers = await _discovery.ScanAsync(TimeSpan.FromSeconds(5));

            foreach (var server in servers)
            {
                DiscoveredServers.Add(server);
            }

            if (DiscoveredServers.Count == 0)
            {
                SetError("No Music Assistant servers found on the network. Make sure your server is running.");
            }
            else if (SelectedServer == null)
            {
                SelectedServer = DiscoveredServers[0];
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to scan for servers: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
        }
    }

    private void OnServerDiscovered(object? sender, DiscoveredServer server)
    {
        // Ensure UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (!DiscoveredServers.Any(s => s.Address == server.Address && s.Port == server.Port))
            {
                DiscoveredServers.Add(server);

                // Auto-select if it's the first one
                if (SelectedServer == null)
                {
                    SelectedServer = server;
                }
            }
        });
    }

    private void OnServerLost(object? sender, DiscoveredServer server)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var existing = DiscoveredServers.FirstOrDefault(s =>
                s.Address == server.Address && s.Port == server.Port);

            if (existing != null)
            {
                DiscoveredServers.Remove(existing);

                // If we lost the selected server, clear selection
                if (SelectedServer == existing)
                {
                    SelectedServer = DiscoveredServers.FirstOrDefault();
                }
            }
        });
    }

    private async Task Login()
    {
        var x = await _massClient.Login(Username, Password, Server);

        if (x.Success)
        {
            _settings.Credentials = new MassCredentials()
            {
                BaseUrl = x.Credentials.BaseUrl,
                Token = x.Credentials.Token,
                Username = Username
            };

            MessageBus.Current.SendMessage(new FromLoginMessage());
            _screen.Router.NavigateBack.Execute();
            return;
        }
        SetError(x.Error);
    }

    private async Task ExecuteLogin()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(Server))
            {
                SetError("Server address is required. Select a discovered server or enter manually.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                SetError("Username is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                SetError("Password is required.");
                return;
            }

            await Login();
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _discovery.ServerDiscovered -= OnServerDiscovered;
        _discovery.ServerLost -= OnServerLost;
        _discovery.StopDiscovery();

        if (_discovery is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
