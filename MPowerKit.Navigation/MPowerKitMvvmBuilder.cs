using MPowerKit.Navigation.Behaviors;
using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Utilities;
using MPowerKit.Navigation.WindowInfrastructure;

namespace MPowerKit.Navigation;

public class MPowerKitInitializationService : IMauiInitializeService
{
    /// <summary>
    /// Initializes the modules.
    /// </summary>
    public void Initialize(IServiceProvider services)
    {
        var builder = services.GetRequiredService<MPowerKitMvvmBuilder>();
        builder.OnInitialized(services);
    }
}

public class MPowerKitMvvmBuilder
{
    protected List<Action<IServiceProvider>> Initializations { get; } = [];
    protected bool Initialized { get; set; }
    public IServiceCollection Services { get; }
    protected Func<IServiceProvider, INavigationService, ValueTask>? OnAppStartedAction;

    public MPowerKitMvvmBuilder(MauiAppBuilder mauiAppBuilder)
    {
        ThreadUtil.Init(SynchronizationContext.Current!);

#if IOS || MACCATALYST
        mauiAppBuilder.ConfigureMauiHandlers(h =>
        {
            h.AddHandler<NavigationPage, MPowerKit.Navigation.PlatfromSpecific.MPowerKitNavigtaionRenderer>();
        });
#endif

        Services = mauiAppBuilder.Services;
        Services.AddSingleton(this);
        Services.AddSingleton<IMauiInitializeService, MPowerKitInitializationService>();
        Services.AddSingleton<IWindowCreator, MPowerKitWindowCreator>();
        Services.AddSingleton<IWindowManager, MPowerKitWindowManager>();
        Services.AddTransient<IMPowerKitWindow, MPowerKitWindow>();
        Services.AddScoped<INavigationService, NavigationService>();
        Services.AddScoped<IPageAccessor, PageAccessor>();

        //Pages
        Services.RegisterBehavior<Page, PageLifecycleAwareBehavior>();
        Services.RegisterBehavior<TabbedPage, TabbedPageActiveTabAwareBehavior>();
        Services.RegisterBehavior<FlyoutPage, FlyoutPageFlyoutPresentedAwareBehavior>();
        Services.RegisterForNavigation<NavigationPage>();
        Services.RegisterForNavigation<TabNavigationPage>();
        Services.RegisterForNavigation<TabbedPage>();
        Services.RegisterForNavigation<FlyoutPage>();
    }

    public virtual MPowerKitMvvmBuilder OnInitialized(Action<IServiceProvider> action)
    {
        Initializations.Add(action);
        return this;
    }

    public virtual MPowerKitMvvmBuilder UsePageEventsInRegions()
    {
        MvvmHelpers.UsePageEventsInRegions = true;
        return this;
    }

    public virtual void OnInitialized(IServiceProvider provider)
    {
        if (Initialized)
            return;

        Initialized = true;
        Initializations.ForEach(action => action(provider));
    }

    public virtual void OnAppStarted(IServiceProvider provider)
    {
        if (OnAppStartedAction is null)
            throw new ArgumentException("You must call OnAppStart on the NavigationBuilder.");

        // Ensure that this is executed before we navigate.
        OnInitialized(provider);

        ThreadUtil.RunOnUIThread(() =>
        {
            var navService = provider.GetRequiredService<INavigationService>();

            navService.CreateInitialWindow();

            return OnAppStartedAction(provider, navService);
        });
    }

    public virtual MPowerKitMvvmBuilder OnAppStart(Func<IServiceProvider, INavigationService, ValueTask> onAppStarted)
    {
        OnAppStartedAction = onAppStarted;
        return this;
    }
}