// <copyright file="SimpleWasapiPlayer.cs" company="Sendspin Windows Client">
// Licensed under the MIT License. See LICENSE file in the project root. https://github.com/chrisuthe/windowsSpin/.
// </copyright>

using Microsoft.VisualBasic;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Sendspin.SDK.Audio;
using Sendspin.SDK.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;
using WateryTart.Core.Settings;
using System.Runtime.InteropServices;

namespace WateryTart.Platform.Windows.Playback;

public sealed class SimpleWasapiPlayer : IAudioPlayer
{
    private WasapiOut? _wasapiOut;
    private WaveFormat? _waveFormat;
    private AudioFormat? _format;
 //   private IAudioSampleSource? _sample_source;
    private AudioSampleProviderAdapter? _sampleProvider;
    private readonly object _sync = new();

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
#pragma warning disable CS0067
    public event EventHandler<AudioPlayerError>? ErrorOccurred;
#pragma warning restore CS0067

    public void SetVolume()
    {
        lock (_sync)
        {
            if (_wasapiOut == null)
                return;

            _wasapiOut.Volume = Volume;
        }
    }

    public Task InitializeAsync(AudioFormat format, CancellationToken ct = default)
    {
        Debug.WriteLine($"SimpleWasapiPlayer.InitializeAsync this={GetHashCode()}");
        _format = format;
        // Create NAudio wave format (SDK always outputs 32-bit float)
        _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(format.SampleRate, format.Channels);

        // Create WASAPI output (shared mode, 50ms latency)
        lock (_sync)
        {
            _wasapiOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, latency: 50);
            OutputLatencyMs = 50;
        }

        SetState(AudioPlayerState.Stopped);
        return Task.CompletedTask;
    }

    public void SetSampleSource(IAudioSampleSource source)
    {
        _sampleProvider = new AudioSampleProviderAdapter(source, _format!);
        _sampleProvider.Volume = 1f;
        _sampleProvider.IsMuted = false;

        // Log the expected format for diagnostics
        try
        {
            Debug.WriteLine($"SetSampleSource: format sampleRate={_format?.SampleRate} channels={_format?.Channels} this={GetHashCode()}");
        }
        catch { }

        // Try to init, and retry a few times if device was transiently invalidated
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            WasapiOut? candidate = null;
            try
            {
                // Always create a fresh WasapiOut for each attempt
                candidate = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, useEventSync: true, latency: 50);

                // Try to init the fresh instance (may throw COMException)
                candidate.Init(_sampleProvider);

                // Init succeeded — publish the new instance and dispose the old one safely
                WasapiOut? old = null;
                lock (_sync)
                {
                    old = _wasapiOut;
                    _wasapiOut = candidate;
                    OutputLatencyMs = 50;
                }

                // dispose old outside lock
                try { old?.Dispose(); } catch { }

                Debug.WriteLine($"Wasapi Init succeeded on attempt {attempt + 1}");
                return;
            }
            catch (COMException ex)
            {
                Debug.WriteLine($"Wasapi Init COMException HRESULT=0x{ex.ErrorCode:X8} attempt={attempt + 1}: {ex.Message}");
                try { candidate?.Dispose(); } catch { }
                try { Thread.Sleep(100); } catch { }
                // continue retrying
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wasapi Init unexpected exception attempt={attempt + 1}: {ex}");
                try { candidate?.Dispose(); } catch { }
                break;
            }
        }

        Debug.WriteLine("Wasapi Init failed after retries; audio will remain inactive.");
    }

    public void Play()
    {
        lock (_sync)
        {
            _wasapiOut?.Play();
        }
        SetState(AudioPlayerState.Playing);
    }

    public void Pause()
    {
        lock (_sync)
        {
            _wasapiOut?.Pause();
        }
        SetState(AudioPlayerState.Paused);
    }

    public void Stop()
    {
        lock (_sync)
        {
            _wasapiOut?.Stop();
        }
        SetState(AudioPlayerState.Stopped);
    }

    public Task SwitchDeviceAsync(string? deviceId, CancellationToken ct = default)
    {
        // For simplicity, this minimal example doesn't support device switching
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        lock (_sync)
        {
            try { _wasapiOut?.Stop(); } catch { }
            try { _wasapiOut?.Dispose(); } catch { }
            _wasapiOut = null;
        }
    }

    private void SetState(AudioPlayerState state)
    {
        State = state;
        StateChanged?.Invoke(this, state);
    }
}
