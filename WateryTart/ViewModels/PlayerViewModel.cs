using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WateryTart.MassClient.Models;

namespace WateryTart.ViewModels
{
    public partial class PlayerViewModel(Player player) : ReactiveObject
    {
        private readonly Player _player = player;

        [Reactive] public partial PlaybackState State { get; set; }
    }
}
