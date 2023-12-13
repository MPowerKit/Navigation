namespace MPowerKit.Navigation.Interfaces;

public interface IRegionAccessor
{
    ContentView? RegionHolder { get; set; }
    string? RegionName { get; set; }
}