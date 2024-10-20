using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;

namespace Sample;

public partial class NewContent1 : ContentView, IInitializeAware, INavigationAware
{
    private readonly IRegionManager _regionManager;

    public NewContent1(IRegionManager regionManager)
    {
        InitializeComponent();
        _regionManager = regionManager;
    }

    public void Initialize(INavigationParameters parameters)
    {
        _regionManager.NavigateTo("NewRegion", "NewContent2");
    }

    public void OnNavigatedFrom(INavigationParameters navigationParameters)
    {

    }

    public void OnNavigatedTo(INavigationParameters navigationParameters)
    {

    }
}