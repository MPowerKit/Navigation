using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Awares;

public interface INavigationAware
{
    void OnNavigatedTo(INavigationParameters navigationParameters);
    void OnNavigatedFrom(INavigationParameters navigationParameters);
}