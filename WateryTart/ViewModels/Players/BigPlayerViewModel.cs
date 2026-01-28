using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using WateryTart.Services;

namespace WateryTart.ViewModels.Players
{
    public partial class BigPlayerViewModel : ReactiveObject, IViewModelBase
    {
        public string? UrlPathSegment { get; } = "BigPlayerViewModel";
        public IScreen HostScreen { get; }
        [Reactive] public partial string Title { get; set; }
        public ReactiveCommand<Unit, Unit> PlayNextCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayerPlayPauseCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayerPreviousCommand { get; }
        [Reactive] public partial IPlayersService PlayersService { get; set; }
        public bool ShowMiniPlayer { get => false; }
        public BigPlayerViewModel(IPlayersService playersService)
        {
            PlayersService = playersService;

            PlayNextCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerNext());
            PlayerPlayPauseCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerPlayPause());
            PlayerPreviousCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerPrevious());

            DispatcherTimer t = new DispatcherTimer();
            t.Interval = new System.TimeSpan(0, 0, 1);
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object? sender, System.EventArgs e)
        {
            if (PlayersService.SelectedPlayer?.PlaybackState == MassClient.Models.PlaybackState.playing)
                PlayersService.SelectedPlayer?.CurrentMedia.elapsed_time += 1;
        }
    }
}


