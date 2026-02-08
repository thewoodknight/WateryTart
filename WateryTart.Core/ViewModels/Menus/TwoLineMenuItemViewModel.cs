using System.Windows.Input;
using Material.Icons;
using ReactiveUI;
using WateryTart.Core.Messages;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels.Menus;

public partial class TwoLineMenuItemViewModel : ReactiveObject, ISmallViewModelBase, IMenuItemViewModel
{
    public bool Indented { get; }
    private readonly ICommand _clickedCommand;

    private readonly MaterialIconKind _icon;
    private readonly string _title;
    private readonly string _subTitle;
    public MaterialIconKind Icon => _icon;

    public string Title => _title;
    public string SubTitle => _subTitle;
    public TwoLineMenuItemViewModel(string title, string subTitle, MaterialIconKind? icon, ICommand clickedCommand, bool indented = false)
    {
        Indented = indented;
        _title = title;
        _subTitle = subTitle;
        if (icon != null)
            _icon = (MaterialIconKind)icon;
        _clickedCommand = clickedCommand;
    }

    [GenerateTypedAction]
    public void TwoLineMenuItemClicked()
    {
        _clickedCommand.Execute(null);
        MessageBus.Current.SendMessage(new CloseMenuMessage());
    }
}