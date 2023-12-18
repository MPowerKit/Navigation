using MPowerKit.Navigation.Popups;
using MPowerKit.Popups;

namespace MPowerKit.Navigation.Interfaces;

public interface IPopupNavigationService
{
    ValueTask<PopupResult> ShowAwaitablePopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true);
    ValueTask<NavigationResult> ShowPopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true, Action<Confirmation>? closeAction = null);
    ValueTask<NavigationResult> HidePopupAsync(bool animated = true);
    ValueTask<NavigationResult> HidePopupAsync(PopupPage page, bool animated = true);
}