using ReactiveUI;

namespace WateryTart.Core.ViewModels;

public interface IViewModelBase : IRoutableViewModel
{
    string Title { get; set; }

    bool ShowMiniPlayer { get; }

    bool ShowNavigation { get; }
}