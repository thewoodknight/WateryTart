namespace WateryTart.Core.Services.Discovery;

/// <summary>
/// Represents a Music Assistant server discovered via mDNS.
/// </summary>
public sealed class DiscoveredServer
{
    /// <summary>
    /// Hostname of the server (e.g., "mass.local").
    /// </summary>
    public required string Hostname { get; init; }

    /// <summary>
    /// IP address of the server.
    /// </summary>
    public required string Address { get; init; }

    /// <summary>
    /// Port number (typically 8095 for Music Assistant).
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// Server ID from mDNS TXT record.
    /// </summary>
    public string? ServerId { get; init; }

    /// <summary>
    /// Server version from mDNS TXT record.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Whether this server is running as a Home Assistant addon.
    /// When true, authentication must go through Home Assistant.
    /// </summary>
    public bool IsHomeAssistantAddon { get; init; }

    /// <summary>
    /// Base URL from mDNS TXT record (e.g., "http://192.168.1.63:8095").
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// Returns the connection URL for this server (IP:port format).
    /// </summary>
    public string ConnectionUrl => $"{Address}:{Port}";

    /// <summary>
    /// Friendly display name for UI.
    /// Shows hostname without trailing dot, or falls back to IP.
    /// </summary>
    public string DisplayName
    {
        get
        {
            var name = Hostname.TrimEnd('.');
            if (string.IsNullOrEmpty(name) || name == ServerId)
            {
                return $"Music Assistant ({Address})";
            }
            return name;
        }
    }

    /// <summary>
    /// Full display string including version if available.
    /// </summary>
    public string FullDisplayName
    {
        get
        {
            var name = DisplayName;
            if (!string.IsNullOrEmpty(Version))
            {
                return $"{name} (v{Version})";
            }
            return name;
        }
    }

    public override string ToString() => DisplayName;
}
