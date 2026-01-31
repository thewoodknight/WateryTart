using Avalonia.Media;
using System.Threading.Tasks;


namespace WateryTart.Core.Services
{
    public interface IColourService
    {
        Color ColourA { get; set; }
        Color ColourB { get; set; }
        string LastId { get; set; }

        Task Update(string id, string url);
    }
}