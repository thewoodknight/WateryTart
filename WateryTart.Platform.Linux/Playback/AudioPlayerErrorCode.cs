// <copyright file="AudioPlayerErrorCode.cs" company="Sendspin Player">
// Licensed under the MIT License. See LICENSE file in the project root https://github.com/chrisuthe/sendspin-player.
// </copyright>

namespace Sendspin.Core.Audio;
public enum AudioPlayerErrorCode
{
    Unknown,
    DeviceInitializationFailed,
    DeviceNotFound,
    FormatNotSupported,
    BufferUnderrun,
    BufferOverflow,
    DeviceLost
}
