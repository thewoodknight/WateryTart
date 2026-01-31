using System;
using Sendspin.Platform.Linux.Audio;
using Sendspin.SDK.Audio;
using WateryTart.Core.Playback;

namespace WateryTart.Platform.Linux;

public class LinuxAudioPlayerFactory : IPlayerFactory
{
    Func<IAudioPlayer> IPlayerFactory.CreatePlayer
    {
        get
        {
            IAudioPlayer PlayerFactory() => new OpenALAudioPlayer();
            return PlayerFactory;
        }
    }
}