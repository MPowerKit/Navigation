namespace MPowerKit.Navigation.Interfaces;

public interface IRegionManager
{
    NavigationResult RequestNavigate(string regionName, string viewName, INavigationParameters? parameters = null);
    IEnumerable<IRegion> GetRegions(VisualElement? regionHolder);
}