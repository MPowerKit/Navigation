using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;

namespace Sample;

public partial class MainPage : IInitializeAware
{
    private readonly INavigationService _navigationService;
    private readonly IRegionManager _regionManager;
    int count = 0;

    public MainPage(INavigationService navigationService,
        IRegionManager regionManager)
    {
        InitializeComponent();
        _navigationService = navigationService;
        _regionManager = regionManager;
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

        SemanticScreenReader.Announce(CounterBtn.Text);


        //await _navigationService.NavigateAsync("../NewPage1");
        await _navigationService.GoBackAsync();
    }
}