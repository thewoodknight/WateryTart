using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zeroconf;

namespace WateryTart.Core.Services.Discovery;

/// <summary>
/// mDNS-based discovery service for Music Assistant servers.
/// Uses the _mass._tcp service type to find servers on the local network.
/// </summary>
public sealed class MassServerDiscovery : IMassServerDiscovery, IDisposable
{
    // Music Assistant mDNS service type
    private const string ServiceType = "_mass._tcp.local.";
    private static readonly TimeSpan DefaultScanTime = TimeSpan.FromSeconds(5);

    private readonly ILogger<MassServerDiscovery>? _logger;
    private readonly List<DiscoveredServer> _discoveredServers = new();
    private readonly object _lock = new();
    private CancellationTokenSource? _discoveryCts;
    private bool _disposed;

    public event EventHandler<DiscoveredServer>? ServerDiscovered;
    public event EventHandler<DiscoveredServer>? ServerLost;

    public IReadOnlyList<DiscoveredServer> DiscoveredServers
    {
        get
        {
            lock (_lock)
            {
                return _discoveredServers.ToList();
            }
        }
    }

    public MassServerDiscovery(ILogger<MassServerDiscovery>? logger = null)
    {
        _logger = logger;
    }

    public async Task StartDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        StopDiscovery();
        _discoveryCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _logger?.LogInformation("Starting Music Assistant server discovery via mDNS");

        // Run continuous discovery in background
#pragma warning disable CS4014 // Fire-and-forget intentional - runs continuous discovery loop
        _ = ContinuousDiscoveryAsync(_discoveryCts.Token);
#pragma warning restore CS4014
    }

    private async Task ContinuousDiscoveryAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ScanAsync(DefaultScanTime, ct);
                await Task.Delay(TimeSpan.FromSeconds(30), ct); // Re-scan every 30 seconds
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error during mDNS discovery scan");
                await Task.Delay(TimeSpan.FromSeconds(5), ct); // Wait before retry
            }
        }
    }

    public void StopDiscovery()
    {
        _discoveryCts?.Cancel();
        _discoveryCts?.Dispose();
        _discoveryCts = null;

        lock (_lock)
        {
            _discoveredServers.Clear();
        }

        _logger?.LogInformation("Stopped Music Assistant server discovery");
    }

    public async Task<IReadOnlyList<DiscoveredServer>> ScanAsync(TimeSpan? scanTime = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var effectiveScanTime = scanTime ?? DefaultScanTime;
        _logger?.LogDebug("Scanning for Music Assistant servers for {ScanTime}", effectiveScanTime);

        try
        {
            var results = await ZeroconfResolver.ResolveAsync(
                ServiceType,
                effectiveScanTime,
                cancellationToken: cancellationToken);

            var newServers = new List<DiscoveredServer>();

            foreach (var result in results)
            {
                foreach (var service in result.Services.Values)
                {
                    var server = CreateDiscoveredServer(result, service);
                    if (server != null)
                    {
                        newServers.Add(server);
                    }
                }
            }

            UpdateDiscoveredServers(newServers);
            return DiscoveredServers;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to scan for Music Assistant servers");
            return DiscoveredServers;
        }
    }

    private DiscoveredServer? CreateDiscoveredServer(IZeroconfHost host, IService service)
    {
        try
        {
            // Get the first IPv4 address
            var address = host.IPAddresses.FirstOrDefault(ip => !ip.Contains(':'));
            if (string.IsNullOrEmpty(address))
            {
                address = host.IPAddresses.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(address))
            {
                _logger?.LogDebug("No IP address found for host {Host}", host.DisplayName);
                return null;
            }

            // Extract TXT record properties
            string? serverId = null;
            string? version = null;
            string? baseUrl = null;
            bool isHaAddon = false;

            foreach (var prop in service.Properties)
            {
                foreach (var kvp in prop)
                {
                    var key = kvp.Key.ToLowerInvariant();
                    switch (key)
                    {
                        case "server_id":
                            serverId = kvp.Value;
                            break;
                        case "server_version":
                            version = kvp.Value;
                            break;
                        case "base_url":
                            baseUrl = kvp.Value;
                            break;
                        case "homeassistant_addon":
                            isHaAddon = kvp.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                       kvp.Value.Equals("True", StringComparison.OrdinalIgnoreCase);
                            break;
                    }
                }
            }

            // Determine hostname - prefer extracting from base_url or use "Music Assistant"
            var hostname = "Music Assistant";

            // Try to extract hostname from base_url if available
            if (!string.IsNullOrEmpty(baseUrl))
            {
                try
                {
                    var uri = new Uri(baseUrl);
                    // Use the host part (e.g., "192.168.1.63" or "mass.local")
                    hostname = uri.Host;

                    // If it's an IP, make it friendlier
                    if (System.Net.IPAddress.TryParse(hostname, out _))
                    {
                        hostname = $"Music Assistant ({hostname})";
                    }
                }
                catch
                {
                    // Keep default
                }
            }

            // If DisplayName looks like a hostname (contains dot, not a hash), use it
            if (!string.IsNullOrEmpty(host.DisplayName) &&
                host.DisplayName.Contains('.') &&
                host.DisplayName != serverId)
            {
                hostname = host.DisplayName.TrimEnd('.');
            }

            var server = new DiscoveredServer
            {
                Hostname = hostname,
                Address = address,
                Port = service.Port,
                ServerId = serverId,
                Version = version,
                BaseUrl = baseUrl,
                IsHomeAssistantAddon = isHaAddon
            };

            _logger?.LogDebug("Discovered Music Assistant server: {Server} at {Address}:{Port} (HA Addon: {IsHaAddon})",
                server.DisplayName, server.Address, server.Port, server.IsHomeAssistantAddon);

            return server;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to parse discovered service from host {Host}", host.DisplayName);
            return null;
        }
    }

    private void UpdateDiscoveredServers(List<DiscoveredServer> newServers)
    {
        lock (_lock)
        {
            // Find new servers
            foreach (var server in newServers)
            {
                var existing = _discoveredServers.FirstOrDefault(s =>
                    s.Address == server.Address && s.Port == server.Port);

                if (existing == null)
                {
                    _discoveredServers.Add(server);
                    _logger?.LogInformation("New Music Assistant server discovered: {Server}", server.DisplayName);
                    ServerDiscovered?.Invoke(this, server);
                }
            }

            // Find lost servers
            var lostServers = _discoveredServers
                .Where(s => !newServers.Any(n => n.Address == s.Address && n.Port == s.Port))
                .ToList();

            foreach (var server in lostServers)
            {
                _discoveredServers.Remove(server);
                _logger?.LogInformation("Music Assistant server lost: {Server}", server.DisplayName);
                ServerLost?.Invoke(this, server);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopDiscovery();
    }
}
