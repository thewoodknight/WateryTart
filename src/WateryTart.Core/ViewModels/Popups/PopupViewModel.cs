using Material.Icons;
using ReactiveUI;

namespace WateryTart.Core.ViewModels.Popups;
public class PopupViewModel : ReactiveObject, IPopupViewModel
{
    public string Message { get; }
    public string? Title { get; }

    public PopupViewModel(string message, string? title = null)
    {
        Message = message;
        Title = title;
    }
}