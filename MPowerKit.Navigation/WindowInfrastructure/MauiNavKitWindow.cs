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

        if (Navigation.ModalStack.Count > 0)
        {
            return MvvmHelpers.HandleSystemBackButtonClickRecursively(Navigation.ModalStack[^1]);
        }

        return MvvmHelpers.HandleSystemBackButtonClickRecursively(Page);
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