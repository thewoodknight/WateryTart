using System;
using Sendspin.SDK.Audio;
using WateryTart.Core.Playback;
using WateryTart.Platform.Windows.Playback;

namespace WateryTart.Platform.Windows;

public class WindowsAudioPlayerFactory : IPlayerFactory
{
    Func<IAudioPlayer> IPlayerFactory.CreatePlayer
    {
        get
        {
            IAudioPlayer PlayerFactory() => new SimpleWasapiPlayer();
            return PlayerFactory;
        }
    }
}