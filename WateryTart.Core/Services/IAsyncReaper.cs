using System.Threading.Tasks;

namespace WateryTart.Core.Services;

public interface IAsyncReaper : IReaper
{
    Task ReapAsync();
}