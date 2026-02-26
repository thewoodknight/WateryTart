using Sendspin.SDK.Audio;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WateryTart.Platform.Windows.Playback;

public sealed partial class SwitchableAudioPlayer : IAudioPlayer
{
    private readonly Func<IAudioPlayer> _createSimple;
    private readonly Func<IAudioPlayer> _createSoundflow;
    private IAudioPlayer? _active;
    private PlayerBackend _backend;
    private Sendspin.SDK.Models.AudioFormat? _format;
    private bool _isMuted;
    private IAudioSampleSource? _sampleSource;
    private float _volume = 1.0f;
    public event EventHandler<AudioPlayerError>? ErrorOccurred;
    public event EventHandler<AudioPlayerState>? StateChanged;

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            if (_active != null)
                _active.IsMuted = _isMuted;
        }
    }

    public int OutputLatencyMs => _active?.OutputLatencyMs ?? 0;
    public AudioPlayerState State => _active?.State ?? AudioPlayerState.Uninitialized;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_active != null)
                _active.Volume = _volume;
        }
    }

    public SwitchableAudioPlayer(Func<IAudioPlayer> createSimple, Func<IAudioPlayer> createSoundflow, PlayerBackend initial)
    {
        Debug.WriteLine($"SwitchableAudioPlayer.ctor this={GetHashCode()} initial={initial}");
        _createSimple = createSimple ?? throw new ArgumentNullException(nameof(createSimple));
        _createSoundflow = createSoundflow ?? throw new ArgumentNullException(nameof(createSoundflow));
        _backend = initial;
    }

    public async ValueTask DisposeAsync()
    {
        if (_active != null)
        {
            try
            {
                await _active.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
            }
            UnsubscribeActiveEvents(_active);
            _active = null;
        }
    }

    public async Task InitializeAsync(Sendspin.SDK.Models.AudioFormat format, CancellationToken ct = default)
    {
        _format = format ?? throw new ArgumentNullException(nameof(format));

        await CreateAndInitActiveAsync(_backend, ct).ConfigureAwait(false);
    }

    public void Pause()
    {
        _active?.Pause();
    }

    public void Play()
    {
        _active?.Play();
    }

    public void SetSampleSource(IAudioSampleSource source)
    {
        _sampleSource = source ?? throw new ArgumentNullException(nameof(source));
        _active?.SetSampleSource(source);
    }

    public void Stop()
    {
        _active?.Stop();
    }

    public Task SwitchDeviceAsync(string? deviceId, CancellationToken ct = default)
    {
        return _active?.SwitchDeviceAsync(deviceId, ct) ?? Task.CompletedTask;
    }

    public async Task SwitchToAsync(PlayerBackend backend, CancellationToken ct = default)
    {
        if (backend == _backend)
        {
            return;
        }

        // Create new player and initialize it before disposing the old one to reduce gaps
        var newPlayer = CreateForBackend(backend);
        if (newPlayer == null)
            return;

        if (_format != null)
        {
            try
            {
                await newPlayer.InitializeAsync(_format, ct).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        if (_sampleSource != null)
            newPlayer.SetSampleSource(_sampleSource);

        newPlayer.Volume = _volume;
        newPlayer.IsMuted = _isMuted;

        // start new player if we are currently playing
        if (_active?.State == AudioPlayerState.Playing)
        {
            try
            {
                newPlayer.Play();
            }
            catch
            {
            }
        }

        // swap
        var old = _active;
        UnsubscribeActiveEvents(old);
        _active = newPlayer;
        SubscribeActiveEvents(_active);
        _backend = backend;

        if (old != null)
        {
            try
            {
                await old.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }

    private async Task CreateAndInitActiveAsync(PlayerBackend backend, CancellationToken ct)
    {
        _active = CreateForBackend(backend);
        SubscribeActiveEvents(_active);
        if (_format != null)
        {
            try
            {
                await _active.InitializeAsync(_format, ct).ConfigureAwait(false);
            }
            catch
            {
            }
        }
        if (_sampleSource != null)
            _active.SetSampleSource(_sampleSource);
        _active.Volume = _volume;
        _active.IsMuted = _isMuted;
    }

    private IAudioPlayer CreateForBackend(PlayerBackend backend)
    {
        IAudioPlayer p = backend switch
        {
            PlayerBackend.SimpleWasapi => _createSimple(),
            PlayerBackend.SoundFlow => _createSoundflow(),
            _ => _createSoundflow(),
        };
        return p;
    }

    private void OnActiveError(object? sender, AudioPlayerError error)
    {
        ErrorOccurred?.Invoke(this, error);
    }

    private void OnActiveStateChanged(object? sender, AudioPlayerState state)
    {
        StateChanged?.Invoke(this, state);
    }

    private void SubscribeActiveEvents(IAudioPlayer? player)
    {
        if (player == null)
            return;
        player.StateChanged += OnActiveStateChanged;
        player.ErrorOccurred += OnActiveError;
    }

    private void UnsubscribeActiveEvents(IAudioPlayer? player)
    {
        if (player == null)
            return;
        player.StateChanged -= OnActiveStateChanged;
        player.ErrorOccurred -= OnActiveError;
    }
}