// <copyright file="SampleSourceProvider.cs" company="Sendspin Windows Client">
// Licensed under the MIT License. See LICENSE file in the project root. https://github.com/chrisuthe/windowsSpin/.
// </copyright>
using NAudio.Wave;
using Sendspin.SDK.Audio;

namespace WateryTart.Platform.Windows.Playback;

internal sealed class SampleSourceProvider : ISampleProvider
{
    private IAudioSampleSource _source;
    public WaveFormat WaveFormat { get; set; }
    public SampleSourceProvider(IAudioSampleSource source, WaveFormat format)
    {
        _source = source;
        WaveFormat = format;
    }

    public void SetWaveFormat(IAudioSampleSource source, WaveFormat format)
    {
        _source = source;
        WaveFormat = format;
    }

    public int Read(float[] buffer, int offset, int count)
        => _source.Read(buffer, offset, count);
}