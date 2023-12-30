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
        .UsePopupNavigation()
        .OnAppStart("NavigationPage/MainPage");
    });
```

When you specify ```.UsePopupNavigation()``` it registers ```MPowerKitPopupsWindow``` as main class for every window, it is responsible for system back button.
It inherits ```MPowerKitWindow``` which is main class for window in [MPowerKit.Navigation](#MPowerKit.Navigation), it also responsible for system back button on every platform, even in mac and ios (top-left back button on the page's toolbar)

#### Register your popup pages

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<TestPopupPage>();
})
```

- The popup will be resolved by it's string name
- No view model is specified, which means it has ```BindingContext``` set to ```new object();```

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

All popup pages should inherit from ```PopupPage``` of [MPowerKit.Popups](https://github.com/MPowerKit/Popups) library

#### IPopupDialogAware

To have full control over the popup flow it is better that your popup or popup's viewmodel implement this interface. This interface gives you an ability to close popup programmatically from popup or it's viewmodel.
```IPopupDialogAware``` interface provides only one property ```RequestClose```. It is an ```Action```. You should call it when you want to close the popup. It accepts a tuple with ```Confirmation``` object and a boolean whether animated or not.
The value for ```RequestClose``` property is set under the hood by the framework, so you don't need to do smth extra with it.

```Confirmation``` is a record which accepts 2 parameters: 
1. Boolean whether confirmed or not; 
2. ```INavigationParameters``` to pass the parameters back from popup to popup caller (it is optional).

##### Example

```csharp
public class TestPopupViewModel : IPopupDialogAware
{
    public Action<(Confirmation Confirmation, bool Animated)> RequestClose { get; set; }

    protected virtual async Task Cancel(object obj = null)
    {
        var nparams = new NavigationParameters
        {
            { NavigationConstants.CloseParameter, obj }
        };

        RequestClose?.Invoke((new Confirmation(false, nparams), true));
    }

    protected virtual async Task Confirm(object obj = null)
    {
        var nparams = new NavigationParameters
        {
            { NavigationConstants.CloseParameter, obj }
        };

        RequestClose?.Invoke((new Confirmation(true, nparams), true));
    }
}
```

#### IPopupNavigationService

Main unit of work of this library is ```IPopupNavigationService```. Under the hood it is registered as scoped service (NOT SINGLETONE), which means that it knows from which page it was opened to know the parent window it is attached to.
So, in theory you can open different popups in different windows in same time.

Inject ```IPopupNavigationService``` to your page's or viewmodel's contructor.

```IPopupNavigationService``` describes 4 methods:

##### Show 'fire-and-forget' popup:
```csharp
ValueTask<NavigationResult> ShowPopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true, Action<Confirmation>? closeAction = null);
```
When you invoke this method it will show the popup and the main thread will continue doing it's very important work. 
You can provide close callback which accepts ```Confirmation``` object with boolean whether confirmed or not and ```INavigationParameters``` parameters.
it invokes all necessary aware interfaces you specified for your popup or it's viewmodel.
The result of showing popup is ```NavigationResult```

##### Show awaitable popup:
```csharp
ValueTask<PopupResult> ShowAwaitablePopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true);
```
When you invoke this method it will show the popup and it will await until the popup is closed.
The reslut of this method is ```PopupResult```. ```PopupResult``` is inherited from ```NavigationResult```. It has extra property for ```Confirmation``` object to know how the popup was closed.

##### Hide the last popup from popup stack:

```csharp
ValueTask<NavigationResult> HidePopupAsync(bool animated = true);
```

Hides the last popup available in the popup stack. The stack is controlled by the [MPowerKit.Popups](https://github.com/MPowerKit/Popups) framework.

##### Hide specific popup:

```csharp
ValueTask<NavigationResult> HidePopupAsync(PopupPage page, bool animated = true);
```

Hides the specified popup if it was opened.
The difference with [MPowerKit.Popups](https://github.com/MPowerKit/Popups) that it invokes all necessary aware interfaces you specified for your popup or it's viewmodel.

https://github.com/MPowerKit/Navigation/assets/102964211/2a0003c2-d6a8-4a6a-91f8-98fd46e8bd71

## MPowerKit.Navigation.Regions

WIP