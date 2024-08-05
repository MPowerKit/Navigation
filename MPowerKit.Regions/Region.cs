using MPowerKit.Navigation;
using MPowerKit.Navigation.Interfaces;
using MPowerKit.Navigation.Utilities;

namespace MPowerKit.Regions;

public class Region : IRegion
{
    protected IServiceProvider ServiceProvider { get; }
    protected IRegionManager RegionManager { get; }
    protected IRegionAccessor RegionAccessor { get; }

    protected ContentView? RegionHolder => RegionAccessor.RegionHolder;

    protected Grid StackContainer { get; } = [];

    protected IList<VisualElement> RegionStack => StackContainer.Children.OfType<VisualElement>().ToList();

    protected VisualElement? CurrentView
    {
        get => RegionStack.SingleOrDefault(c => c.IsVisible);
        set
        {
            var cv = RegionStack.FirstOrDefault(c => c.IsVisible && c != value);

            if (cv is not null) cv.IsVisible = false;

            if (value is not null) value.IsVisible = true;
        }
    }

    public Region(IServiceProvider serviceProvider,
        IRegionManager regionManager,
        IRegionAccessor regionAccessor)
    {
        ServiceProvider = serviceProvider;
        RegionManager = regionManager;
        RegionAccessor = regionAccessor;

        RegionHolder.Content = StackContainer;
    }

