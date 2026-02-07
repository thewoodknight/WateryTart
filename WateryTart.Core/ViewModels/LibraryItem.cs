using Avalonia.Xaml.Interactivity;
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;
using Xaml.Behaviors.SourceGenerators;


namespace WateryTart.Core.ViewModels;

[GenerateTypedInvokeCommandAction]
public partial class ClickAction : StyledElementAction
{
    [ActionCommand]
    private ICommand? _command;

    [ActionParameter]
    private string? _commandParameter;
}

public class LibraryItem : ReactiveObject
{
    public string Title { get; set; } = string.Empty;
    public ICommand? ClickedCommand { get; set; }
    public string LowerTitle => Title.ToLowerInvariant();

    public int Count
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}
