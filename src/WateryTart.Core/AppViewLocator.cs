using ReactiveUI;

namespace WateryTart.Core
{
    public class AppViewLocator : IViewLocator
    {
        public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
        {
            if (viewModel != null)
                if (ViewLocator._viewFactories.TryGetValue(viewModel.GetType(), out var factory))
                {
                    return (IViewFor)factory();
                }

            return null;
        }
    }
}