using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Awares;

public interface IInitializeAsyncAware
{
    Task InitializeAsync(INavigationParameters parameters);
}