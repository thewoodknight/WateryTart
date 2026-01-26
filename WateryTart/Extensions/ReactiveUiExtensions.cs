using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace WateryTart.Extensions;

public static class ReactiveUiExtensions
{
    public static IObservable<bool> ExecuteIfPossible<TParam, TResult>(this ReactiveCommand<TParam, TResult> cmd) =>
        cmd.CanExecute.FirstAsync().Where(can => can).Do(async _ => await cmd.Execute());

    public static bool GetCanExecute<TParam, TResult>(this ReactiveCommand<TParam, TResult> cmd) =>
        cmd.CanExecute.FirstAsync().Wait();
}