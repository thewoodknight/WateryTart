// <copyright file="AudioDeviceInfo.cs" company="Sendspin Player">
// Licensed under the MIT License. See LICENSE file in the project root https://github.com/chrisuthe/sendspin-player.
// </copyright>

namespace Sendspin.Core.Audio;
public sealed class AudioDeviceInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public bool IsDefault { get; init; }
}
