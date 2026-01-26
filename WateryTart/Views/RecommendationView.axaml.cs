using ReactiveUI.Avalonia;
using WateryTart.ViewModels;

namespace WateryTart.Views;

public partial class RecommendationView : ReactiveUserControl<RecommendationViewModel>
{
    public RecommendationView()
    {
        InitializeComponent();
    }
}