    public virtual NavigationResult ReplaceAll(string viewName, INavigationParameters? parameters)
    {
        parameters ??= new NavigationParameters();

        try
        {
            var view = InitView(viewName, parameters);

            var viewsToRemove = RegionStack.Reverse().ToList();

            parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.New;

            if (CurrentView is not null)
            {
                NavigatedRecursively(parameters, false);
            }

            StackContainer.Children.Add(view);
            CurrentView = view;
            NavigatedRecursively(parameters, true);

            foreach (var item in viewsToRemove)
            {
                StackContainer.Children.Remove(item);
                DestroyRecursively(item);
            }

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual NavigationResult Push(string viewName, INavigationParameters? parameters)
    {
        parameters ??= new NavigationParameters();

        try
        {
            var view = InitView(viewName, parameters);

            var index = RegionStack.Count - 1;

            parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.New;

            if (CurrentView is not null)
            {
                NavigatedRecursively(parameters, false);
                index = RegionStack.IndexOf(CurrentView);
            }

            var viewsToRemove = RegionStack.Skip(index + 1).Reverse().ToList();

            StackContainer.Children.Add(view);
            CurrentView = view;
            NavigatedRecursively(parameters, true);

            foreach (var item in viewsToRemove)
            {
                StackContainer.Children.Remove(item);
                DestroyRecursively(item);
            }

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual NavigationResult PushBackwards(string viewName, INavigationParameters? parameters)
    {
        parameters ??= new NavigationParameters();

        try
        {
            var view = InitView(viewName, parameters);

            var index = 0;

            parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.New;

            if (CurrentView is not null)
            {
                NavigatedRecursively(parameters, false);
                index = RegionStack.IndexOf(CurrentView);
            }

            var viewsToRemove = RegionStack.Take(index).Reverse().ToList();

            StackContainer.Children.Insert(index, view);
            CurrentView = view;
            NavigatedRecursively(parameters, true);

            foreach (var item in viewsToRemove)
            {
                StackContainer.Children.Remove(item);
                DestroyRecursively(item);
            }

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    protected virtual View InitView(string viewName, INavigationParameters parameters)
    {
        var view = (ServiceProvider.GetViewAndViewModel(viewName) as View)!;

        ViewRegionViewNameAttached.SetRegionViewName(view, viewName);

        MvvmHelpers.OnInitialized(view, parameters);

        BehaviorExtensions.ApplyBehaviors(ServiceProvider, view);

        return view;
    }

    public virtual bool CanGoBack()
    {
        return CurrentView is not null && RegionStack.Count > 1 && RegionStack.IndexOf(CurrentView) >= 1;
    }

    public virtual bool CanGoForward()
    {
        return CurrentView is not null && RegionStack.Count > 1 && RegionStack.IndexOf(CurrentView) <= RegionStack.Count - 2;
    }

    public virtual bool CanGoByName(string viewName)
    {
        return (RegionStack as List<VisualElement>).Exists(v => ViewRegionViewNameAttached.GetRegionViewName(v) == viewName);
    }

    public virtual NavigationResult GoBack(INavigationParameters? parameters)
    {
        parameters ??= new NavigationParameters();

        try
        {
            if (!CanGoBack())
            {
                throw new InvalidOperationException("Cannot go back");
            }

            var index = RegionStack.IndexOf(CurrentView!);

            var viewNavigateTo = RegionStack[index - 1];
            var viewNavigateFrom = CurrentView!;

            parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.Back;

            NavigatedRecursively(parameters, false);
            CurrentView = viewNavigateTo;
            NavigatedRecursively(parameters, true);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual NavigationResult GoForward(INavigationParameters? parameters)
    {
        parameters ??= new NavigationParameters();

        try
        {
            if (!CanGoForward())
            {
                throw new InvalidOperationException("Cannot go back");
            }

            var index = RegionStack.IndexOf(CurrentView!);

            var viewNavigateTo = RegionStack[index + 1];

            parameters[KnownNavigationParameters.NavigationDirection] = NavigationDirection.Forward;

            NavigatedRecursively(parameters, false);
            CurrentView = viewNavigateTo;
            NavigatedRecursively(parameters, true);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual NavigationResult GoByName(string viewName, INavigationParameters? parameters)
    {
        parameters ??= new NavigationParameters();

        try
        {
            if (!CanGoByName(viewName))
            {
                throw new InvalidOperationException("Cannot go by view name");
            }

            var viewNavigateTo = (RegionStack as List<VisualElement>).Find(v => ViewRegionViewNameAttached.GetRegionViewName(v) == viewName);

            if (viewNavigateTo == CurrentView)
            {
                throw new InvalidOperationException("Cannot go to the already active view");
            }

            var index = RegionStack.IndexOf(CurrentView!);

            var indexNavigateTo = RegionStack.IndexOf(viewNavigateTo);

            parameters[KnownNavigationParameters.NavigationDirection] = index > indexNavigateTo ? NavigationDirection.Back : NavigationDirection.Forward;

            NavigatedRecursively(parameters, false);
            CurrentView = viewNavigateTo;
            NavigatedRecursively(parameters, true);

            return new NavigationResult(true, null);
        }
        catch (Exception ex)
        {
            return new NavigationResult(false, ex);
        }
    }

    public virtual void NavigatedRecursively(INavigationParameters parameters, bool to)
    {
        if (CurrentView is null) return;

        if (to)
        {
            MvvmHelpers.Navigated(CurrentView, parameters, to);
        }

        foreach (var region in RegionManager.GetRegions(RegionHolder))
        {
            region.NavigatedRecursively(parameters, to);
        }

        if (!to)
        {
            MvvmHelpers.Navigated(CurrentView, parameters, to);
        }
    }

    public virtual void DestroyAll()
    {
        var viewsToRemove = RegionStack.Reverse().ToList();

        foreach (var item in viewsToRemove)
        {
            RegionStack.Remove(item);
            DestroyRecursively(item);
        }

        if (RegionHolder is null) return;

        ViewServiceProviderAttached.GetServiceScope(RegionHolder)?.Dispose();

        Regions.RegionManager.RemoveHolder(RegionAccessor.RegionName);
    }

    public virtual void DestroyRecursively(VisualElement view)
    {
        foreach (var region in RegionManager.GetRegions(view))
        {
            region.DestroyAll();
        }

        if (view is null) return;

        MvvmHelpers.Destroy(view);

        view.Behaviors?.Clear();
        view.BindingContext = null;
    }

    public virtual void OnWindowLifecycleRecursively(bool resume)
    {
        foreach (var view in RegionStack)
        {
            MvvmHelpers.WindowLifecycle(view, resume);
        }

        foreach (var region in RegionManager.GetRegions(RegionHolder))
        {
            region.OnWindowLifecycleRecursively(resume);
        }
    }

    public virtual void OnPageLifecycleRecursively(bool appearing)
    {
        if (appearing)
        {
            foreach (var view in RegionStack)
            {
                MvvmHelpers.PageLifecycle(view, appearing);
            }
        }

        foreach (var region in RegionManager.GetRegions(RegionHolder))
        {
            region.OnPageLifecycleRecursively(appearing);
        }

        if (!appearing)
        {
            foreach (var view in RegionStack.Reverse())
            {
                MvvmHelpers.PageLifecycle(view, appearing);
            }
        }
    }
}