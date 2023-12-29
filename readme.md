# MPowerKit .NET MAUI MVVM navigation framework. 

#### Supports regular/modal navigation, opening/closing windows, multiple windows, regions, popups

## Available Nugets

| Framework | Nuget |
|-|-|
| [MPowerKit.Navigation.Core](#MPowerKit.Navigation.Core) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation.Core)](https://www.nuget.org/packages/MPowerKit.Navigation.Core) |
| [MPowerKit.Navigation](#MPowerKit.Navigation) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation)](https://www.nuget.org/packages/MPowerKit.Navigation) |
| [MPowerKit.Navigation.Popups](#MPowerKit.Navigation.Popups) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation.Popups)](https://www.nuget.org/packages/MPowerKit.Navigation.Popups) |
| [MPowerKit.Navigation.Regions](#MPowerKit.Navigation.Regions) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Regions)](https://www.nuget.org/packages/MPowerKit.Regions) |

## MPowerKit.Navigation.Core

WIP

## MPowerKit.Navigation

WIP

## MPowerKit.Navigation.Popups

This library based on [MPowerKit.Navigation](#MPowerKit.Navigation) and [MPowerKit.Popups](https://github.com/MPowerKit/Popups) libraries

Main unit of work of this library is ```IPopupNavigationService```. Under the hood it is registered as scoped service (NOT SINGLETONE), which means that it knows from which page it was opened to know the parent window it is attached to.
So, in theory you can open different popups in different windows in same time.

### Setup

Add ```UsePopupNavigation()``` to ```MPowerKitBuilder``` in your MauiProgram.cs file as next

```csharp
    builder
    .UseMauiApp<App>()
    .UseMPowerKit(mpowerBuilder =>
    {
        mpowerBuilder.ConfigureServices(s =>
        {
            s.RegisterForNavigation<MainPage>();
            s.RegisterForNavigation<TestPopupPage>();
        })
        .UsePopupNavigation();
    })
    .OnAppStart("NavigationPage/MainPage");
```

When you specify ```.UsePopupNavigation()``` it registers ```MPowerKitPopupsWindow``` as main class for every window, it is responsible for system back button.
It inherits ```MPowerKitWindow``` which is main class for window in [MPowerKit.Navigation](#MPowerKit.Navigation), it also responsible for system back button on every platform, even in mac and ios (top-left back button on the page's toolbar)

All popup pages should inherit ```PopupPage``` of [MPowerKit.Popups](https://github.com/MPowerKit/Popups) library

##### Register your popup pages

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<TestPopupPage>();
})
```

- The popup will be resolved by it's string name
- No view model is secified, which means it has ```BindingContext``` set to ```new object();```

or

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<TestPopupPage, TestPopupViewModel>();
})
```

- The popup will be resolved by it's string name
- The view model is ```TestPopupViewModel```

or

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<TestPopupPage, TestPopupViewModel>("TheAssociationNameForYourPopup");
})
```

- The popup will be resolved by association name, which is preferred way
- The view model is ```TestPopupViewModel```

### Usage

Inject ```IPopupNavigationService``` to your page's or viewmodel's contructor

```IPopupNavigationService``` describes 4 methods:

##### Show 'fire-and-forget' popup:
```csharp
ValueTask<NavigationResult> ShowPopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true, Action<Confirmation>? closeAction = null);
```
When you invoke this method it will show the popup and main thread will continue doing it's very important work. 
You can provide close callback which accepts ```Confirmation``` object with boolean whether confirmed or not and ```INavigationParameters``` parameters.

The result of showing popup is ```NavigationResult```

##### Show awaitable popup:
```csharp
ValueTask<PopupResult> ShowAwaitablePopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true);
```
When you invoke this method it will show the popup and it will await until user interaction on popup.

https://github.com/MPowerKit/Navigation/assets/102964211/2a0003c2-d6a8-4a6a-91f8-98fd46e8bd71

## MPowerKit.Navigation.Regions

WIP