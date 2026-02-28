using IconPacks.Avalonia.Material;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;
using WateryTart.Platform.Windows.Playback;

namespace WateryTart.Platform.Windows.ViewModels
{
    public partial class PlaybackSettingsViewModel : ViewModelBase<PlaybackSettingsViewModel>, IHaveSettings
    {
        private readonly SwitchableAudioPlayer _player;
        private readonly ISettings _settings;
        public string Description => "Select audio backend";
        public PackIconMaterialKind Icon => PackIconMaterialKind.MusicNote;
        public IEnumerable<PlaybackBackend> PlaybackBackendOptions { get; } = Enum.GetValues<PlaybackBackend>();

        public PlaybackBackend SelectedBackend
        {
            get => _settings.PlaybackBackend;
            set
            {
                if (_settings.PlaybackBackend != value)
                {
                    _settings.PlaybackBackend = value;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var backend = value == PlaybackBackend.SoundFlow ?
                                SwitchableAudioPlayer.PlayerBackend.SoundFlow :
                                SwitchableAudioPlayer.PlayerBackend.SimpleWasapi;

                            await _player.SwitchToAsync(backend, CancellationToken.None).ConfigureAwait(false);
                        }
                        catch
                        {
                        }
                    });
                }
            }
        }

        public new string Title => "Playback";
        public PlaybackSettingsViewModel(ISettings settings, SwitchableAudioPlayer player, ILoggerFactory factory)
            : base(factory)
        {
            _settings = settings;
            _player = player;
            SelectedBackend = _settings.PlaybackBackend;
        }
    }
}