// <copyright file="IAudioDeviceEnumerator.cs" company="Sendspin Player">
// Licensed under the MIT License. See LICENSE file in the project root https://github.com/chrisuthe/sendspin-player.
// </copyright>
using System.Collections.Generic;
namespace Sendspin.Core.Audio;

public interface IAudioDeviceEnumerator
{
    IReadOnlyList<AudioDeviceInfo> GetDevices();
    AudioDeviceInfo? GetDefaultDevice();
}
