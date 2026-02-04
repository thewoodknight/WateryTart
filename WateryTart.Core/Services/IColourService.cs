using Avalonia.Media;
using System.Threading.Tasks;
using WateryTart.Core.Services;

namespace WateryTart.Core.Services
{
    public enum ColourChosen
    {
        AB,
        CD
    }
    public interface IColourService
    {
        Color ColourA { get; set; }
        Color ColourB { get; set; }

        Color ColourC { get; set; }
        Color ColourD { get; set; }

        ColourChosen LastPick { get; set; }

        string LastId { get; set; }

        Task Update(string id, string url);
    }
}