using Material.Icons;
using ReactiveUI;

namespace WateryTart.Core.ViewModels;

public interface ISmallViewModelBase
{
    string Title { get;}
    MaterialIconKind Icon { get; }
}
public interface IViewModelBase : IRoutableViewModel
{
    string Title { get; }

    bool ShowMiniPlayer { get; }

    bool ShowNavigation { get; }
}