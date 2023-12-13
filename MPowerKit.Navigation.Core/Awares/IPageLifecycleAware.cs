namespace MPowerKit.Navigation.Awares;

public interface IPageLifecycleAware
{
    void OnAppearing();
    void OnDisappearing();
}