using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services.Discovery;
using WateryTart.Core.Settings;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models.Auth;

namespace WateryTart.Core.ViewModels;

public partial class ServerSettingsViewModel : ReactiveObject, IHaveSettings, IDisposable
{
    private readonly MusicAssistantClient _massClient;
    private readonly ISettings _settings;
    private readonly IMassServerDiscovery _discovery;
    private bool _disposed;

    public MaterialIconKind Icon => MaterialIconKind.Server;

    // Current connection info (read-only display)
    [Reactive] public partial string CurrentServer { get; set; } = string.Empty;
    [Reactive] public partial string CurrentUsername { get; set; } = string.Empty;
    public string MaskedPassword => "**********";

    // New connection fields
    [Reactive] public partial string Server { get; set; } = string.Empty;
    [Reactive] public partial string Username { get; set; } = string.Empty;
    [Reactive] public partial string Password { get; set; } = string.Empty;
    [Reactive] public partial string ErrorMessage { get; set; } = string.Empty;
    [Reactive] public partial bool HasError { get; set; } = false;
    [Reactive] public partial bool IsLoading { get; set; } = false;
    [Reactive] public partial bool IsScanning { get; set; } = false;
    [Reactive] public partial DiscoveredServer? SelectedServer { get; set; }
    [Reactive] public partial bool IsHomeAssistantAddon { get; set; } = false;
    [Reactive] public partial bool ShowSuccess { get; set; } = false;

    public ObservableCollection<DiscoveredServer> DiscoveredServers { get; } = new();

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand RefreshServersCommand { get; }

    public ServerSettingsViewModel(MusicAssistantClient massClient, ISettings settings, IMassServerDiscovery? discovery = null)
    {
        _massClient = massClient;
        _settings = settings;
        _discovery = discovery ?? new MassServerDiscovery();

        // Load current settings
        CurrentServer = _settings.Credentials?.BaseUrl ?? "Not configured";
        CurrentUsername = _settings.Credentials?.Username ?? "Not configured";

        SaveCommand = new AsyncRelayCommand(ExecuteSave);
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

            var servers = await _discovery.ScanAsync(TimeSpan.FromSeconds(3));
            foreach (var server in servers)
            {
                if (!DiscoveredServers.Any(s => s.Address == server.Address && s.Port == server.Port))
                {
                    DiscoveredServers.Add(server);
                }
            }

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
                SetError("No Music Assistant servers found on the network.");
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
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (DiscoveredServers.Any(s => s.Address == server.Address && s.Port == server.Port)) 
                return;

            DiscoveredServers.Add(server);
            SelectedServer ??= server;
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

                if (SelectedServer == existing)
                {
                    SelectedServer = DiscoveredServers.FirstOrDefault();
                }
            }
        });
    }

    private async Task ExecuteSave()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ShowSuccess = false;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Server))
            {
                SetError("Server address is required.");
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

            var result = await _massClient.WithWs().GetAuthToken(Username, Password);

            if (result.Success)
            {
                _settings.Credentials = new MusicAssistantCredentials()
                {
                    BaseUrl = result.Credentials.BaseUrl,
                    Token = result.Credentials.Token,
                    Username = Username
                };

                // Update display
                CurrentServer = result.Credentials.BaseUrl;
                CurrentUsername = Username;

                // Clear form
                Server = string.Empty;
                Username = string.Empty;
                Password = string.Empty;

                ShowSuccess = true;

                // Hide success after 3 seconds
                _ = Task.Delay(3000).ContinueWith(_ =>
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => ShowSuccess = false));
            }
            else
            {
                SetError(result.Error ?? "Login failed");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to save: {ex.Message}");
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
