using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;

namespace Sample;

public partial class NewPage1 : INavigationAware, IInitializeAsyncAware
{
    public NewPage1()
    {
        InitializeComponent();
    }

    public Task InitializeAsync(INavigationParameters parameters)
    {
        return Task.Delay(1000);
    }

    public void OnNavigatedFrom(INavigationParameters navigationParameters)
    {

    }

    public void OnNavigatedTo(INavigationParameters navigationParameters)
    {

    }
}