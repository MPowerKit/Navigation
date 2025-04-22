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

        MvvmHelpers.SetTabsActiveState(tp);
    }
}