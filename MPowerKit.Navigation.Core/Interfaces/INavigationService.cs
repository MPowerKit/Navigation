namespace MPowerKit.Navigation.Interfaces;

public interface INavigationService
{
    Guid CurrentWindowId { get; }
    Window? Window { get; }

    NavigationResult CloseWindow(Guid? windowId = null);
    ValueTask<NavigationResult> OpenNewWindowAsync(string uri, INavigationParameters? parameters = null);
    NavigationResult ToggleFlyout(bool isPresented);
    NavigationResult SelectTab(string tabName, INavigationParameters? parameters);
    ValueTask<NavigationResult> GoBackAsync(INavigationParameters? parameters = null, bool modal = false, bool animated = true);
    ValueTask<NavigationResult> GoBackToRootAsync(INavigationParameters? parameters = null, bool animated = true);
    ValueTask<NavigationResult> NavigateAsync(string uri, INavigationParameters? navigationParameters = null, bool modal = false, bool animated = true);
}