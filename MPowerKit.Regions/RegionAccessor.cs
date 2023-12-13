using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Regions;

public class RegionAccessor : IRegionAccessor
{
    private WeakReference<ContentView?>? _weakPage;

    public ContentView? RegionHolder
    {
        get => _weakPage?.TryGetTarget(out var target) is true ? target : null;
        set => _weakPage = value is null ? null : new(value);
    }

    public string? RegionName { get; set; }
}