using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Awares;

public interface IInitializeAware
{
    void Initialize(INavigationParameters parameters);
}