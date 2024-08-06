using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Popups;

namespace Sample;

public partial class PopupPageTest : IPopupDialogAware
{
    public PopupPageTest()
    {
        InitializeComponent();
    }

    public Action<(Confirmation Confirmation, bool Animated)> RequestClose { get; set; }
}