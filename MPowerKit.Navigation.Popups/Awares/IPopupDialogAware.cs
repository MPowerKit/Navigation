using MPowerKit.Navigation.Popups;

namespace MPowerKit.Navigation.Awares;

public interface IPopupDialogAware
{
    public Action<(Confirmation Confirmation, bool Animated)>? RequestClose { get; set; }
}