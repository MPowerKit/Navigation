using MPowerKit.Navigation.Interfaces;

namespace Sample;

public partial class FlyPage : FlyoutPage
{
    private readonly INavigationService _navigationService;

    public FlyPage(INavigationService navigationService)
    {
        InitializeComponent();
        _navigationService = navigationService;
    }

    async void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        var res = await _navigationService.NavigateThroughFlyoutPageAsync("NavigationPage/NewPage1");
    }
}