using Sendspin.SDK.Audio;
using System;

namespace WateryTart.Core.Playback;

public interface IPlayerFactory
{
    Func<IAudioPlayer> CreatePlayer { get; }
}
