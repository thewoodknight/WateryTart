// <copyright file="SimpleWasapiPlayer.cs" company="Sendspin Windows Client">
// Licensed under the MIT License. See LICENSE file in the project root. https://github.com/chrisuthe/windowsSpin/.
// </copyright>

using Microsoft.VisualBasic;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Sendspin.SDK.Audio;
using Sendspin.SDK.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;
using WateryTart.Core.Settings;

namespace WateryTart.Platform.Windows.Playback;

public sealed class SimpleWasapiPlayer : IAudioPlayer
{
    private WasapiOut? _wasapiOut;
    private WaveFormat? _waveFormat;
    private AudioFormat? _format;
    private IAudioSampleSource? _sampleSource;
    private AudioSampleProviderAdapter? _sampleProvider;

    public AudioPlayerState State { get; private set; } = AudioPlayerState.Uninitialized;

    public float Volume
    {
        get => field;
        set
        {
            field = Math.Clamp(value, 0f, 1f);
            //If using AppVolume control, change the stream volume of the sample provider. Otherwise, set the WASAPI output volume.
            if (_sampleProvider != null && WateryTart.Core.App.Settings.VolumeEventControl == VolumeEventControl.AppVolume)
            {
                _sampleProvider.Volume = field;
            }
            else
                SetVolume();
        }
    }

    public bool IsMuted { get; set; }
    public int OutputLatencyMs { get; private set; }

    public event EventHandler<AudioPlayerState>? StateChanged;
    public event EventHandler<AudioPlayerError>? ErrorOccurred;
    public void SetVolume()
    {
        if (_wasapiOut == null)
            return;


        _wasapiOut.Volume = Volume;
    }
    public Task InitializeAsync(AudioFormat format, CancellationToken ct = default)
    {
        _format = format;
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
        _sampleProvider = new AudioSampleProviderAdapter(source, _format!);
        _sampleProvider.Volume = 1f;
        _sampleProvider.IsMuted = false;
        _wasapiOut?.Init(_sampleProvider);
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
