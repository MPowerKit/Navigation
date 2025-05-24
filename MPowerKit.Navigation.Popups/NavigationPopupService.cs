using MPowerKit.Navigation.Interfaces;
using MPowerKit.Popups;

namespace MPowerKit.Navigation.Popups;

public partial class NavigationPopupService : PopupService, INavigationPopupService
{
#if ANDROID && NET9_0_OR_GREATER
    protected override void BackPressCallback(PopupPage page, global::Android.App.Activity activity)
    {
        activity.OnBackPressed();
    }
#endif
}