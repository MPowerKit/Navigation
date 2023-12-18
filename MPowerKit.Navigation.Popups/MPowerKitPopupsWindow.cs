using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Utilities;
using MPowerKit.Navigation.WindowInfrastructure;

namespace MPowerKit.Navigation.Popups;

public class MPowerKitPopupsWindow : MPowerKitWindow
{
    protected IPopupNavigationService PopupNavigationService { get; }

    public MPowerKitPopupsWindow(IPopupNavigationService popupNavigationService)
    {
        PopupNavigationService = popupNavigationService;
    }

    protected override bool OnBackButtonClicked()
    {
        var popupService = this.Handler.MauiContext.Services.GetRequiredService<INavigationPopupService>();

        var popup = popupService.PopupStack.FirstOrDefault(p => p.Window == this);

        if (popup is not null)
        {
            if (MvvmHelpers.OnSystemBackButtonClick(popup)) return true;

            var handled = false;
            MvvmHelpers.InvokeViewAndViewModelAction<IPopupDialogAware>(popup, aware =>
            {
                if (aware?.RequestClose is not null && !handled)
                {
                    aware?.RequestClose((new Confirmation(false, null), true));
                    handled = true;
                }
            });

            if (!handled) PopupNavigationService.HidePopupAsync(popup, true);

            return true;
        }

        return base.OnBackButtonClicked();
    }
}