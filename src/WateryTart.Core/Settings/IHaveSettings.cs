using IconPacks.Avalonia.Material;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Settings;

public interface IHaveSettings : IViewModelBase
{
    public PackIconMaterialKind Icon { get; }

    public string Description { get; }

}