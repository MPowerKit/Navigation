namespace MPowerKit.Navigation.Interfaces;

public interface IRegion
{
    NavigationResult ReplaceAll(string viewName, INavigationParameters? parameters);
    NavigationResult Push(string viewName, INavigationParameters? parameters);
    NavigationResult GoBack(INavigationParameters? parameters);
    NavigationResult GoForward(INavigationParameters? parameters);
    bool CanGoBack();
    bool CanGoForward();

    void NavigatedRecursively(INavigationParameters parameters, bool to);
    void DestroyAll();
    void DestroyRecursively(VisualElement view);
    void OnWindowLifecycleRecursively(bool resume);
    void OnPageLifecycleRecursively(bool appearing);
}