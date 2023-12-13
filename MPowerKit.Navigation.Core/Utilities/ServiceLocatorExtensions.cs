using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MPowerKit.Navigation.Utilities;

public static class ServiceLocatorExtensions
{
    public static IServiceCollection RegisterForNavigation<TView>(this IServiceCollection container, string? name = null) where TView : VisualElement
    {
        return container.RegisterForNavigation(typeof(TView), null, name);
    }

    public static IServiceCollection RegisterForNavigation<TView, TViewModel>(this IServiceCollection container, string? name = null) where TView : VisualElement
    {
        return container.RegisterForNavigation(typeof(TView), typeof(TViewModel), name);
    }

    public static IServiceCollection RegisterForNavigation(this IServiceCollection container, Type view, Type? viewModel, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(view, nameof(view));

        if (string.IsNullOrWhiteSpace(name))
            name = view.Name;

        container.AddKeyedSingleton(name, new ViewRegistrationModel
        {
            Name = name,
            View = view,
            ViewModel = viewModel
        })
            .TryAddTransient(view);

        if (viewModel is not null)
            container.TryAddTransient(viewModel);

        return container;
    }

    public static VisualElement GetViewAndViewModel(this IServiceProvider container, string viewName)
    {
        var model = container.GetKeyedService<ViewRegistrationModel>(viewName)
            ?? throw new KeyNotFoundException($"Couldn't find registration for view {viewName}");

        var view = (container.GetService(model.View) as VisualElement)!;
        object? viewModel = null;
        if (model.ViewModel is not null)
        {
            viewModel = container.GetService(model.ViewModel)!;
        }

        view.BindingContext = viewModel ?? new object();

        return view;
    }
}