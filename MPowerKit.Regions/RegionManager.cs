using System.ComponentModel;

using MPowerKit.Navigation;
using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Utilities;

namespace MPowerKit.Regions;

public class RegionManager : IRegionManager
{
    protected IServiceProvider ServiceProvider { get; }

    public RegionManager(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual async ValueTask<NavigationResult> NavigateTo(string regionName, string viewName, INavigationParameters? parameters = null)
    {
        parameters ??= new NavigationParameters();

        try
        {
            if (!_regionHolders.TryGetValue(regionName, out WeakReference<ContentView>? value))
            {
                throw new ArgumentNullException($"There is not registered region with name {regionName}");
            }

            var regionHolder = (value.TryGetTarget(out var target) ? target : null)
                ?? throw new NullReferenceException("Region was disposed");

            var scope = ViewServiceProviderAttached.GetServiceScope(regionHolder);
            if (scope is null)
            {
                scope = ServiceProvider.CreateScope();
                var ra = scope.ServiceProvider.GetRequiredService<IRegionAccessor>();
                ra.RegionName = regionName;
                ra.RegionHolder = regionHolder;

                ViewServiceProviderAttached.SetServiceScope(regionHolder, scope);

                var parentPage = MvvmHelpers.GetParentOfTypeOrSelf<Page>(regionHolder);
                if (parentPage is not null)
                {
                    var pageAccessor = ViewServiceProviderAttached.GetServiceScope(parentPage)?.ServiceProvider?.GetService<IPageAccessor>();
                    if (pageAccessor is not null)
                    {
                        var pa = scope.ServiceProvider.GetService<IPageAccessor>();
                        if (pa is not null)
                        {
                            pa.Page = pageAccessor.Page;
                            pa.SegmentName = pageAccessor.SegmentName;
                        }
                    }
                }
            }

            var region = scope.ServiceProvider.GetRequiredService<IRegion>();

            return await region.ReplaceAll(viewName, parameters);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual IRegion? GetRegionByName(string regionName)
    {
        if (!_regionHolders.TryGetValue(regionName, out WeakReference<ContentView>? value))
        {
            throw new ArgumentNullException($"There is not registered region with name {regionName}");
        }

        var regionHolder = (value.TryGetTarget(out var target) ? target : null)
            ?? throw new NullReferenceException("Region was disposed");

        return ViewServiceProviderAttached.GetServiceScope(regionHolder)?.ServiceProvider.GetRequiredService<IRegion>();
    }

    public virtual IEnumerable<IRegion> GetRegions(VisualElement? regionHolder, bool onlyDirectDescendants = true)
    {
        var holders = RegionHolders.Where(v => MvvmHelpers.IsParentRegionHolder(v, regionHolder));

        if (onlyDirectDescendants)
        {
            var allHolders = holders.ToList();
            foreach (var holder in holders.Reverse())
            {
                if (allHolders.Any(h => MvvmHelpers.IsParentRegionHolder(holder, h)))
                {
                    allHolders.Remove(holder);
                }
            }

            holders = allHolders;
        }

        foreach (var holder in holders)
        {
            if (holder is null) continue;

            var region = ViewServiceProviderAttached.GetServiceScope(holder)?.ServiceProvider.GetRequiredService<IRegion>();
            if (region is null) continue;

            yield return region;
        }
    }

    private static readonly Dictionary<string, WeakReference<ContentView>> _regionHolders = [];

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IList<ContentView?> RegionHolders => _regionHolders.Values
        .Select(static w => w.TryGetTarget(out var target) ? target : null)
        .Where(static v => v is not null).ToList();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RemoveHolder(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var kvps = _regionHolders.Where(static kvp => !kvp.Value.TryGetTarget(out var _)).ToList();
        foreach (var kvp in kvps)
        {
            _regionHolders.Remove(kvp.Key);
        }

        if (!_regionHolders.ContainsKey(key)) return;

        _regionHolders.Remove(key);
    }

    private static void OnRegionNameChanged(BindableObject view, object oldValue, object newValue)
    {
        if (view is not ContentView cv || newValue is not string newKey || string.IsNullOrWhiteSpace(newKey)) return;

        var kvps = _regionHolders.Where(static kvp => !kvp.Value.TryGetTarget(out var _)).ToList();
        foreach (var kvp in kvps)
        {
            _regionHolders.Remove(kvp.Key);
        }

        var region = _regionHolders.FirstOrDefault(kvp => (kvp.Value.TryGetTarget(out var target) ? target : null) == cv);

        if (!string.IsNullOrWhiteSpace(region.Key))
        {
            _regionHolders.Remove(region.Key);
        }

        try
        {
            if (_regionHolders.ContainsKey(newKey) && _regionHolders[newKey].TryGetTarget(out var target))
            {
                throw new Exception();
            }

            _regionHolders[newKey] = new(cv);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Cannot register region with name {newKey}, because region with such name already exists");
        }
    }

    #region RegionName
    public static readonly BindableProperty RegionNameProperty =
        BindableProperty.CreateAttached(
            "RegionName",
            typeof(string),
            typeof(RegionManager),
            null,
            propertyChanged: OnRegionNameChanged);

    public static string GetRegionName(BindableObject view) => (string)view.GetValue(RegionNameProperty);

    public static void SetRegionName(BindableObject view, string value) => view.SetValue(RegionNameProperty, value);
    #endregion
}