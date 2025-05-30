using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Utilities;

namespace MPowerKit.Navigation.Behaviors;

public class PageLifecycleAwareBehavior : BehaviorBase<Page>
{
    protected override void OnAttachedTo(Page bindable)
    {
        base.OnAttachedTo(bindable);

        bindable.Appearing += Bindable_Appearing;
        bindable.Disappearing += Bindable_Disappearing;
    }

    protected override void OnDetachingFrom(Page bindable)
    {
        bindable.Appearing -= Bindable_Appearing;
        bindable.Disappearing -= Bindable_Disappearing;

        base.OnDetachingFrom(bindable);
    }

    private void Bindable_Appearing(object? sender, EventArgs e)
    {
        if (sender is not Page page) return;

        if (page.BindingContext is IPageLifecycleAware aware)
        {
            aware.OnAppearing();
        }

        if (MvvmHelpers.UsePageEventsInRegions)
        {
            var regionManager = ViewServiceProviderAttached.GetServiceScope(page)?.ServiceProvider.GetService<IRegionManager>();

            if (regionManager is not null)
            {
                foreach (var region in regionManager.GetRegions(page))
                {
                    region.OnPageLifecycleRecursively(true);
                }
            }
        }
    }

    private void Bindable_Disappearing(object? sender, EventArgs e)
    {
        if (sender is not Page page) return;

        if (MvvmHelpers.UsePageEventsInRegions)
        {
            var regionManager = ViewServiceProviderAttached.GetServiceScope(page)?.ServiceProvider.GetService<IRegionManager>();

            if (regionManager is not null)
            {
                foreach (var region in regionManager.GetRegions(page))
                {
                    region.OnPageLifecycleRecursively(false);
                }
            }
        }

        if (page.BindingContext is IPageLifecycleAware aware)
        {
            aware.OnDisappearing();
        }
    }
}