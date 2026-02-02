using System;
using Sendspin.SDK.Audio;
using WateryTart.Core.Playback;
using WateryTart.Platform.macOS.Playback;

namespace WateryTart.Platform.macOS;

public class MacOSAudioPlayerFactory : IPlayerFactory
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
