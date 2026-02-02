// <copyright file="DynamicResamplerFactory.cs" company="Sendspin Player">
// Licensed under the MIT License. See LICENSE file in the project root https://github.com/chrisuthe/sendspin-player.
// </copyright>

using Sendspin.Core.Audio;
using Sendspin.Platform.Shared.Audio;

namespace Sendspin.Platform.macOS.Audio;

/// <summary>
/// Factory for creating dynamic resamplers on macOS.
/// Uses linear interpolation fallback (speexdsp could be added later if needed).
/// </summary>
public static class DynamicResamplerFactory
{
    public static IDynamicResampler Create(int sampleRate, int channels = 1, ResamplerQuality quality = ResamplerQuality.Default)
    {
        // For now, use the linear interpolation fallback on macOS
        // SpeexDSP could be added later via homebrew (brew install speexdsp)
        return new LinearInterpolationResampler(channels);
    }
}
