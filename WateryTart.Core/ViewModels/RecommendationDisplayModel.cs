using System.Collections.Generic;
using System.Collections.ObjectModel;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public class RecommendationDisplayModel
{
    private readonly Recommendation _recommendation;

    public string Title => _recommendation.Name;
    public string Path => _recommendation.path;
    public object Image => _recommendation.image;
    public string Icon => _recommendation.icon;
    public string Subtitle => _recommendation.subtitle;
    public string ItemId => _recommendation.ItemId;
    public string Provider => _recommendation.Provider;

    public ObservableCollection<object> Items { get; set; }

    public RecommendationDisplayModel(Recommendation recommendation, IEnumerable<object> viewModels)
    {
        _recommendation = recommendation;
        Items = new ObservableCollection<object>(viewModels);
    }

    public Recommendation GetOriginalRecommendation() => _recommendation;
}   