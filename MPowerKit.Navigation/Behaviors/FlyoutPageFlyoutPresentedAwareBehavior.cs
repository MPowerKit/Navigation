using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Utilities;

namespace MPowerKit.Navigation.Behaviors;

public class FlyoutPageFlyoutPresentedAwareBehavior : BehaviorBase<FlyoutPage>
{
    protected override void OnAttachedTo(FlyoutPage bindable)
    {
        base.OnAttachedTo(bindable);

        bindable.IsPresentedChanged += Bindable_IsPresentedChanged;
    }

    protected override void OnDetachingFrom(FlyoutPage bindable)
    {
        bindable.IsPresentedChanged -= Bindable_IsPresentedChanged;

        base.OnDetachingFrom(bindable);
    }

    private void Bindable_IsPresentedChanged(object? sender, EventArgs e)
    {
        if (sender is not FlyoutPage fp) return;

        if (fp.BindingContext is IFlyoutPageFlyoutPresentedAware aware) aware.IsFlyoutPresented = fp.IsPresented;

        MvvmHelpers.InvokeViewAndViewModelAction<IFlyoutPageFlyoutPresentedAware>(fp.Flyout, a => a.IsFlyoutPresented = fp.IsPresented);

        MvvmHelpers.InvokeViewAndViewModelAction<IFlyoutPageFlyoutPresentedAware>(fp.Detail, a => a.IsFlyoutPresented = fp.IsPresented);

        if (fp.Detail is not NavigationPage navPage) return;

        MvvmHelpers.InvokeViewAndViewModelAction<IFlyoutPageFlyoutPresentedAware>(navPage.RootPage, a => a.IsFlyoutPresented = fp.IsPresented);

        if (navPage.RootPage == navPage.CurrentPage) return;

        MvvmHelpers.InvokeViewAndViewModelAction<IFlyoutPageFlyoutPresentedAware>(navPage.CurrentPage, a => a.IsFlyoutPresented = fp.IsPresented);
    }
}