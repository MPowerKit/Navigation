namespace MPowerKit.Navigation.Interfaces;

public interface IRegionManager
{
    ValueTask<NavigationResult> NavigateTo(string regionName, string viewName, INavigationParameters? parameters = null);
    IEnumerable<IRegion> GetRegions(VisualElement? regionHolder, bool onlyDirectDescendants = true);
}