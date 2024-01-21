using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Utilities;

namespace MPowerKit.Navigation.Behaviors;

public class TabbedPageActiveTabAwareBehavior : BehaviorBase<TabbedPage>
{
    protected override void OnAttachedTo(TabbedPage bindable)
    {
        base.OnAttachedTo(bindable);

        bindable.CurrentPageChanged += Bindable_CurrentPageChanged;
    }

    protected override void OnDetachingFrom(TabbedPage bindable)
    {
        bindable.CurrentPageChanged -= Bindable_CurrentPageChanged;

        base.OnDetachingFrom(bindable);
    }

    private void Bindable_CurrentPageChanged(object? sender, EventArgs e)
    {
        if (sender is not TabbedPage tp) return;

        for (int i = 0; i < tp.Children.Count; i++)
        {
            var active = tp.CurrentPage == tp.Children[i];

            MvvmHelpers.InvokeViewAndViewModelAction<IActiveTabAware>(tp.Children[i], a => a.IsOnActiveTab = active);

            if (tp.Children[i] is not NavigationPage navPage) continue;

            MvvmHelpers.InvokeViewAndViewModelAction<IActiveTabAware>(navPage.RootPage, a => a.IsOnActiveTab = active);

            if (navPage.RootPage == navPage.CurrentPage) continue;

            MvvmHelpers.InvokeViewAndViewModelAction<IActiveTabAware>(navPage.CurrentPage, a => a.IsOnActiveTab = active);
        }
    }
}