using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Models.Enums;

namespace WateryTart.Core.ViewModels
{
    public partial class PlayerViewModel(Player player) : ReactiveObject
    {
        private readonly Player _player = player;

        [Reactive] public partial PlaybackState State { get; set; }
    }
}