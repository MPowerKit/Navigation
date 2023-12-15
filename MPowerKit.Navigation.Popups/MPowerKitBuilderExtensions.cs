using MPowerKit.Navigation.Interfaces;
using MPowerKit.Popups;

namespace MPowerKit.Navigation.Popups;

public static class MPowerKitBuilderExtensions
{
    public static MPowerKitMvvmBuilder UsePopupNavigation(this MPowerKitMvvmBuilder builder)
    {
        builder.Services.AddScoped<IPopupNavigationService, PopupNavigationService>();
        builder.Services.AddSingleton(PopupService.Current);
        builder.Services.AddTransient<IMPowerKitWindow, MPowerKitPopupsWindow>();

        return builder;
    }
}