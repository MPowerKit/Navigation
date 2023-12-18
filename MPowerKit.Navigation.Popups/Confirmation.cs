using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Popups;

public record Confirmation(bool Confirmed, INavigationParameters? CloseParameters);