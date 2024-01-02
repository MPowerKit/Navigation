namespace MPowerKit.Navigation.Interfaces;

public interface IRegionManager
{
    NavigationResult NavigateTo(string regionName, string viewName, INavigationParameters? parameters = null);
    IEnumerable<IRegion> GetRegions(VisualElement? regionHolder);
}