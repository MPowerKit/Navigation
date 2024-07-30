using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Utilities;

namespace MPowerKit.Navigation.WindowInfrastructure;

public class MPowerKitWindow : Window, IMPowerKitWindow, IWindow
{
    public Page? CurrentPage => Page is null ? null : MvvmHelpers.GetTopMostPage(Page);

    bool IWindow.BackButtonClicked()
    {
        return OnBackButtonClicked();
    }

    protected virtual bool OnBackButtonClicked()
    {
        if (Page is null) return false;

        var page = Navigation.ModalStack.Count > 0
            ? Navigation.ModalStack[^1]
            : Page;

        return MvvmHelpers.HandleSystemBackButtonClickRecursively(page);
    }

    protected override void OnResumed()
    {
        MvvmHelpers.OnWindowLifecycleWithModalStack(this, true);
    }

    protected override void OnStopped()
    {
        MvvmHelpers.OnWindowLifecycleWithModalStack(this, false);
    }
}