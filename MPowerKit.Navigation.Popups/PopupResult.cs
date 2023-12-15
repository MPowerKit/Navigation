namespace MPowerKit.Navigation.Popups;

public record PopupResult(bool Success, Exception? Exception, Confirmation? Confirmation)
    : NavigationResult(Success, Exception);