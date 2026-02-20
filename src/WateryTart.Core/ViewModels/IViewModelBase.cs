using IconPacks.Avalonia.Material;
using ReactiveUI;

namespace WateryTart.Core.ViewModels;

public interface ISmallViewModelBase
{
    string Title { get;}
    PackIconMaterialKind Icon { get; }
}
public interface IViewModelBase : IRoutableViewModel
{
    string Title { get; }

    bool ShowMiniPlayer { get; }

    bool ShowNavigation { get; }
    bool IsLoading { get; set; }
}