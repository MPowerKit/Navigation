using MPowerKit.Navigation;

namespace MPowerKit.Regions;

public class ViewRegionViewNameAttached
{
    #region RegionViewName
    public static readonly BindableProperty RegionViewNameProperty =
        BindableProperty.CreateAttached(
            "RegionViewName",
            typeof(string),
            typeof(ViewServiceProviderAttached),
            null);

    public static string GetRegionViewName(BindableObject view) => (string)view.GetValue(RegionViewNameProperty);

    public static void SetRegionViewName(BindableObject view, string value) => view.SetValue(RegionViewNameProperty, value);
    #endregion
}