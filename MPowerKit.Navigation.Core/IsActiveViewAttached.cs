namespace MPowerKit.Navigation;

public class IsActiveViewAttached
{
    #region IsActiveView
    public static readonly BindableProperty IsActiveViewProperty =
        BindableProperty.CreateAttached(
            "IsActiveView",
            typeof(bool),
            typeof(IsActiveViewAttached),
            false);

    public static bool GetIsActiveView(BindableObject view) => (bool)view.GetValue(IsActiveViewProperty);

    public static void SetIsActiveView(BindableObject view, bool value) => view.SetValue(IsActiveViewProperty, value);
    #endregion
}