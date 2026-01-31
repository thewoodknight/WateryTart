using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels.Players
{
    public partial class BigPlayerViewModel : ReactiveObject, IViewModelBase, IReaper
    {
        public string? UrlPathSegment { get; } = "BigPlayerViewModel";
        public IScreen HostScreen { get; }
        [Reactive] public partial string Title { get; set; }
        public ReactiveCommand<Unit, Unit> PlayNextCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayerPlayPauseCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayerPreviousCommand { get; }
        [Reactive] public partial IPlayersService PlayersService { get; set; }
        public bool ShowMiniPlayer { get => false; }
        public bool ShowNavigation => true;

        private DispatcherTimer _timer;

        public BigPlayerViewModel(IPlayersService playersService)
        {
            PlayersService = playersService;

            PlayNextCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerNext());
            PlayerPlayPauseCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerPlayPause());
            PlayerPreviousCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerPrevious());

            /*  _timer = new DispatcherTimer();
              _timer.Interval = new System.TimeSpan(0, 0, 1);
              _timer.Tick += T_Tick;
              _timer.Start();*/
        }

        private void T_Tick(object? sender, System.EventArgs e)
        {
            if (PlayersService.SelectedPlayer?.PlaybackState == PlaybackState.playing)
                PlayersService.SelectedPlayer?.CurrentMedia.elapsed_time += 1;
        }

        public void Reap()
        {
            _timer?.Stop();
            _timer = null;
        }
    }
}


