using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive;
using WateryTart.Extensions;
using WateryTart.Messages;

namespace WateryTart.ViewModels;

public partial class MenuItemViewModel : ReactiveObject, IViewModelBase
{
    private readonly ReactiveCommand<Unit, Unit> _clickedCommand;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; set; }
    public string Icon { get; }

    [Reactive] public partial ReactiveCommand<Unit, Unit> ClickedCommand { get; set; }

    public MenuItemViewModel(string title, string icon, ReactiveCommand<Unit, Unit> clickedCommand)
    {
        _clickedCommand = clickedCommand;
        Title = title;
        Icon = icon;
        ClickedCommand = ReactiveCommand.Create<Unit>(_ =>
        {
            _clickedCommand.ExecuteIfPossible().Subscribe();
            MessageBus.Current.SendMessage(new CloseMenuMessage());
        });
    }
}