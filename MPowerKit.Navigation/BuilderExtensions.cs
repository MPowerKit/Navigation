using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation;

public static class BuilderExtensions
{
    public static MauiAppBuilder UseMPowerKit(this MauiAppBuilder mauiAppBuilder, Action<MPowerKitMvvmBuilder> configureNavigation)
    {
        var navBuilder = new MPowerKitMvvmBuilder(mauiAppBuilder);

        configureNavigation(navBuilder);

        return mauiAppBuilder;
    }

    public static MPowerKitMvvmBuilder ConfigureServices(this MPowerKitMvvmBuilder builder, Action<IServiceCollection> action)
    {
        action?.Invoke(builder.Services);
        return builder;
    }

    public static MPowerKitMvvmBuilder OnInitialized(this MPowerKitMvvmBuilder builder, Action action)
    {
        return builder.OnInitialized(_ => action());
    }

    public static MPowerKitMvvmBuilder OnAppStart(this MPowerKitMvvmBuilder builder, string uri)
    {
        return builder.OnAppStart(async navigation => await navigation.NavigateAsync(uri));
    }

    public static MPowerKitMvvmBuilder OnAppStart(this MPowerKitMvvmBuilder builder, Func<INavigationService, ValueTask> onAppStarted)
    {
        return builder.OnAppStart((_, n) => onAppStarted(n));
    }

    public static MPowerKitMvvmBuilder OnAppStart(this MPowerKitMvvmBuilder builder, Func<IServiceProvider, INavigationService, ValueTask> onAppStarted) =>
       builder.OnAppStart(onAppStarted);

    public static MPowerKitMvvmBuilder OnAppStart(this MPowerKitMvvmBuilder builder, string uri, Action<Exception> onError)
    {
        return builder.OnAppStart(async navigation =>
        {
            var result = await navigation.NavigateAsync(uri);
            if (result.Exception is not null)
                onError(result.Exception);
        });
    }
}