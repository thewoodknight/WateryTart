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
using System.Diagnostics;
using System.Collections;
using System.Reflection;

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
        _format = format ?? throw new ArgumentNullException(nameof(format));
        try
        {
            _engine = new MiniAudioEngine();
            _engine.UpdateAudioDevicesInfo();

            // Diagnostic: enumerate devices in a reflection-safe way
            try
            {
                Debug.WriteLine($"SoundflowPlayer.InitializeAsync this={GetHashCode()}");

                object? devicesObj = null;
                var engineType = _engine.GetType();

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
                        var name = TryGetMemberString(d, new[] { "Name", "DeviceName", "FriendlyName" });
                        var id = TryGetMemberString(d, new[] { "Id", "DeviceId" });
                        Debug.WriteLine($"Soundflow device[{index}]: Type={d?.GetType().FullName} Name=\"{name}\" Id=\"{id}\"");
                        index++;
                    }
                    Debug.WriteLine($"Soundflow: PlaybackDevices enumerated count={index}");
                }
                else
                {
                    Debug.WriteLine($"Soundflow: PlaybackDevices object type: {(devicesObj == null ? "<null>" : devicesObj.GetType().FullName)}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Soundflow device enumeration failed: {ex}");
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
                var devName = TryGetMemberString(_playbackDevice, new[] { "Name", "DeviceName", "FriendlyName" });
                var devId = TryGetMemberString(_playbackDevice, new[] { "Id", "DeviceId" });
                Debug.WriteLine($"Soundflow: InitializePlaybackDevice returned: Type={_playbackDevice?.GetType().FullName} Name=\"{devName}\" Id=\"{devId}\"");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Soundflow: playback device probe failed: {ex}");
            }

            _sfFormat = sfFormat;
            _playbackDevice.Start();
            Debug.WriteLine("Soundflow: playback device Start() called");
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
                Debug.WriteLine($"Soundflow: created SoundFlowSampleSourceProvider thisPlayer={GetHashCode()} providerHash={provider.GetHashCode()}");

                _soundPlayer = new SoundPlayer(_engine, _sfFormat.Value, provider);
                _playbackDevice.MasterMixer.AddComponent(_soundPlayer);

                Debug.WriteLine("Soundflow: SoundPlayer added to MasterMixer");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Soundflow SetSampleSource failed: {ex}");
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
        //State = state;
        StateChanged?.Invoke(this, state);
    }

    private static string TryGetMemberString(object? obj, string[] candidateNames)
    {
        if (obj == null) return string.Empty;
        var t = obj.GetType();
        foreach (var name in candidateNames)
        {
            var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
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
        try { return obj.ToString() ?? string.Empty; } catch { return string.Empty; }
    }
}