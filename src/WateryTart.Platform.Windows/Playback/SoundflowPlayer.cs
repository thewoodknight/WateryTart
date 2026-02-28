using Sendspin.SDK.Audio;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Backends.MiniAudio.Devices;
using SoundFlow.Components;
using SoundFlow.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging;

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

    private readonly ILogger _logger;
    public SoundflowPlayer(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger<SoundflowPlayer>();

    public Task InitializeAsync(Sendspin.SDK.Models.AudioFormat format, CancellationToken ct = default)
    {
        _format = format ?? throw new ArgumentNullException(nameof(format));
        try
        {
            _engine = new MiniAudioEngine();
            _engine.UpdateAudioDevicesInfo();

            // Diagnostic: enumerate devices in a reflection-safe way
            try
            {
                _logger.LogDebug("SoundflowPlayer.InitializeAsync this={Hash}", GetHashCode());

                object? devicesObj = null;
                // Use compile-time type to satisfy the trimmer/analysis requirement
                var engineType = typeof(MiniAudioEngine);

                // Try property first
                var prop = engineType.GetProperty("PlaybackDevices", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    devicesObj = prop.GetValue(_engine);
                }
                else
                {
                    // Try method
                    var m = engineType.GetMethod("PlaybackDevices", BindingFlags.Public | BindingFlags.Instance);
                    if (m != null)
                    {
                        devicesObj = m.Invoke(_engine, null);
                    }
                }

                if (devicesObj is IEnumerable devices)
                {
                    int index = 0;
                    foreach (var d in devices)
                    {
                        // Try to read common properties via reflection, fall back to ToString
                        var name = TryGetMemberString(d, ["Name", "DeviceName", "FriendlyName"]);
                        var id = TryGetMemberString(d, [ "Id", "DeviceId" ]);
                        _logger.LogDebug("Soundflow device[{Index}]: Type={Type} Name=\"{Name}\" Id=\"{Id}\"", index, d?.GetType().FullName, name, id);
                        index++;
                    }
                    _logger.LogDebug("Soundflow: PlaybackDevices enumerated count={Count}", index);
                }
                else
                {
                    _logger.LogDebug("Soundflow: PlaybackDevices object type: {Type}", devicesObj == null ? "<null>" : devicesObj.GetType().FullName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Soundflow device enumeration failed");
            }

            var sfFormat = new SoundFlow.Structs.AudioFormat
            {
                Format = SampleFormat.F32,
                Channels = _format.Channels,
                SampleRate = _format.SampleRate,
                Layout = SoundFlow.Structs.AudioFormat.GetLayoutFromChannels(_format.Channels)
            };

            var deviceConfig = new MiniAudioDeviceConfig { NoFixedSizedCallback = true };
            _playbackDevice = _engine.InitializePlaybackDevice(null, sfFormat, deviceConfig);

            // Diagnostic: report device assignment without assuming Name/Id members
            try
            {
                var devName = TryGetMemberString(_playbackDevice, [ "Name", "DeviceName", "FriendlyName" ]);
                var devId = TryGetMemberString(_playbackDevice, ["Id", "DeviceId"]);
                _logger.LogDebug("Soundflow: InitializePlaybackDevice returned: Type={Type} Name=\"{Name}\" Id=\"{Id}\"", _playbackDevice?.GetType().FullName, devName, devId);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Soundflow: playback device probe failed");
            }

            _sfFormat = sfFormat;
            _playbackDevice.Start();
            _logger.LogDebug("Soundflow: playback device Start() called");
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

                // Diagnostic: probe provider if it implements a probe interface or log creation
                _logger.LogDebug("Soundflow: created SoundFlowSampleSourceProvider thisPlayer={PlayerHash} providerHash={ProviderHash}", GetHashCode(), provider.GetHashCode());

                _soundPlayer = new SoundPlayer(_engine, _sfFormat.Value, provider);
                _playbackDevice.MasterMixer.AddComponent(_soundPlayer);

                _logger.LogDebug("Soundflow: SoundPlayer added to MasterMixer");
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Soundflow SetSampleSource failed");
        }
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

                _engine.SwitchDevice(_playbackDevice!, dev, null);
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
        //State = state;
        StateChanged?.Invoke(this, state);
    }

    private static string TryGetMemberString(object? obj, string[] candidateNames)
    {
        if (obj == null) return string.Empty;
        var t = obj.GetType();
        foreach (var name in candidateNames)
        {
            PropertyInfo? p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (p != null)
            {
                try
                {
                    var v = p.GetValue(obj);
                    return v?.ToString() ?? string.Empty;
                }
                catch { }
            }

            var f = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
            {
                try
                {
                    var v = f.GetValue(obj);
                    return v?.ToString() ?? string.Empty;
                }
                catch { }
            }
        }
        // Fallback to ToString
        try
        {
            return obj.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}