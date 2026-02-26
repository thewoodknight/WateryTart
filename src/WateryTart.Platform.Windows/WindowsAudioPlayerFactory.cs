using System;
using Sendspin.SDK.Audio;
using WateryTart.Core.Playback;
using WateryTart.Platform.Windows.Playback;
using System.Diagnostics;

namespace WateryTart.Platform.Windows;

public class WindowsAudioPlayerFactory : IPlayerFactory
{
    private readonly Func<IAudioPlayer> _create;

    public WindowsAudioPlayerFactory() : this(() => new SimpleWasapiPlayer()) { }

    public WindowsAudioPlayerFactory(Func<IAudioPlayer> create)
    {
        Debug.WriteLine($"WindowsAudioPlayerFactory.ctor this={GetHashCode()}");
        _create = create ?? throw new ArgumentNullException(nameof(create));
    }

    Func<IAudioPlayer> IPlayerFactory.CreatePlayer => () =>
    {
        var p = _create();
        Debug.WriteLine($"IPlayerFactory.CreatePlayer returned type={(p?.GetType().FullName ?? "null")} hash={(p?.GetHashCode().ToString() ?? "null")}");
        return p;
    };
}