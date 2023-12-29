.NET MAUI MVVM navigation framework. It supports regular/modal navigation, opening/closing windows, regions

# MPowerKit.Navigation.Popups

.NET MAUI popup library which allows you to open MAUI pages as a popup. Also the library allows you to use very simple and flexible animations for showing popup pages.

[![NuGet](https://github.com/MPowerKit/Navigation/tree/main/MPowerKit.Navigation.Popups)

Inspired by [Rg.Plugins.Popup](https://github.com/rotorgames/Rg.Plugins.Popup) and [Mopups](https://github.com/LuckyDucko/Mopups), but implementation is completely different. 

- It has almost the same PopupPage API as packages above, but improved animations, removed redundant properties as ```KeyboardOffset```, changed names of some properties. 

- Improved code and fixed some known bugs, eg Android window insets (system padding) or animation flickering. 

- Changed API of ```PopupService```, now you have an ability to choose a window to show/hide popup on.

- Under the hood platform specific code does not use custom renderers for ```PopupPage```.

- Hiding keyboard when tapping anywhere on popup except entry field

- ```PopupStack``` is not static from now.

- All API's are public or protected from now, so you can easily override and change implementation as you want

## Supported Platforms

* .NET8
* .NET8 for Android (min 23)
* .NET8 for iOS (min 13.0)
* .NET8 for MacCatalyst (min 13.1)
* .NET8 for Windows (min 10.0.17763.0)

Note: .NET8 for Tizen is not supported, but your PRs are welcome.

## Setup

Add ```UsePopupNavigation()``` to your MauiProgram.cs file as next

```csharp
 builder
 .UseMauiApp<App>()
 .UseMPowerKit(b =>
 {
   b.ConfigureServices(s =>
 {
   s.RegisterForNavigation<MainPage>();
   s.RegisterForNavigation<PopupPageTest>();
})
  .UsePopupNavigation()
  .OnAppStart("NavigationPage/MainPage");
```

Show Popups:

```csharp

ValueTask<NavigationResult> ShowPopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true, Action<Confirmation>? closeAction = null);

await _popupService.ShowPopupAsync("PopupPageTest", null, true);
```

To show Confirmation Popup:

```csharp
ValueTask<PopupResult> ShowAwaitablePopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true);

await _popupService.ShowAwaitablePopupAsync("PopupPageTest", null, true);
```

Hide Popups:

```csharp
ValueTask<NavigationResult> HidePopupAsync(bool animated = true);

await _popupNavigationService.HidePopupAsync();
```

```csharp
ValueTask<NavigationResult> HidePopupAsync(PopupPage page, bool animated = true);

```
https://github.com/MPowerKit/Navigation/assets/102964211/2a0003c2-d6a8-4a6a-91f8-98fd46e8bd71




