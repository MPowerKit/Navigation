using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;

namespace Sample;

public partial class NewContent1 : ContentView, IInitializeAware, INavigationAware, ILoadedAsyncAware
{
    private readonly IRegionManager _regionManager;

    public NewContent1(IRegionManager regionManager)
    {
        InitializeComponent();
        _regionManager = regionManager;
    }

    public void Initialize(INavigationParameters parameters)
    {
        Console.WriteLine("Initialize NewContent1");
    }

    public async Task OnLoadedAsync(INavigationParameters navigationParameters)
    {
        await _regionManager.NavigateTo("NewRegion", "NewContent2");
    }

    public void OnNavigatedFrom(INavigationParameters navigationParameters)
    {
        Console.WriteLine("OnNavigatedFrom NewContent1");
    }

    public void OnNavigatedTo(INavigationParameters navigationParameters)
    {
        Console.WriteLine("OnNavigatedTo NewContent1");
    }
}