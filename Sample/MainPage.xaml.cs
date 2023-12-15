using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Popups;

namespace Sample;

public partial class MainPage : IInitializeAware
{
    private readonly INavigationService _navigationService;
    private readonly IRegionManager _regionManager;
    private readonly IPopupNavigationService _popupNavigationService;
    int count = 0;

    public MainPage(INavigationService navigationService,
        IRegionManager regionManager, IPopupNavigationService popupNavigationService)
    {
        InitializeComponent();
        _navigationService = navigationService;
        _regionManager = regionManager;
        _popupNavigationService = popupNavigationService;
    }

    public void Initialize(INavigationParameters parameters)
    {
        _regionManager.RequestNavigate("MainRegion", "NewContent1", null);
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        await _popupNavigationService.ShowPopupAsync("PopupPageTest", null, true);


        //await _navigationService.NavigateAsync("NewPage1", null, true);
    }
}