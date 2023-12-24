using MPowerKit.Navigation.Awares;
using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Utilities;

public class MvvmHelpers
{
    public static bool UsePageEventsInRegions { get; set; }

    public static Page? GetPreviousRootPageForNavigatingTo(Window window)
    {
        if (window.Navigation.ModalStack.Count > 1)
        {
            return window.Navigation.ModalStack[^2];
        }

        return window.Page;
    }

    public static Page? GetTopMostPage(Page? page)
    {
        if (page is null) return null;

        if (page.Navigation.ModalStack.Count > 0)
        {
            page = page.Navigation.ModalStack[^1];
        }

        return EvaluateTopMostPage(page);
    }

    private static Page EvaluateTopMostPage(Page rootPage)
    {
        var child = rootPage switch
        {
            NavigationPage np => np.CurrentPage,
            TabbedPage tp => tp.CurrentPage,
            FlyoutPage fp => fp.Detail,
            _ => null
        };

        if (child is not null) return EvaluateTopMostPage(child);

        return rootPage;
    }

    public static Page GetFarthestDirectParentPageOrSelf(Page page)
    {
        if (page.Parent is not Page
            || page.Parent is NavigationPage np && np.RootPage != page) return page;

        return GetFarthestDirectParentPageOrSelf((page.Parent as Page)!);
    }

    public static T? GetParentOfTypeOrSelf<T>(VisualElement? element) where T : VisualElement
    {
        while (element is not (T or null))
        {
            element = element.Parent as VisualElement;
        }

        return element as T;
    }

    public static Page GetRootPageBeforeWindowForPage(Page page)
    {
        while (page.Parent is Page parent)
        {
            page = parent;
        }

        return page;
    }

    public static string? GetSegmentNameFromPage(Page page)
    {
        return ViewServiceProviderAttached.GetServiceScope(page)?.ServiceProvider?.GetService<IPageAccessor>()?.SegmentName;
    }

    public static bool IsParentForElement(Element? element, Page parent)
    {
        while (element is not null || element != parent)
        {
            element = element!.Parent;
        }

        return element is not null;
    }

    public static bool IsParentRegionHolder(Element? initialView, Element? regionHolder)
    {
        if (regionHolder is null || initialView is null) return false;

        var view = initialView;

        while (view.Parent is not null)
        {
            if (view.Parent is Page page)
            {
                return page == regionHolder;
            }
            if (view.Parent == regionHolder) return true;

            view = view.Parent;
        }

        return false;
    }

    public static void PageNavigatedRecursively(Page target, INavigationParameters parameters, bool to)
    {
        var child = target switch
        {
            NavigationPage np => np.CurrentPage,
            TabbedPage tp => tp.CurrentPage,
            FlyoutPage fp => fp.Detail,
            _ => null
        };

        IRegionManager? regionManager = null;

        if (UsePageEventsInRegions)
        {
            regionManager = ViewServiceProviderAttached.GetServiceScope(target)?.ServiceProvider.GetService<IRegionManager>();
        }

        if (to)
        {
            Navigated(target, parameters, to);

            if (UsePageEventsInRegions && regionManager is not null)
            {
                foreach (var region in regionManager.GetRegions(target))
                {
                    region.NavigatedRecursively(parameters, to);
                }
            }
        }

        if (child is not null)
        {
            PageNavigatedRecursively(child, parameters, to);
        }

        if (!to)
        {
            if (UsePageEventsInRegions && regionManager is not null)
            {
                foreach (var region in regionManager.GetRegions(target))
                {
                    region.NavigatedRecursively(parameters, to);
                }
            }

            Navigated(target, parameters, to);
        }
    }

    public static void Navigated(VisualElement view, INavigationParameters parameters, bool to)
    {
        IsActiveViewAttached.SetIsActiveView(view, to);
        InvokeViewAndViewModelAction<INavigationAware>(view, to ? v => v.OnNavigatedTo(parameters) : v => v.OnNavigatedFrom(parameters));
    }

    public static void OnInitialized(VisualElement page, INavigationParameters parameters)
    {
        InvokeViewAndViewModelAction<IInitializeAware>(page, v => v.Initialize(parameters));
    }

    public static void InvokeViewAndViewModelAction<T>(VisualElement view, Action<T> action) where T : class
    {
        if (view is T viewAsT)
        {
            action(viewAsT);
        }

        if (view.BindingContext is T viewModelAsT)
        {
            action(viewModelAsT);
        }
    }

    public static async Task InvokeViewAndViewModelActionAsync<T>(VisualElement view, Func<T, Task> action) where T : class
    {
        if (view is T viewAsT)
        {
            await action(viewAsT);
        }

        if (view.BindingContext is T viewModelAsT)
        {
            await action(viewModelAsT);
        }
    }

    public static async ValueTask InvokeViewAndViewModelActionAsync<T>(VisualElement view, Func<T, ValueTask> action) where T : class
    {
        if (view is T viewAsT)
        {
            await action(viewAsT);
        }

        if (view.BindingContext is T viewModelAsT)
        {
            await action(viewModelAsT);
        }
    }

    public static void DestroyAllPages(Page mainPage)
    {
        foreach (var page in mainPage.Navigation.ModalStack.Reverse())
        {
            DestroyPageRecursively(page);
        }
        DestroyPageRecursively(mainPage);
    }

