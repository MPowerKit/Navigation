using Microsoft.Extensions.DependencyInjection.Extensions;

using MPowerKit.Navigation.Behaviors;
using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Utilities;

public static class BehaviorExtensions
{
    public static IServiceCollection RegisterBehaviorFactory(this IServiceCollection services, Action<VisualElement> pageBehaviorFactory)
    {
        return services.AddSingleton<IBehaviorFactory>(c => new DelegateBehaviorFactory(pageBehaviorFactory));
    }

    public static IServiceCollection RegisterBehaviorFactory(this IServiceCollection services, Action<IServiceProvider, VisualElement> pageBehaviorFactory)
    {
        return services.AddScoped<IBehaviorFactory>(c => new DelegateContainerBehaviorFactory(pageBehaviorFactory, c));
    }

    public static IServiceCollection RegisterBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : Behavior
    {
        services
            .RegisterBehaviorFactory(static (c, p) => p.Behaviors.Add(c.GetService<TBehavior>()))
            .TryAddTransient<TBehavior>();
        return services;
    }

    public static IServiceCollection RegisterBehavior<TElement, TBehavior>(this IServiceCollection services)
        where TElement : VisualElement
        where TBehavior : Behavior
    {
        services
            .RegisterBehaviorFactory(static (c, p) =>
            {
                if (p is TElement)
                    p.Behaviors.Add(c.GetService<TBehavior>());
            })
            .TryAddTransient<TBehavior>();

        return services;
    }

    public static void ApplyPageBehaviors(IServiceProvider container, Page page)
    {
        ArgumentNullException.ThrowIfNull(page, nameof(page));

        if (page is TabbedPage tabbed)
        {
            foreach (var child in tabbed.Children)
            {
                ApplyPageBehaviors(container, child);
            }
        }
        else if (page is FlyoutPage flyout)
        {
            ApplyPageBehaviors(container, flyout.Flyout);
            ApplyPageBehaviors(container, flyout.Detail);
        }
        else if (page is NavigationPage navPage && navPage.RootPage is not null)
        {
            ApplyPageBehaviors(container, navPage.RootPage);
        }

        ApplyBehaviors(container, page);
    }

    public static void ApplyBehaviors(IServiceProvider container, VisualElement view)
    {
        ArgumentNullException.ThrowIfNull(view, nameof(view));

        var behaviorFactories = container.GetServices<IBehaviorFactory>();
        foreach (var factory in behaviorFactories)
        {
            factory.ApplyBehaviors(view);
        }
    }
}