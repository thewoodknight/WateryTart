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

    public Control Build(object data)
    {
        if (Templates.ContainsKey(data.GetType().Name))
            return Templates[data.GetType().Name].Build(data);
        return null;
    }

    public bool Match(object data)
    {
        return data is IViewModelBase;
    }
}