using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Popups;

public class Confirmation
{
    public bool Confirmed { get; set; }
    public INavigationParameters? CloseParameters { get; set; }
}