    public static void DestroyPageRecursively(Page page)
    {
        try
        {
            switch (page)
            {
                case FlyoutPage flyout:
                    DestroyPageRecursively(flyout.Detail);
                    DestroyPageRecursively(flyout.Flyout);
                    break;
                case TabbedPage tabbedPage:
                    foreach (var item in tabbedPage.Children.Reverse())
                    {
                        DestroyPageRecursively(item);
                    }
                    break;
                case NavigationPage navigationPage:
                    foreach (var item in navigationPage.Navigation.NavigationStack.Reverse())
                    {
                        DestroyPageRecursively(item);
                    }
                    break;
            }

            DestroyPage(page);

            ViewServiceProviderAttached.GetServiceScope(page)?.Dispose();
        }
        catch (Exception ex)
        {
            throw new Exception($"Cannot destroy {page}.", ex);
        }
    }

    public static void DestroyPage(Page page)
    {
        var regionManager = ViewServiceProviderAttached.GetServiceScope(page)?.ServiceProvider.GetService<IRegionManager>();

        if (regionManager is not null)
        {
            foreach (var region in regionManager.GetRegions(page))
            {
                region.DestroyAll();
            }
        }

        Destroy(page);

        page.Behaviors?.Clear();
        page.BindingContext = null;
    }

    public static void Destroy(VisualElement element)
    {
        InvokeViewAndViewModelAction<IDestructible>(element, static v => v.Destroy());
    }

    public static void OnWindowLifecycleWithModalStack(Window window, bool resume)
    {
        OnWindowLifecycleRecursively(window.Page!, resume);

        if (window.Navigation.ModalStack.Count == 0) return;

        foreach (var item in window.Navigation.ModalStack)
        {
            OnWindowLifecycleRecursively(item, resume);
        }
    }

    private static void OnWindowLifecycleRecursively(Page page, bool resume)
    {
        WindowLifecycle(page, resume);

        if (UsePageEventsInRegions)
        {
            var regionManager = ViewServiceProviderAttached.GetServiceScope(page)?.ServiceProvider.GetService<IRegionManager>();

            if (regionManager is not null)
            {
                foreach (var region in regionManager.GetRegions(page))
                {
                    region.OnWindowLifecycleRecursively(resume);
                }
            }
        }

        switch (page)
        {
            case FlyoutPage flyout:
                OnWindowLifecycleRecursively(flyout.Flyout, resume);
                OnWindowLifecycleRecursively(flyout.Detail, resume);
                break;
            case TabbedPage tabbedPage:
                foreach (var item in tabbedPage.Children)
                {
                    OnWindowLifecycleRecursively(item, resume);
                }
                break;
            case NavigationPage navigationPage:
                foreach (var item in navigationPage.Navigation.NavigationStack)
                {
                    OnWindowLifecycleRecursively(item, resume);
                }
                break;
        }
    }

    public static void WindowLifecycle(VisualElement element, bool resume)
    {
        InvokeViewAndViewModelAction<IWindowLifecycleAware>(element, resume ? static v => v.OnResume() : static v => v.OnSleep());
    }

    public static void PageLifecycle(VisualElement element, bool appearing)
    {
        InvokeViewAndViewModelAction<IPageLifecycleAware>(element, appearing ? static v => v.OnAppearing() : static v => v.OnDisappearing());
    }

    public static bool HandleSystemBackButtonClickRecursively(Page target)
    {
        return HandleSystemBackButtonClickRecursively(target, []);
    }

    public static bool HandleSystemBackButtonClickRecursively(Page target, Dictionary<Element, Page> parentChild)
    {
        if (target is FlyoutPage fPage && fPage.IsPresented)
        {
            if (HandleSystemBackButtonClickRecursively(fPage.Flyout, parentChild)) return true;

            fPage.IsPresented = false;
            return true;
        }

        var child = target switch
        {
            NavigationPage np => np.CurrentPage,
            TabbedPage tp => tp.CurrentPage,
            FlyoutPage fp => fp.Detail,
            _ => null
        };

        if (child is not null)
        {
            if (HandleSystemBackButtonClickRecursively(child, parentChild)) return true;
        }

        if (OnSystemBackButtonClick(target)) return true;

        var window = target.Window;

        parentChild[target.Parent] = target;

        var handled = target.SendBackButtonPressed();

        if (handled)
        {
            var nparams = new NavigationParameters
            {
                { KnownNavigationParameters.NavigationDirection, NavigationDirection.Back }
            };

            Page? pageNavigateFrom;
            Page? pageNavigateTo;

            if (target.Parent is null || target.Parent is Window wnd && target != wnd.Page)
            {
                pageNavigateFrom = target;
                pageNavigateTo = window.Navigation.ModalStack.Count > 0 ? window.Navigation.ModalStack[^1] : window.Page;
            }
            else
            {
                pageNavigateFrom = parentChild[target];
                var index = target.Navigation.NavigationStack.ToList().IndexOf(pageNavigateFrom);
                //HACK: this done for ios and mac, because MAUI team couldn't do this in a more proper way
                index = index != -1 ? 2 : 1;
                pageNavigateTo = target.Navigation.NavigationStack[^index];
            }

            PageNavigatedRecursively(pageNavigateFrom, nparams, false);
            PageNavigatedRecursively(pageNavigateTo!, nparams, true);

            DestroyPageRecursively(pageNavigateFrom);
        }

        return handled;
    }

    public static bool OnSystemBackButtonClick(Page page)
    {
        var handled = false;
        InvokeViewAndViewModelAction<ISystemBackButtonClickAware>(page, v => handled |= handled || v.OnSystemBackButtonClick());
        return handled;
    }
}