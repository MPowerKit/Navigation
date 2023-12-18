using MPowerKit.Navigation;
using MPowerKit.Navigation.Interfaces;

namespace MPowerKit;

public static class NavigationParametersExtensions
{
    public static NavigationDirection GetNavigationDirection(this INavigationParameters? parameters)
    {
        if (parameters is null) return NavigationDirection.None;

        return parameters.GetValue<NavigationDirection>(KnownNavigationParameters.NavigationDirection);
    }
}

public class NavigationParameters : Dictionary<string, object>, INavigationParameters
{
    public NavigationParameters()
    {

    }

    public NavigationParameters(IDictionary<string, object> dictionary)
        : base(dictionary, null)
    {

    }

    public object? GetValue(string key)
    {
        if (this.ContainsKey(key)) return this[key];

        return default;
    }

    public T GetValue<T>(string key)
    {
        if (this.ContainsKey(key)) return (T)this[key];

        return default;
    }
}