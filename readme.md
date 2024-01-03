# MPowerKit .NET MAUI MVVM navigation framework. 

#### Supports regular/modal navigation, opening/closing windows, multiple windows, regions, popups

## Available Nugets

| Framework | Nuget |
|-|-|
| [MPowerKit.Navigation.Core](#MPowerKitNavigationCore) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation.Core)](https://www.nuget.org/packages/MPowerKit.Navigation.Core) |
| [MPowerKit.Navigation](#MPowerKitNavigation) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation)](https://www.nuget.org/packages/MPowerKit.Navigation) |
| [MPowerKit.Navigation.Popups](#MPowerKitNavigationPopups) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation.Popups)](https://www.nuget.org/packages/MPowerKit.Navigation.Popups) |
| [MPowerKit.Navigation.Regions](#MPowerKitNavigationRegions) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Regions)](https://www.nuget.org/packages/MPowerKit.Regions) |

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
    .UseMPowerKitNavigation(mpowerBuilder =>
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

- The popup will be resolved by it's __nameof()__
- No view model is specified, which means it has ```BindingContext``` set to ```new object();```

or

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<TestPopupPage, TestPopupViewModel>();
})
```

- The popup will be resolved by it's __nameof()__
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

Each popup page must inherit from ```PopupPage``` of [MPowerKit.Popups](https://github.com/MPowerKit/Popups) library

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

## MPowerKit.Navigation.Regions

Like [MPowerKit.Navigation](#MPowerKit.Navigation) Regions library is very similar to [Prism's](https://github.com/PrismLibrary/Prism). It has same sense, but different implementation.

Shortly what it is: 
In MAUI you can navigate only through pages, but what if you need to have big page with few different sections, let's call them, regions. For example: [TabView](https://github.com/MPowerKit/TabView) or some desktop screen with sections. Do we need to keep all logic in one viewmodel? - With regions no.
It gives you simple and flexible way to navigate to the regions (sections on UI) from your page or viewmodel, or even from another region.

### Setup

Add ```UseMPowerKitRegions()``` to your MauiProgram.cs file as next

```csharp
builder
    .UseMauiApp<App>()
    .UseMPowerKitRegions();
```

Regions can work with(out) [MPowerKit.Navigation](#MPowerKit.Navigation) or [MPowerKit.Navigation.Popups](#MPowerKit.Navigation.Popups).

```csharp
builder
    .UseMauiApp<App>()
    .UseMPowerKitNavigation(mpowerBuilder =>
    {
        mpowerBuilder.ConfigureServices(s =>
        {
            s.RegisterForNavigation<MainPage>();
            s.RegisterForNavigation<RegionView1>();
        })
        .OnAppStart("NavigationPage/MainPage");
    })
    .UseMPowerKitRegions();
```


**Note: if you are using regions in couple with [MPowerKit.Navigation](#MPowerKit.Navigation) you can specify whether you want your region views get parent page's events like navigation, destroy, lifecycle etc.
Just add ```UsePageEventsInRegions()``` to ```mpowerBuilder```**



```csharp
builder
    .UseMauiApp<App>()
    .UseMPowerKitNavigation(mpowerBuilder =>
    {
        mpowerBuilder.ConfigureServices(s =>
        {
            s.RegisterForNavigation<MainPage>();
            s.RegisterForNavigation<RegionView1>();
        })
        .UsePageEventsInRegions()
        .OnAppStart("NavigationPage/MainPage");
    })
    .UseMPowerKitRegions();
```

#### Register your region views

```csharp
builder.Services
    .RegisterForNavigation<RegionView1>();
```

- The region view will be resolved by it's __nameof()__
- No view model is specified, which means it has ```BindingContext``` set to ```new object();```

or

```csharp
builder.Services
    .RegisterForNavigation<RegionView1, Region1ViewModel>();
```

- The region view will be resolved by it's __nameof()__
- The view model is ```Region1ViewModel```

or

```csharp
builder.Services
    .RegisterForNavigation<RegionView1, Region1ViewModel>("RegionViewAssociationName");
```

- The region view will be resolved by association name, which is preferred way
- The view model is ```Region1ViewModel```

### Usage

Each region should have the parent container which will be the so-called region holder. This region holder has to be ```typeof(ContentView)```.

##### In your xaml:

Firstly add namespace

```csharp
xmlns:regions="clr-namespace:MPowerKit.Regions;assembly=MPowerKit.Regions"
```

and then just simple to use

```csharp
<ContentView regions:RegionManager.RegionName="YourVeryMeaningfulRegionName" />
```

or, unlike [Prism](https://github.com/PrismLibrary/Prism), it can have dynamic name, for example if you need to bind it to some ID.

```csharp
<ContentView regions:RegionManager.RegionName="{Binding DynamicString}" />
```

This is very helpful if you use it, for example, with [TabView](https://github.com/MPowerKit/TabView) and you need to open new tab with tab specific dynamic data which has region(s). With static names you are not able to do such trick.


**!!! Important: the region names MUST be unique throughout the entire app or it will crash!!!**


To remove region holder from region registrations there is hidden method ```RegionManager.RemoveHolder(string? key)```.


**Note: you SHOULD NOT use it, if you specified ```UsePageEventsInRegions()```**


#### IRegionManager

This interface is registered as a singleton and consists of two methods:

```csharp
1. NavigationResult NavigateTo(string regionName, string viewName, INavigationParameters? parameters = null);
```
   This method performs navigation within an empty region holder. It creates an `IRegion` object that describes the region with a region stack and then pushes the chosen view into the region. If the region holder already contains child views, it will clear the region stack and push the new view into the region.
   
```csharp
2. IEnumerable<IRegion> GetRegions(VisualElement? regionHolder);
```
   This method retrieves all child regions associated with a chosen region holder. It can be particularly useful when you need to clean up resources and invoke lifecycle events for these regions.

##### Example

```csharp
IRegionManager _regionManager;

_regionManger.NavigateTo("YourRegionName", "RegionViewAssociationName", optionalNavigationParametersObject);
```

#### IRegion

This interface is registered as scoped service. It means that each region holder contains it's own ```IRegion``` object which can be injected into each region view it holds. This object is responsible for navigation inside the region it describes.

Each region has it's region stack and ```CurrentView```. Region stack is just ```Grid``` with children. So it means that all of region views are currently attached to the visual tree but only one is visible. Visible region view is ```CurrentView```.

This interface has 7 main methods:
1. ```NavigationResult ReplaceAll(string viewName, INavigationParameters? parameters);```
Replaces entire region stack, calls all implemented aware interfaces and pushes new region view to the region holder.
2. ```NavigationResult Push(string viewName, INavigationParameters? parameters);```
Detects index of ```CurrentView``` in the stack, clears all view after ```CurrentView``` and pushes new view after ```CurrentView``` and makes it to be ```CurrentView```
3. ```NavigationResult PushBackwards(string viewName, INavigationParameters? parameters);```
Same as ```Push``` but clears all views before ```CurrentView``` in the stack and pushes new view before ```CurrentView``` and makes it to be ```CurrentView```.
4. ```NavigationResult GoBack(INavigationParameters? parameters);```
Checks whether it can navigate back through the region stack and does backwards navigation invoking ```INavigationAware``` interface.
5. ```NavigationResult GoForward(INavigationParameters? parameters);```
Same as ```GoBack``` but to the opposite direction.
6. ```bool CanGoBack();```
Checks whether it can navigate back through the region stack.
7. ```bool CanGoForward();```
Same as ```CanGoBack``` but to the opposite direction.

Also, this interface has another few utility methods which invoke aware interfaces.

Region views or their viewmodels can implement next aware interfaces: ```IInitializeAware```, ```INavigationAware```, ```IDestructible```, ```IWindowLifecycleAware```, ```IPageLificycleAware```

To use ```IRegion``` object just inject it to your region view ot it's viewmodel and then you will have the control over your region stack.
