namespace MPowerKit.Navigation;

public class TabNavigationPage : NavigationPage
{
    public TabNavigationPage()
    {
        this.SetBinding(Page.IconImageSourceProperty, new Binding("RootPage.IconImageSource", BindingMode.TwoWay, source: this));
        this.SetBinding(Page.TitleProperty, new Binding("RootPage.Title", BindingMode.TwoWay, source: this));
    }
}