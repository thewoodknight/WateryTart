using Sendspin.SDK.Audio;
using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Metadata.Models;
using SoundFlow.Structs;
using System;
using System.Diagnostics;

namespace WateryTart.Platform.Windows.Playback;

public sealed class SoundFlowSampleSourceProvider : ISoundDataProvider, IDisposable
{
    private readonly Func<bool> _getMuted;
    private readonly Func<float> _getVolume;
    private readonly SoundFlow.Structs.AudioFormat _sfFormat;
    private readonly IAudioSampleSource _source;
    private bool _disposed;
    private int _position;

    public event EventHandler<EventArgs>? EndOfStreamReached;
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;
    public bool CanSeek => false;
    public SoundFormatInfo? FormatInfo => null;
    public bool IsDisposed => _disposed;
    public int Length => -1;
    public int Position => _position;
    public SampleFormat SampleFormat => _sfFormat.Format;
    public int SampleRate
    {
        get => _sfFormat.SampleRate;
        set { /* ignore */ }
    }

    public SoundFlowSampleSourceProvider(IAudioSampleSource source, SoundFlow.Structs.AudioFormat sfFormat, Func<float> getVolume, Func<bool> getMuted)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _sfFormat = sfFormat;
        _position = 0;
        _getVolume = getVolume ?? (() => 1.0f);
        _getMuted = getMuted ?? (() => false);
    }

    public void Dispose()
    {
        _disposed = true;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        // Diagnostic: log that native side is invoking Read
        try
        {
            Debug.WriteLine($"SoundFlowSampleSourceProvider.Read invoked this={GetHashCode()} count={count}");
        }
        catch { }

        // Fill buffer from source
        int samplesRead = 0;
        try
        {
            samplesRead = _source.Read(buffer, offset, count);
            // Apply volume/mute
            var vol = _getVolume();
            var muted = _getMuted();
            if (muted) vol = 0f;
            if (vol != 1f && samplesRead > 0)
            {
                for (int i = offset; i < offset + samplesRead; i++)
                    buffer[i] *= vol;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SoundFlowSampleSourceProvider.Read error: {ex}");
        }

        return samplesRead;
    }

    public int ReadBytes(Span<float> buffer)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SoundFlowSampleSourceProvider));
        if (buffer.IsEmpty) return 0;
        var arr = new float[buffer.Length];
        int read = _source.Read(arr, 0, arr.Length);
        if (read > 0)
        {
            // Apply per-player volume (power curve) before copying to provided buffer
            var volume = _getVolume();
            if (volume < 0.999f || _getMuted())
            {
                var amplitude = _getMuted() ? 0f : (float)Math.Pow(volume, 1.5);
                for (int i = 0; i < read; i++) arr[i] *= amplitude;
            }

            new Span<float>(arr, 0, read).CopyTo(buffer);
            _position += read;
            // Raise PositionChanged event
            PositionChanged?.Invoke(this, new PositionChangedEventArgs(_position));
        }
        else
        {
            // signal end of stream
            EndOfStreamReached?.Invoke(this, EventArgs.Empty);
        }
        return read;
    }

    public void Seek(int offset)
    {
        throw new NotSupportedException();
    }
}