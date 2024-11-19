using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Utilities;
using MPowerKit.Navigation.WindowInfrastructure;

namespace MPowerKit.Navigation;

public class NavigationService : INavigationService
{
    protected IServiceProvider ServiceProvider { get; }
    protected IWindowManager WindowManager { get; }
    protected IPageAccessor PageAccessor { get; }

    public Guid CurrentWindowId => Window?.Id ?? Guid.Empty;

    private Window? _window;
    public Window? Window
    {
        get
        {
            if (_window is null && PageAccessor.Page is not null)
            {
                _window = PageAccessor.Page.Window;
            }

            return _window;
        }
    }

    public NavigationService(
        IServiceProvider serviceProvider,
        IWindowManager windowManager,
        IPageAccessor pageAccessor)
    {
        ServiceProvider = serviceProvider;
        WindowManager = windowManager;
        PageAccessor = pageAccessor;
    }

    public virtual ValueTask<NavigationResult> OpenNewWindowAsync(string stringUri, INavigationParameters? parameters = null)
    {
        return ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<INavigationService>().NavigateAsync(stringUri, parameters, false, false);
    }

    public virtual NavigationResult CloseWindow(Guid? windowId = null)
    {
        try
        {
            windowId ??= CurrentWindowId;

            var window = WindowManager.Windows.FirstOrDefault(x => x.Id == windowId)
                ?? WindowManager.Windows.LastOrDefault()
                ?? throw new ArgumentException("Window not found");

            MvvmHelpers.DestroyAllPages(window.Page!);

            WindowManager.CloseWindow(window);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual NavigationResult ToggleFlyout(bool isPresented)
    {
        try
        {
            var currentPage = GetCurrentPage()!;

            var flyoutPage = MvvmHelpers.GetParentOfTypeOrSelf<FlyoutPage>(currentPage)
                ?? throw new InvalidOperationException("There is no parent FlyoutPage");

            flyoutPage.IsPresented = isPresented;

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual NavigationResult SelectTab(string tabName, INavigationParameters? parameters)
    {
        parameters ??= new NavigationParameters();

        try
        {
            var currentPage = GetCurrentPage()!;

            var tabbedPage = MvvmHelpers.GetParentOfTypeOrSelf<TabbedPage>(currentPage)
                ?? throw new InvalidOperationException("There is no parent TabbedPage to select page on it");

            Page pageNavigateFrom = tabbedPage.CurrentPage;
            Page? pageNavigateTo = null;

            foreach (var page in tabbedPage.Children)
            {
                if (MvvmHelpers.GetSegmentNameFromPage(page) == tabName
                    || page is NavigationPage navPage && MvvmHelpers.GetSegmentNameFromPage(navPage.RootPage) == tabName)
                {
                    pageNavigateTo = page;
                    break;
                }
            }

            if (pageNavigateTo is null)
            {
                throw new ArgumentException($"There is not such tab with name {tabName}");
            }
            if (pageNavigateFrom == pageNavigateTo)
            {
                throw new InvalidOperationException("Cannot select already selected tab");
            }

            MvvmHelpers.PageNavigatedRecursively(pageNavigateFrom, parameters, false);
            MvvmHelpers.PageNavigatedRecursively(pageNavigateTo, parameters, true);

            tabbedPage.CurrentPage = pageNavigateTo;

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual async ValueTask<NavigationResult> GoBackToRootAsync(INavigationParameters? parameters = null, bool animated = true)
    {
        parameters ??= new NavigationParameters();

        try
        {
            var currentPage = GetCurrentPage()!;
            if (!IsActiveViewAttached.GetIsActiveView(currentPage))
            {
                throw new InvalidOperationException("Cannot navigate back from inactvie page");
            }

            if (currentPage.Parent is not NavigationPage np
                || np.RootPage == currentPage)
            {
                throw new InvalidOperationException("Cannot navigate back to root, because you already in root");
            }

            var pageNavigateTo = np.RootPage;
            var pageNavigateFrom = np.CurrentPage;

            parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.Back;

            MvvmHelpers.PageNavigatedRecursively(pageNavigateFrom, parameters, false);
            MvvmHelpers.PageNavigatedRecursively(pageNavigateTo, parameters, true);

            await np.Navigation.PopToRootAsync(animated);

            foreach (var page in np.Navigation.NavigationStack.Reverse().ToList()[..^1])
            {
                MvvmHelpers.DestroyPageRecursively(page);
            }

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual async ValueTask<NavigationResult> GoBackAsync(INavigationParameters? parameters = null, bool modal = false, bool animated = true)
    {
        parameters ??= new NavigationParameters();

        try
        {
            if (Window is null)
            {
                throw new InvalidOperationException("Window does not exist");
            }

            var currentPage = GetCurrentPage()!;
            if (!IsActiveViewAttached.GetIsActiveView(currentPage))
            {
                throw new InvalidOperationException("Cannot navigate back from inactvie page");
            }

            if (modal)
            {
                await GoBackModally(parameters, animated);
            }
            else
            {
                await GoBack(parameters, animated);
            }

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    protected virtual async ValueTask GoBack(INavigationParameters parameters, bool animated)
    {
        var currentPage = GetCurrentPage()!;

        var farthestDirectParent = MvvmHelpers.GetFarthestDirectParentPageOrSelf(currentPage);

        if (farthestDirectParent.Parent is Window
            && Window!.Navigation.ModalStack.Count > 0
            && Window.Navigation.ModalStack.Any(p => p == farthestDirectParent))
        {
            await GoBackModally(parameters, animated);
            return;
        }

        var previousRootPage = MvvmHelpers.GetPreviousRootPageForNavigatingTo(Window);
        if (previousRootPage == farthestDirectParent)
        {
            throw new InvalidOperationException("Cannot navigate back from MainPage of the Window");
        }
        if (farthestDirectParent.Navigation.NavigationStack.Count == 0)
        {
            throw new InvalidOperationException("Cannot navigate back, the navigation stack is empty");
        }

        var index = farthestDirectParent.Navigation.NavigationStack.ToList().IndexOf(farthestDirectParent);

        var pageNavigateTo = farthestDirectParent.Navigation.NavigationStack[index - 1];
        var pageNavigateFrom = farthestDirectParent;

        parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.Back;

        MvvmHelpers.PageNavigatedRecursively(pageNavigateFrom, parameters, false);
        MvvmHelpers.PageNavigatedRecursively(pageNavigateTo, parameters, true);

        var poppedPage = await DoPop(pageNavigateTo.Navigation, false, animated);
        if (poppedPage is null) return;

        MvvmHelpers.DestroyPageRecursively(poppedPage);
    }

    protected virtual async ValueTask GoBackModally(INavigationParameters parameters, bool animated)
    {
        if (Window!.Navigation.ModalStack.Count == 0)
        {
            throw new InvalidOperationException("Cannot navigate back modally, modal stack is empty");
        }

        var previousPage = MvvmHelpers.GetPreviousRootPageForNavigatingTo(Window!)
            ?? throw new InvalidOperationException("Cannot navigate back modally");

        parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.Back;

        MvvmHelpers.PageNavigatedRecursively(GetCurrentPage()!, parameters, false);
        MvvmHelpers.PageNavigatedRecursively(previousPage, parameters, true);

        var poppedPage = await DoPop(Window!.Navigation, true, animated);
        if (poppedPage is null) return;

        MvvmHelpers.DestroyPageRecursively(poppedPage);
    }

    protected virtual async ValueTask<Page?> DoPop(INavigation navigation, bool useModalNavigation, bool animated)
    {
        return await (useModalNavigation ? navigation.PopModalAsync(animated) : navigation.PopAsync(animated));
    }

    public virtual async ValueTask<NavigationResult> NavigateAsync(string stringUri, INavigationParameters? parameters = null, bool modal = false, bool animated = true)
    {
        // need to be here to wait until ui finish its important work
        await Task.Delay(1);

        parameters ??= new NavigationParameters();

        try
        {
            var uri = UriParsingHelper.ReplaceDotsAndParseUri(stringUri);

            var isAbsoluteNavigation = uri.IsAbsoluteUri || Window?.Page is null;

            if (!isAbsoluteNavigation && modal)
            {
                parameters.Add(KnownNavigationParameters.IsNavigatedModally, modal);
            }

            var pages = await ConstructPages(uri, parameters, isAbsoluteNavigation);

            if (isAbsoluteNavigation)
            {
                DoAbsolutePush(pages[0]!, parameters);
            }
            else if (modal)
            {
                await DoModalPush(pages[0]!, parameters, animated);
            }
            else
            {
                await DoRelativeNavigation(pages, parameters, animated);
            }

            if (pages?.Count > 0) await MvvmHelpers.OnPageLoadedRecursively(pages[0]!, parameters);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual async ValueTask<NavigationResult> NavigateThrougFlyoutPageAsync(string stringUri, INavigationParameters? parameters = null)
    {
        if (PageAccessor?.Page is not FlyoutPage) throw new InvalidOperationException("Can proceed this navigation only from FlyoutPage");

        // need to be here to wait until ui finish its important work
        await Task.Delay(1);

        parameters ??= new NavigationParameters();

        try
        {
            var uri = UriParsingHelper.ReplaceDotsAndParseUri(stringUri);

            if (uri.IsAbsoluteUri) throw new InvalidOperationException("Using absolute navigation inside FlyoutPage is not allowed");

            var pages = await ConstructPages(uri, parameters);

            DoFlyoutPush(pages[0]!, parameters);

            if (pages?.Count > 0) await MvvmHelpers.OnPageLoadedRecursively(pages[0]!, parameters);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    protected virtual async ValueTask DoRelativeNavigation(List<Page?> pages, INavigationParameters parameters, bool animated)
    {
        if (Window is null)
        {
            throw new InvalidOperationException("Window does not exist");
        }

        var currentPage = GetCurrentPage()!;

        if (!IsActiveViewAttached.GetIsActiveView(currentPage))
        {
            throw new InvalidOperationException("Cannot execute relative navigation from inactive page");
        }

        var initialCount = pages.Count;
        pages.RemoveAll(static p => p is null);
        var countOfPagesToBeRemoved = initialCount - pages.Count;

        var parent = currentPage.Parent;

        if (parent is Window && Window!.Page != currentPage)
        {
            throw new InvalidOperationException("Cannot use relative navigation for root of modal stack. Use GoBack method");
        }
        if (parent is NavigationPage np)
        {
            if (np.Navigation.NavigationStack.Count < countOfPagesToBeRemoved
                || (np.Navigation.NavigationStack.Count == countOfPagesToBeRemoved && pages.Count == 0))
            {
                throw new InvalidOperationException("Cannot remove more pages by '..' back operator(s) than the navigation stack has");
            }

            await ProcessRelativeNavigationWhenParentIsNavigationPage(np, pages, countOfPagesToBeRemoved, parameters, animated);
            return;
        }

        if (countOfPagesToBeRemoved == 0)
        {
            throw new InvalidOperationException("Cannot execute relative navigation from root page, because it has no navigation stack");
        }
        if (pages.Count == 0)
        {
            throw new InvalidOperationException("Cannot navigate back from CurrentPage page of the TabbedPage");
        }
        if (countOfPagesToBeRemoved > 1 && pages.Count > 0)
        {
            throw new InvalidOperationException("Cannot remove more pages by '..' back operator(s) than the navigation stack has");
        }
        if (countOfPagesToBeRemoved == 1 && pages.Count > 1)
        {
            throw new InvalidOperationException("Cannot replace CurrentPage page of the TabbedPage by provided pages within navigation path. Navigation path should start from NavigationPage, TabbedPage or FlyoutPage only, or just contain only one segment of ContentPage");
        }

        ProcessRelativeNavigationWhenParentIsNotNavigationPage(parent, currentPage, parameters);
    }

    protected virtual void ProcessRelativeNavigationWhenParentIsNotNavigationPage(Element parent, Page currentPage, INavigationParameters parameters)
    {
        var pageNavigateTo = currentPage;
        var pageNavigateFrom = (parent switch
        {
            Window wind => wind.Page,
            TabbedPage tp => tp.CurrentPage,
            FlyoutPage fp => fp.Detail,
            _ => null
        })!;

        parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.New;

        MvvmHelpers.PageNavigatedRecursively(pageNavigateFrom, parameters, false);
        MvvmHelpers.PageNavigatedRecursively(pageNavigateTo, parameters, true);

        if (parent is Window wnd)
        {
            wnd.Page = pageNavigateTo;
        }
        else if (parent is TabbedPage tp)
        {
            var index = tp.Children.IndexOf(pageNavigateFrom);

            tp.Children[index] = pageNavigateTo;
            tp.CurrentPage = pageNavigateTo;
        }
        else if (parent is FlyoutPage fp)
        {
            fp.Detail = pageNavigateTo;
        }

        MvvmHelpers.DestroyPageRecursively(pageNavigateFrom);
    }

    protected virtual ValueTask ProcessRelativeNavigationWhenParentIsNavigationPage(
        NavigationPage np,
        List<Page?> pages,
        int countOfPagesToBeRemoved,
        INavigationParameters parameters,
        bool animated)
    {
        bool? forward = null;

        if (countOfPagesToBeRemoved == 0)
        {
            forward = true;
        }
        else if (np.Navigation.NavigationStack.Count > countOfPagesToBeRemoved && pages.Count == 0)
        {
            forward = false;
        }
        else if (np.Navigation.NavigationStack.Count >= countOfPagesToBeRemoved && pages.Count != 0)
        {
            forward = null;
        }

        return ProcessNavigationForNavigationPage(np, pages!, countOfPagesToBeRemoved, parameters, animated, forward);
    }

    protected virtual async ValueTask ProcessNavigationForNavigationPage(
        NavigationPage navigationPage,
        List<Page> pages,
        int countOfPagesToBeRemoved,
        INavigationParameters parameters,
        bool animated,
        bool? forward)
    {
        var navStack = navigationPage.Navigation.NavigationStack.ToList();

        var lastIndex = navStack.Count - 1;
        var pagesToRemove = navStack[(lastIndex - countOfPagesToBeRemoved + 1)..];
        pagesToRemove.Reverse();
        var pageNavigateFrom = navStack[^1];
        var pageNavigateTo = forward is false ? navStack[lastIndex - countOfPagesToBeRemoved] : pages[^1];

        parameters[KnownNavigationParameters.NavigationDirection] = forward is false ? NavigationDirection.Back : NavigationDirection.New;

        MvvmHelpers.PageNavigatedRecursively(pageNavigateFrom, parameters, false);
        MvvmHelpers.PageNavigatedRecursively(pageNavigateTo, parameters, true);

        if (forward is not false)
        {
            await navigationPage.Navigation.PushAsync(pageNavigateTo, animated);

            foreach (var page in pages[..^1])
            {
                navigationPage.Navigation.InsertPageBefore(page, pageNavigateTo);
            }
        }
        if (forward is not true)
        {
            foreach (var page in pagesToRemove[(forward is false ? 1 : 0)..])
            {
                navigationPage.Navigation.RemovePage(page);
            }

            if (forward is false)
            {
                await DoPop(navigationPage.Navigation, false, animated);
            }

            foreach (var page in pagesToRemove)
            {
                MvvmHelpers.DestroyPageRecursively(page);
            }
        }
    }

    protected virtual async ValueTask DoModalPush(Page rootPage, INavigationParameters parameters, bool animated)
    {
        if (Window is null)
        {
            throw new InvalidOperationException("Window does not exist");
        }

        parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.New;

        var currentPage = GetCurrentPage()!;

        var navigateFrom = MvvmHelpers.GetRootPageBeforeWindowForPage(currentPage);

        MvvmHelpers.PageNavigatedRecursively(navigateFrom, parameters, false);
        MvvmHelpers.PageNavigatedRecursively(rootPage, parameters, true);

        await currentPage.Navigation.PushModalAsync(rootPage, animated);
    }

    protected virtual void DoAbsolutePush(Page rootPage, INavigationParameters parameters)
    {
        parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.New;

        if (Window is null)
        {
            MvvmHelpers.PageNavigatedRecursively(rootPage, parameters, true);

            _window = (ServiceProvider.GetRequiredService<IMPowerKitWindow>() as Window)!;

            if (_window is not MPowerKitWindow)
            {
                throw new ArgumentException($"Registered window should be of type {nameof(IMPowerKitWindow)}");
            }

            _window.Page = rootPage;

            WindowManager.OpenWindow(_window);

            return;
        }

        var navigateFrom = GetPageFromWindow()!;

        if (navigateFrom.Navigation.ModalStack.Count > 0)
        {
            navigateFrom = navigateFrom.Navigation.ModalStack[^1];
        }

        MvvmHelpers.PageNavigatedRecursively(navigateFrom, parameters, false);
        MvvmHelpers.PageNavigatedRecursively(rootPage, parameters, true);

        Window.Page = rootPage;

        MvvmHelpers.DestroyAllPages(navigateFrom);
    }

    protected virtual void DoFlyoutPush(Page rootPage, INavigationParameters parameters)
    {
        parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.New;

        var flyoutPage = (PageAccessor.Page as FlyoutPage)!;

        var navigateFrom = flyoutPage.Detail;

        MvvmHelpers.PageNavigatedRecursively(navigateFrom, parameters, false);
        MvvmHelpers.PageNavigatedRecursively(rootPage, parameters, true);

        flyoutPage.Detail = rootPage;

        MvvmHelpers.DestroyAllPages(navigateFrom);
    }

    protected virtual async ValueTask<List<Page?>> ConstructPages(Uri uri, INavigationParameters parameters, bool isAbsoluteOrModal = true)
    {
        var (segments, queryParameters) = UriParsingHelper.ProcessUri(uri);

        if (segments.Count == 0)
        {
            throw new ArgumentException("Navigation path should have at least one page");
        }
        if (isAbsoluteOrModal && segments.Peek() == UriParsingHelper._onePageBack)
        {
            throw new ArgumentException("Absolute, modal or query navigation path should not contain '..' back operator. This does not have any sense");
        }

        var pagesStack = await ConstructPagesFromSegmentsRecursively(segments, segments.Count, queryParameters, parameters);

        if (isAbsoluteOrModal && pagesStack.Count > 1)
        {
            throw new ArgumentException("Absolute, modal or query navigation path should start from NavigationPage, TabbedPage or FlyoutPage only, or just contain only one segment of ContentPage");
        }

        return [.. pagesStack];
    }

    protected virtual async ValueTask<Stack<Page?>> ConstructPagesFromSegmentsRecursively(
        Queue<string> segments,
        int initialCount,
        List<(string, Uri)> queryParameters,
        INavigationParameters parameters)
    {
        if (segments.Count == 0) return new Stack<Page?>(initialCount);

        var segment = segments.Dequeue();

        Page? page = null;
        if (segment != UriParsingHelper._onePageBack)
        {
            page = await ConfigurePage(segment, parameters);
        }

        var pages = await ConstructPagesFromSegmentsRecursively(segments, initialCount, queryParameters, parameters);

        switch (page)
        {
            case TabbedPage tp:
                {
                    if (pages.Count > 0)
                    {
                        throw new ArgumentException("Usage of another segments after TabbedPage in navigation path is not allowed");
                    }

                    await ConfigureTabbedPage(tp, queryParameters, parameters);
                }
                break;
            case not TabbedPage when pages.Count == 0 && queryParameters.Count > 0:
                {
                    throw new ArgumentException("Query parameters can be added after TabbedPage only");
                }
            case FlyoutPage fp:
                {
                    if (pages.Count != 1)
                    {
                        throw new ArgumentException("NavigationPage or single page should follow after FlyoutPage in navigation path");
                    }

                    ConfigureFlyoutPage(fp, pages.Pop()!);
                }
                break;
            case NavigationPage np:
                {
                    await ConfigureNavigationPage(np, pages!);
                }
                break;
            default:
                break;
        }

        pages.Push(page);

        return pages;
    }

    protected virtual async ValueTask ConfigureNavigationPage(NavigationPage navPage, Stack<Page> pages)
    {
        if (pages.Count == 0)
        {
            throw new ArgumentException("At least one page should follow after NavigationPage");
        }

        while (pages.Count > 0)
        {
            await navPage.PushAsync(pages.Pop(), false);
        }
    }

    protected virtual void ConfigureFlyoutPage(FlyoutPage flyoutPage, Page page)
    {
        flyoutPage.Detail = page;
    }

    protected virtual async ValueTask ConfigureTabbedPage(TabbedPage tabbedPage, List<(string Key, Uri Value)> queryParameters, INavigationParameters parameters)
    {
        if (queryParameters.Count == 0)
        {
            throw new ArgumentException("No query parameters found after TabbedPage in navigation uri");
        }
        if (!queryParameters.Any(static qp => qp.Key is KnownNavigationParameters.CreateTab or KnownNavigationParameters.SelectTab))
        {
            throw new ArgumentException("Provided query parameters are not acceptable for this type of page TabbedPage");
        }
        if (queryParameters.Count == 1 && queryParameters[0].Key == KnownNavigationParameters.SelectTab)
        {
            throw new ArgumentException("There is no tab pages created to select one");
        }

        var createTabs = queryParameters.FindAll(static qp => qp.Key is KnownNavigationParameters.CreateTab);

        foreach (var (key, value) in createTabs)
        {
            var pages = await ConstructPages(value, parameters);

            tabbedPage.Children.Add(pages[0]);
        }

        if (!queryParameters.Any(static qp => qp.Key is KnownNavigationParameters.SelectTab)) return;

        var (Key, Value) = queryParameters.FindLast(static qp => qp.Key is KnownNavigationParameters.SelectTab);

        Page? pageSelectTo = null;

        foreach (var page in tabbedPage.Children)
        {
            if (MvvmHelpers.GetSegmentNameFromPage(page) == Value.OriginalString
                || page is NavigationPage navPage && MvvmHelpers.GetSegmentNameFromPage(navPage.RootPage) == Value.OriginalString)
            {
                pageSelectTo = page;
                break;
            }
        }

        if (pageSelectTo is null)
        {
            throw new ArgumentException($"There is not such tab with name {Value.OriginalString} to select");
        }

        tabbedPage.CurrentPage = pageSelectTo;
    }

    protected virtual async ValueTask<Page> ConfigurePage(string pageName, INavigationParameters parameters)
    {
        var newScope = ServiceProvider.CreateScope();

        var sp = newScope.ServiceProvider;
        var pa = sp.GetRequiredService<IPageAccessor>();

        var page = (sp.GetViewAndViewModel(pageName) as Page)!;

        pa.Page = page;
        pa.SegmentName = pageName;

        ViewServiceProviderAttached.SetServiceScope(page, newScope);

        BehaviorExtensions.ApplyPageBehaviors(ServiceProvider, page);

        MvvmHelpers.OnInitialized(page, parameters);

        await MvvmHelpers.OnInitializedAsync(page, parameters);

        return page;
    }

    protected virtual Page? GetCurrentPage()
    {
        return PageAccessor.Page is not null ? PageAccessor.Page : GetPageFromWindow();
    }

    private Page? GetPageFromWindow()
    {
        try
        {
            return Window?.Page;
        }
#if DEBUG
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return null;
        }
#else
        catch
        {
            return null;
        }
#endif
    }
}