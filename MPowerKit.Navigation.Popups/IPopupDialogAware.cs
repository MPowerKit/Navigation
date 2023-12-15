namespace MPowerKit.Navigation.Popups;

public interface IPopupDialogAware
{
    public Action<(Confirmation Confirmation, bool Animated)> RequestClose { get; set; }
}