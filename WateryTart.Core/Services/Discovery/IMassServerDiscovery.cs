using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WateryTart.Core.Services.Discovery;

/// <summary>
/// Service for discovering Music Assistant servers on the local network via mDNS.
/// </summary>
public interface IMassServerDiscovery
{
    /// <summary>
    /// Event raised when a server is discovered.
    /// </summary>
    event EventHandler<DiscoveredServer>? ServerDiscovered;

    /// <summary>
    /// Event raised when a server is lost (no longer responding).
    /// </summary>
    event EventHandler<DiscoveredServer>? ServerLost;

    /// <summary>
    /// Gets the currently discovered servers.
    /// </summary>
    IReadOnlyList<DiscoveredServer> DiscoveredServers { get; }

    /// <summary>
    /// Starts discovery. Call this to begin searching for servers.
    /// </summary>
    Task StartDiscoveryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops discovery and clears the discovered servers list.
    /// </summary>
    void StopDiscovery();

    /// <summary>
    /// Performs a one-time scan for servers.
    /// </summary>
    /// <param name="scanTime">How long to scan (default 5 seconds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of discovered servers.</returns>
    Task<IReadOnlyList<DiscoveredServer>> ScanAsync(TimeSpan? scanTime = null, CancellationToken cancellationToken = default);
}
