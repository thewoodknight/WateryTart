using ReactiveUI;
using System.Reactive;


namespace WateryTart.Core.ViewModels;

public class LibraryItem : ReactiveObject
{
    private int _count;

    public string Title { get; set; }
    public ReactiveCommand<Unit, Unit> ClickedCommand { get; set; }
    public string LowerTitle { get { return Title.ToLowerInvariant(); } }
    public int Count
    {
        get => _count;
        set => this.RaiseAndSetIfChanged(ref _count, value);
    }
}
