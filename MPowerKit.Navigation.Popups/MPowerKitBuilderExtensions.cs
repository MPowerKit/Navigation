using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Popups;

public static class MPowerKitBuilderExtensions
{
    public static MPowerKitMvvmBuilder UsePopupNavigation(this MPowerKitMvvmBuilder builder)
    {
        builder.Services.AddScoped<IPopupNavigationService, PopupNavigationService>();
        builder.Services.AddSingleton<INavigationPopupService, NavigationPopupService>();
        builder.Services.AddTransient<IMPowerKitWindow, MPowerKitPopupsWindow>();

        return builder;
    }
}