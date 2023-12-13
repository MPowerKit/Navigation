namespace MPowerKit.Navigation.Interfaces;

public interface INavigationParameters : IDictionary<string, object>
{
    object? GetValue(string key);
    T GetValue<T>(string key);
}