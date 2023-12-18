using MPowerKit.Navigation.Popups;
using MPowerKit.Popups;

namespace MPowerKit.Navigation.Interfaces;

public interface IPopupNavigationService
{
    Task<PopupResult> ShowAwaitablePopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true);
    Task<NavigationResult> ShowPopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true, Action<Confirmation>? closeAction = null);
    Task<NavigationResult> HidePopupAsync(bool animated = true);
    Task<NavigationResult> HidePopupAsync(PopupPage page, bool animated = true);
}