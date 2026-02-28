using System;
using Microsoft.Extensions.Logging;
using Sendspin.SDK.Audio;
using WateryTart.Core.Playback;
using WateryTart.Platform.Windows.Playback;

namespace WateryTart.Platform.Windows;

public class WindowsAudioPlayerFactory : IPlayerFactory
{
    private readonly Func<IAudioPlayer> _create;
    public WindowsAudioPlayerFactory(ILoggerFactory factory) : this(() => new SoundflowPlayer(factory)) { }

    public WindowsAudioPlayerFactory(Func<IAudioPlayer> create)
    {
        _create = create ?? throw new ArgumentNullException(nameof(create));
    }

    Func<IAudioPlayer> IPlayerFactory.CreatePlayer => () =>
    {
        var p = _create();
        return p;
    };
}