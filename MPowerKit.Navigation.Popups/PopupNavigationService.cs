using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Utilities;
using MPowerKit.Popups;
using MPowerKit.Popups.Interfaces;

namespace MPowerKit.Navigation.Popups;

public class PopupNavigationService : IPopupNavigationService
{
    protected IPopupService PopupService { get; }
    protected IPageAccessor PageAccessor { get; }

    public PopupNavigationService(INavigationPopupService popupService, IPageAccessor pageAccessor)
    {
        PopupService = popupService;
        PageAccessor = pageAccessor;
    }

    public virtual async ValueTask<PopupResult> ShowAwaitablePopupAsync(
        string popupName,
        INavigationParameters? parameters = null,
        bool animated = true)
    {
        try
        {
            var tcs = new TaskCompletionSource<Confirmation?>();

            var navResult = await ShowPopupAsync(popupName, parameters, animated, tcs.SetResult);

            if (!navResult.Success || navResult.Exception is not null)
            {
                return new PopupResult(navResult.Success, navResult.Exception, null);
            }

            var c = await tcs.Task;

            return new PopupResult(true, null, c);
        }
        catch (Exception ex)
        {
            return new PopupResult(false, ex, null);
        }
    }

    public virtual async ValueTask<NavigationResult> ShowPopupAsync(
        string popupName,
        INavigationParameters? parameters = null,
        bool animated = true,
        Action<Confirmation?>? closeAction = null)
    {
        parameters ??= new NavigationParameters();

        try
        {
            var window = (PageAccessor.Page?.Window)
                ?? throw new InvalidOperationException("No parent window found");

            parameters.Add(KnownNavigationParameters.NavigationDirection, NavigationDirection.New);

            var page = await ConfigurePage(popupName, parameters);

            async void Page_BackgroundClicked(object? sender, RoutedEventArgs e)
            {
                var page = (sender as PopupPage)!;

                if (!page.CloseOnBackgroundClick) return;

                try
                {
                    if (e.Handled) return;

                    e.Handled = true;

                    page.BackgroundClicked -= Page_BackgroundClicked;
                    await this.HidePopupAsync(page, true);
                    closeAction?.Invoke(new Confirmation(false, null));
                }
                catch { }
            }

            page.BackgroundClicked += Page_BackgroundClicked;

            MvvmHelpers.PageNavigatedRecursively(page, parameters, true);

            MvvmHelpers.InvokeViewAndViewModelAction<IPopupDialogAware>(page,
                aware =>
                {
                    aware.RequestClose = async close =>
                    {
                        try
                        {
                            await this.HidePopupAsync(page, close.Animated);
                            closeAction?.Invoke(close.Confirmation);
                        }
                        catch { }
                    };
                });

            await PopupService.ShowPopupAsync(page, window, animated);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual async ValueTask<NavigationResult> HidePopupAsync(bool animated = true)
    {
        try
        {
            if (PopupService.PopupStack.Count == 0)
            {
                throw new InvalidOperationException("Popup stack is empty");
            }
            var window = (PageAccessor.Page?.Window)
                ?? throw new InvalidOperationException("No parent window found");

            var page = PopupService.PopupStack.LastOrDefault(p => p.Window == window)
                ?? throw new InvalidOperationException("No popup shown from current context");

            await HidePopupAsync(page, animated);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual async ValueTask<NavigationResult> HidePopupAsync(PopupPage page, bool animated = true)
    {
        try
        {
            if (PopupService.PopupStack.Count == 0)
            {
                throw new InvalidOperationException("Popup stack is empty");
            }
            var window = page.Window
                ?? throw new InvalidOperationException("No parent window found");

            var parameters = new NavigationParameters
            {
                { KnownNavigationParameters.NavigationDirection, NavigationDirection.Back }
            };

            MvvmHelpers.PageNavigatedRecursively(page, parameters, false);

            await PopupService.HidePopupAsync(page, animated);

            MvvmHelpers.InvokeViewAndViewModelAction<IPopupDialogAware>(page, static aware => aware.RequestClose = null);

            MvvmHelpers.DestroyPage(page);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    protected virtual async ValueTask<PopupPage> ConfigurePage(string pageName, INavigationParameters parameters)
    {
        var scope = ViewServiceProviderAttached.GetServiceScope(PageAccessor.Page!);

        var page = (scope.ServiceProvider.GetViewAndViewModel(pageName) as PopupPage)!;

        ViewServiceProviderAttached.SetServiceScope(page, scope);

        BehaviorExtensions.ApplyPageBehaviors(scope.ServiceProvider, page);

        MvvmHelpers.OnInitialized(page, parameters);

        await MvvmHelpers.OnInitializedAsync(page, parameters);

        return page;
    }
}