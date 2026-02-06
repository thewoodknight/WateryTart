using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Windows.Input;
using WateryTart.Core.Extensions;
using WateryTart.Core.Messages;

namespace WateryTart.Core.ViewModels.Menus;

public partial class MenuItemViewModel : ReactiveObject, IViewModelBase
{
    private readonly ReactiveCommand<Unit, Unit> _clickedCommand;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; set; } = string.Empty;
    public string Icon { get; } = string.Empty;
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    [Reactive] public partial ICommand CoolDudeCommand { get; set; }

    public MenuItemViewModel(string title, string icon, ReactiveCommand<Unit, Unit> clickedCommand)
    {
        _clickedCommand = clickedCommand;
        Title = title;
        Icon = icon;

#pragma warning disable IL2026
        CoolDudeCommand = ReactiveCommand.Create(() =>
#pragma warning restore IL2026
        {
            _clickedCommand.ExecuteIfPossible().Subscribe(
                onNext: _ => { },
                onError: ex => Debug.WriteLine($"Error executing menu command '{title}': {ex.Message}")
            );
            MessageBus.Current.SendMessage(new CloseMenuMessage());
        });
    }
}