using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core;

public class WateryTartSmallViewSelector : IDataTemplate
{
    public bool SupportsRecycling => false;
    [Content]
    public Dictionary<string, IDataTemplate> Templates { get; } = new Dictionary<string, IDataTemplate>();

    public Control? Build(object? data)
    {
        var type = data?.GetType();
        var name = data?.GetType().Name;
        if (name != null && type != null && Templates.ContainsKey(name))
            return Templates[type.Name].Build(data);

        return null;
    }

    public bool Match(object? data)
    {
        return data is IViewModelBase;
    }
}