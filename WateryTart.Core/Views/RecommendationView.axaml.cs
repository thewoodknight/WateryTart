using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class RecommendationView : ReactiveUserControl<RecommendationViewModel>
{
    public RecommendationView()
    {
        InitializeComponent();
    }
}