using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WateryTart.Core.Services.Discovery;
using WateryTart.Core.Settings;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models.Auth;
using CommunityToolkit.Mvvm.Input;

namespace WateryTart.Core.ViewModels;

public class FromLoginMessage()
{

}
public partial class LoginViewModel : ReactiveObject, IViewModelBase, IDisposable
{
    private readonly MusicAssistantClient _massClient;
    private readonly ISettings _settings;
    private readonly IMassServerDiscovery _discovery;
    private bool _disposed;

    public string? UrlPathSegment { get; } = "login";

    public IScreen HostScreen { get; }
    public string Title { get; set; } = "Login";
    public bool ShowMiniPlayer { get; } = false;
    public bool ShowNavigation { get; } = false;

    [Reactive] public partial string Server { get; set; } = string.Empty;

    [Reactive] public partial string Username { get; set; } = string.Empty;

    [Reactive] public partial string Password { get; set; } = string.Empty;

    [Reactive] public partial string ErrorMessage { get; set; } = string.Empty;

    [Reactive] public partial bool HasError { get; set; } = false;

    [Reactive] public partial bool IsLoading { get; set; } = false;

    [Reactive] public partial bool IsScanning { get; set; } = false;

    [Reactive] public partial DiscoveredServer? SelectedServer { get; set; }

    [Reactive] public partial bool IsHomeAssistantAddon { get; set; } = false;

    public ObservableCollection<DiscoveredServer> DiscoveredServers { get; } = new();

    public AsyncRelayCommand LoginCommand { get; }
    public AsyncRelayCommand RefreshServersCommand { get; }

    public LoginViewModel(IScreen screen, MusicAssistantClient massClient, ISettings settings, IMassServerDiscovery? discovery = null)
    {
        _massClient = massClient;
        _settings = settings;
        HostScreen = screen;
        _discovery = discovery ?? new MassServerDiscovery();

        LoginCommand = new AsyncRelayCommand(ExecuteLogin);
        RefreshServersCommand = new AsyncRelayCommand(RefreshServers);

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
            else 
                SelectedServer ??= DiscoveredServers[0];
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
                SelectedServer ??= server;
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
        _massClient.SetBaseUrl(Server);
        var x = await _massClient.WithWs().GetAuthToken(Username, Password);

        if (x.Success)
        {
            _settings.Credentials = new MusicAssistantCredentials()
            {
                BaseUrl = GetJustHost(x.Credentials.BaseUrl),
                Token = x.Credentials.Token,
                Username = Username
            };

            MessageBus.Current.SendMessage(new FromLoginMessage());
            HostScreen.Router.NavigateBack.Execute();
            return;
        }
        SetError(x.Error);
    }

    private string GetJustHost(string urlOrHost)
    {
        if (Uri.TryCreate(urlOrHost, UriKind.Absolute, out var uri))
        {
            // If the port is not specified, use the default port for the scheme
            int port = uri.IsDefaultPort ? (uri.Scheme == Uri.UriSchemeHttps ? 443 : 80) : uri.Port;
            return $"{uri.Host}:{port}";
        }
        return urlOrHost;
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
