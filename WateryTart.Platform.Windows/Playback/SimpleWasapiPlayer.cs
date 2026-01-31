// <copyright file="SimpleWasapiPlayer.cs" company="Sendspin Windows Client">
// Licensed under the MIT License. See LICENSE file in the project root. https://github.com/chrisuthe/windowsSpin/.
// </copyright>

using Sendspin.SDK.Audio;
using Sendspin.SDK.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace WateryTart.Platform.Windows.Playback;

public sealed class SimpleWasapiPlayer : IAudioPlayer
{
    private WasapiOut? _wasapiOut;
    private WaveFormat? _waveFormat;
    private IAudioSampleSource? _sampleSource;
    private SampleSourceProvider? _provider;

    public AudioPlayerState State { get; private set; } = AudioPlayerState.Uninitialized;
    public float Volume { get; set; } = 1.0f;
    public bool IsMuted { get; set; }
    public int OutputLatencyMs { get; private set; }

    public event EventHandler<AudioPlayerState>? StateChanged;
    public event EventHandler<AudioPlayerError>? ErrorOccurred;

    public Task InitializeAsync(AudioFormat format, CancellationToken ct = default)
    {
        // Create NAudio wave format (SDK always outputs 32-bit float)
        _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(format.SampleRate, format.Channels);

        // Create WASAPI output (shared mode, 50ms latency)
        _wasapiOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, latency: 50);

        OutputLatencyMs = 50;

        SetState(AudioPlayerState.Stopped);
        return Task.CompletedTask;
    }

    public void SetSampleSource(IAudioSampleSource source)
    {
        _sampleSource = source;
        _provider = new SampleSourceProvider(source, _waveFormat!);
        _wasapiOut?.Init(_provider);
    }

    public void Play()
    {
        _wasapiOut?.Play();
        SetState(AudioPlayerState.Playing);
    }

    public void Pause()
    {
        _wasapiOut?.Pause();
        SetState(AudioPlayerState.Paused);
    }

    public void Stop()
    {
        _wasapiOut?.Stop();
        SetState(AudioPlayerState.Stopped);
    }

    public Task SwitchDeviceAsync(string? deviceId, CancellationToken ct = default)
    {
        // For simplicity, this minimal example doesn't support device switching
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _wasapiOut?.Stop();
        _wasapiOut?.Dispose();
    }

    private void SetState(AudioPlayerState state)
    {
        State = state;
        StateChanged?.Invoke(this, state);
    }
}
