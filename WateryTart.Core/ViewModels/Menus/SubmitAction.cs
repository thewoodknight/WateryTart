using System.Windows.Input;
using Avalonia.Xaml.Interactivity;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels.Menus;

[GenerateTypedInvokeCommandAction]
public partial class SubmitAction : StyledElementAction
{
    [ActionCommand]
    private ICommand? _command;

    [ActionParameter]
    private string? _commandParameter;
}