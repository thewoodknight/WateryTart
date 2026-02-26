using Sendspin.SDK.Audio;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Backends.MiniAudio.Devices;
using SoundFlow.Components;
using SoundFlow.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WateryTart.Platform.Windows.Playback;

public sealed partial class SoundflowPlayer : IAudioPlayer
{
    private MiniAudioEngine? _engine;
    private Sendspin.SDK.Models.AudioFormat? _format;
    private bool _isMuted;
    private AudioPlaybackDevice? _playbackDevice;
    private IAudioSampleSource? _sampleSource;
    private SoundFlow.Structs.AudioFormat? _sfFormat;
    private SoundPlayer? _soundPlayer;
    private volatile AudioPlayerState _state = AudioPlayerState.Uninitialized;
    private float _volume = 1.0f;

    public event EventHandler<AudioPlayerError>? ErrorOccurred;

    public event EventHandler<AudioPlayerState>? StateChanged;

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
        }
    }

    public int OutputLatencyMs { get; private set; } = 50;
    public AudioPlayerState State => _state;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
        }
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            _soundPlayer?.Dispose();
            _playbackDevice?.Dispose();
            _engine?.Dispose();
        }
        catch
        {
        }
        _soundPlayer = null;

        _playbackDevice = null;
        _engine = null;
        return ValueTask.CompletedTask;
    }

    public Task InitializeAsync(Sendspin.SDK.Models.AudioFormat format, CancellationToken ct = default)
    {
        Debug.WriteLine($"SoundflowPlayer.InitializeAsync this={GetHashCode()}");
        _format = format ?? throw new ArgumentNullException(nameof(format));
        try
        {
            _engine = new MiniAudioEngine();
            _engine.UpdateAudioDevicesInfo();

            var sfFormat = new SoundFlow.Structs.AudioFormat
            {
                Format = SampleFormat.F32,
                Channels = _format.Channels,
                SampleRate = _format.SampleRate,
                Layout = SoundFlow.Structs.AudioFormat.GetLayoutFromChannels(_format.Channels)
            };

            var deviceConfig = new MiniAudioDeviceConfig { NoFixedSizedCallback = true };
            _playbackDevice = _engine.InitializePlaybackDevice(null, sfFormat, deviceConfig);

            _sfFormat = sfFormat;
            _playbackDevice.Start();
        }
        catch (Exception ex)
        {
            RaiseError("SoundFlow init failed", ex, setErrorState: true);
            throw;
        }

        SetState(AudioPlayerState.Stopped);
        return Task.CompletedTask;
    }

    public void Pause()
    {
        try
        {
            _soundPlayer?.Pause();
        }
        catch
        {
        }
        SetState(AudioPlayerState.Paused);
    }

    public void Play()
    {
        if (_format == null)
            return;
        try
        {
            _soundPlayer?.Play();
        }
        catch
        {
        }
        SetState(AudioPlayerState.Playing);
    }

    public void SetSampleSource(IAudioSampleSource source)
    {
        _sampleSource = source ?? throw new ArgumentNullException(nameof(source));
        try
        {
            if (_engine != null && _playbackDevice != null && _sfFormat != null)
            {
                try { _soundPlayer?.Dispose(); } catch { }

                var provider = new SoundFlowSampleSourceProvider(_sampleSource, _sfFormat.Value, () => _volume, () => _isMuted);
                _soundPlayer = new SoundPlayer(_engine, _sfFormat.Value, provider);
                _playbackDevice.MasterMixer.AddComponent(_soundPlayer);
            }
        }
        catch { /* ignore */ }
    }

    public void Stop()
    {
        try
        {
            _soundPlayer?.Stop();
        }
        catch
        {
        }
        SetState(AudioPlayerState.Stopped);
    }

    public Task SwitchDeviceAsync(string? deviceId, CancellationToken ct = default)
    {
        try
        {
            if (_engine != null)
            {
                var dev = _engine.PlaybackDevices.FirstOrDefault(d => string.Equals(d.Name, deviceId, StringComparison.OrdinalIgnoreCase));
                if (dev != null)
                {
                    _engine.SwitchDevice(_playbackDevice!, dev, null);
                }
            }
        }
        catch
        {
        }

        return Task.CompletedTask;
    }

    private void RaiseError(string msg, Exception? ex, bool setErrorState = false)
    {
        if (setErrorState)
            _state = AudioPlayerState.Error;
        ErrorOccurred?.Invoke(this, new AudioPlayerError(msg, ex));
    }

    private void SetState(AudioPlayerState state)
    {
        _state = state;
        StateChanged?.Invoke(this, state);
    }
}