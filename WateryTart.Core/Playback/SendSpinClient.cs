using Microsoft.Extensions.Logging;
using Sendspin.SDK.Audio;
using Sendspin.SDK.Client;
using Sendspin.SDK.Connection;
using Sendspin.SDK.Models;
using Sendspin.SDK.Synchronization;
using System;
using System.Threading.Tasks;
using WateryTart.Core.Services;

namespace WateryTart.Core.Playback;

public class SendSpinClient : IDisposable, IReaper
{
    private readonly SendspinClientService _sendspinClient;
    private readonly AudioPipeline _audioPipeline;
    private readonly ILogger<SendSpinClient> _logger;
    private AudioPlayerState _state = AudioPlayerState.Uninitialized;
    private bool _isConnected;
    private bool _disposed;

    public event EventHandler<PlaybackChangedEventArgs> OnPlaybackChanged;
    public event EventHandler<ErrorEventArgs> OnError;
    public event EventHandler<EventArgs> OnConnected;
    public event EventHandler<EventArgs> OnDisconnected;
    public event EventHandler<AudioPlayerState>? StateChanged;
    public event EventHandler<AudioPlayerError>? ErrorOccurred;

    public AudioPlayerState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }
    }

    public SendSpinClient(IPlayerFactory? player = null, ILoggerFactory? loggerFactory = null)
    {
        if (OperatingSystem.IsAndroid())
            return;

        try
        {
            loggerFactory ??= LoggerFactory.Create(b =>
            {
                b.AddConsole();
                b.SetMinimumLevel(LogLevel.Information);
            });

            _logger = loggerFactory.CreateLogger<SendSpinClient>();

            var connection = new SendspinConnection(loggerFactory.CreateLogger<SendspinConnection>());
            var clockSync = new KalmanClockSynchronizer(loggerFactory.CreateLogger<KalmanClockSynchronizer>());
            var decoderFactory = new AudioDecoderFactory();

            Func<AudioFormat, IClockSynchronizer, ITimedAudioBuffer> bufferFactory =
                (format, sync) =>
                {
                    var buffer = new TimedAudioBuffer(
                        format,
                        sync,
                        bufferCapacityMs: 8000,
                        syncOptions: SyncCorrectionOptions.Default,
                        logger: loggerFactory.CreateLogger<TimedAudioBuffer>());

                    buffer.TargetBufferMilliseconds = 250;
                    return buffer;
                };

            Func<ITimedAudioBuffer, Func<long>, IAudioSampleSource> sourceFactory = (buffer, getTime) => new BufferedAudioSampleSource(buffer, getTime);

            _audioPipeline = new AudioPipeline(
                loggerFactory.CreateLogger<AudioPipeline>(),
                decoderFactory,
                clockSync,
                bufferFactory,
                player.CreatePlayer,
                sourceFactory,
                precisionTimer: null,
                waitForConvergence: true,
                convergenceTimeoutMs: 5000,
                useMonotonicTimer: false);

            var capabilities = new ClientCapabilities
            {
                ClientName = $"WateryTart ({Environment.MachineName})",
                ProductName = "WateryTart",
                Manufacturer = "WateryTart",
                SoftwareVersion = "1.0.0",
                InitialVolume = 100,
                InitialMuted = false
            };

            _sendspinClient = new SendspinClientService(
                loggerFactory.CreateLogger<SendspinClientService>(),
                connection,
                clockSync,
                capabilities,
                _audioPipeline
            );

            _sendspinClient.GroupStateChanged += HandleGroupStateChanged;
            _sendspinClient.PlayerStateChanged += HandlePlayerStateChanged;

            State = AudioPlayerState.Stopped;
            _logger.LogInformation("SendSpinNAudioClient initialized.");
        }
        catch (Exception ex)
        {
            State = AudioPlayerState.Error;
            _logger.LogError(ex, "Error initializing SendSpinNAudioClient");
            OnError?.Invoke(this, new ErrorEventArgs(ex));
            ErrorOccurred?.Invoke(this, new AudioPlayerError(ex.Message, ex));
            throw;
        }
    }

    public async Task ConnectAsync(string serverUri)
    {
        //for now, assume the url 
        var temp = new Uri("ws://" + serverUri);
        
        //and also assuming the port, default/recommended port used
        serverUri = $"ws://{temp.Host}:8927/sendspin";

        if (_isConnected)
        {
            _logger.LogInformation("Already connected to Sendspin server.");
            return;
        }

        try
        {
            await _sendspinClient.ConnectAsync(new Uri(serverUri));
            _isConnected = true;
            State = AudioPlayerState.Stopped;
            _logger.LogInformation("Connected to Sendspin server: {ServerUri}", serverUri);
            OnConnected?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _isConnected = false;
            State = AudioPlayerState.Error;
            _logger.LogError(ex, "Error connecting to Sendspin server");
            OnError?.Invoke(this, new ErrorEventArgs(ex));
            ErrorOccurred?.Invoke(this, new AudioPlayerError($"Connection failed: {ex.Message}", ex));
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (!_isConnected)
            return;

        try
        {
            await _sendspinClient.DisconnectAsync();
            _isConnected = false;
            State = AudioPlayerState.Stopped;
            _logger.LogInformation("Disconnected from Sendspin server.");
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            State = AudioPlayerState.Error;
            _logger.LogError(ex, "Error disconnecting from Sendspin server");
            OnError?.Invoke(this, new ErrorEventArgs(ex));
            ErrorOccurred?.Invoke(this, new AudioPlayerError($"Disconnection failed: {ex.Message}", ex));
        }
    }

    /// <summary>
    /// Stop audio playback.
    /// </summary>
    public void Stop()
    {
        try
        {
            if (State == AudioPlayerState.Stopped || State == AudioPlayerState.Uninitialized)
                return;

            State = AudioPlayerState.Stopped;
            _logger.LogInformation("Audio playback stopped");
        }
        catch (Exception ex)
        {
            State = AudioPlayerState.Error;
            _logger.LogError(ex, "Error stopping playback");
            ErrorOccurred?.Invoke(this, new AudioPlayerError($"Playback stop failed: {ex.Message}", ex));
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            Stop();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
        }

        _disposed = true;
    }

    public void Reap()
    {
        _audioPipeline.DisposeAsync();
        DisconnectAsync().GetAwaiter().GetResult();
        Dispose();
        
    }

    private void HandleGroupStateChanged(object sender, GroupState group)
    {
        OnPlaybackChanged?.Invoke(this, new PlaybackChangedEventArgs { GroupState = group });
    }

    private void HandlePlayerStateChanged(object sender, PlayerState playerState)
    {
        _logger.LogInformation(
            "Player state changed: Volume={Volume}%, Muted={Muted}",
            playerState?.Volume ?? 0,
            playerState?.Muted ?? false
        );
    }
}