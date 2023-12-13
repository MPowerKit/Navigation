namespace MPowerKit.Navigation;

public static class ViewServiceProviderAttached
{
    #region ServiceScope
    public static readonly BindableProperty ServiceScopeProperty =
        BindableProperty.CreateAttached(
            "ServiceScope",
            typeof(IServiceScope),
            typeof(ViewServiceProviderAttached),
            null);

    public static IServiceScope GetServiceScope(BindableObject view) => (IServiceScope)view.GetValue(ServiceScopeProperty);

    public static void SetServiceScope(BindableObject view, IServiceScope value) => view.SetValue(ServiceScopeProperty, value);
    #endregion
}