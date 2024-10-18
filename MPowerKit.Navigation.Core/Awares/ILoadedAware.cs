using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Awares;

public interface ILoadedAware
{
    void OnLoaded(INavigationParameters navigationParameters);
}

public interface ILoadedAsyncAware
{
    Task OnLoadedAsync(INavigationParameters navigationParameters);
}