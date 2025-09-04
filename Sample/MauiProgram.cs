using MPowerKit.Navigation;
using MPowerKit.Navigation.Popups;
using MPowerKit.Navigation.Utilities;
using MPowerKit.Regions;

namespace Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMPowerKitNavigation(b =>
            {
                b.ConfigureServices(s =>
                {
                    s.RegisterForNavigation<MainPage>();
                    s.RegisterForNavigation<NewPage1, NewPageViewModel>();
                    s.RegisterForNavigation<NewContent1>();
                    s.RegisterForNavigation<NewContent2>();
                    s.RegisterForNavigation<PopupPageTest>();
                    s.RegisterForNavigation<FlyPage>();
                })
                .UsePopupNavigation()
                .UsePageEventsInRegions()
                .OnAppStart("/FlyPage/NavigationPage/MainPage");
            })
            .UseMPowerKitRegions()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}