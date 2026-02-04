using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AsyncImageLoader;
using ReactiveUI;
using System;
using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels.Players;

namespace WateryTart.Core.Views.Players;

public partial class BigPlayerView : ReactiveUserControl<BigPlayerViewModel>
{
    public BigPlayerView()
    {
        AvaloniaXamlLoader.Load(this);

        var advancedImage = this.Find<AdvancedImage>("imgAlbumArt");
        if (advancedImage != null)
        {
            advancedImage.PropertyChanged += (s, e) =>
            {
                if (e.Property == AdvancedImage.CurrentImageProperty && DataContext is BigPlayerViewModel vm)
                {
                    if (advancedImage.CurrentImage != null && 
                        advancedImage.Width > 0 && 
                        advancedImage.Height > 0)
                    {
                        vm.UpdateCachedDimensions(advancedImage.Width, advancedImage.Height);
                    }
                }
            };      
        }

        var advancedImageSmall = this.Find<AdvancedImage>("imgAlbumArtSmall");
        if (advancedImageSmall != null)
        {
            advancedImageSmall.PropertyChanged += (s, e) =>
            {
                if (e.Property == AdvancedImage.CurrentImageProperty && DataContext is BigPlayerViewModel vm)
                {
                    if (advancedImageSmall.CurrentImage != null && 
                        advancedImageSmall.Width > 0 && 
                        advancedImageSmall.Height > 0)
                    {
                        vm.UpdateCachedDimensions(advancedImageSmall.Width, advancedImageSmall.Height);
                    }
                }
            };
        }
    }
}