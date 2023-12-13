using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Regions;

public static class BuilderExtensions
{
    public static MauiAppBuilder UseMPowerKitRegions(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<IRegionManager, RegionManager>();
        mauiAppBuilder.Services.AddScoped<IRegion, Region>();
        mauiAppBuilder.Services.AddScoped<IRegionAccessor, RegionAccessor>();

        return mauiAppBuilder;
    }
}