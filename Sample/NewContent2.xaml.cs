using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;
using MPowerKit.Regions;

namespace Sample;

public partial class NewContent2 : ContentView, IInitializeAware, INavigationAware
{
    public NewContent2()
    {
        InitializeComponent();
    }

    public void Initialize(INavigationParameters parameters)
    {
        Console.WriteLine("Initialize NewContent2");
    }

    public void OnNavigatedFrom(INavigationParameters navigationParameters)
    {
        Console.WriteLine("OnNavigatedFrom NewContent2");
    }

    public void OnNavigatedTo(INavigationParameters navigationParameters)
    {
        Console.WriteLine("OnNavigatedTo NewContent2");
    }
}