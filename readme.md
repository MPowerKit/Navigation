# MPowerKit .NET MAUI MVVM navigation framework. 

#### Supports regular/modal navigation, opening/closing windows, multiple windows, regions, popups

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/alexdobrynin)

Inspired by [Prism](https://github.com/PrismLibrary/Prism) navigation framework

##### Since MAUI's Shell navigation is a bit castrated and not suitable with building production-ready large applications and, unfortunately, Prism Library for .NET MAUI has some critical (for our company) bugs and different behavior comparing to Prism.Forms and since Prism became paid, we decided to write our own navigation framework. This library brings you the same principle for navigation through the MAUI app as Prism, but has absolutely different implementation and a bit improved performance. It also brings (in our opinion) proper way to handle 'System back button' click, it works and has same behavior for all platforms.

## Available Nugets

| Framework | Nuget |
|-|-|
| [MPowerKit.Navigation.Core](https://www.nuget.org/packages/MPowerKit.Navigation.Core) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation.Core)](https://www.nuget.org/packages/MPowerKit.Navigation.Core) |
| [MPowerKit.Navigation](https://www.nuget.org/packages/MPowerKit.Navigation) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation)](https://www.nuget.org/packages/MPowerKit.Navigation) |
| [MPowerKit.Navigation.Popups](https://www.nuget.org/packages/MPowerKit.Navigation.Popups) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Navigation.Popups)](https://www.nuget.org/packages/MPowerKit.Navigation.Popups) |
| [MPowerKit.Navigation.Regions](https://www.nuget.org/packages/MPowerKit.Regions) | [![Nuget](https://img.shields.io/nuget/v/MPowerKit.Regions)](https://www.nuget.org/packages/MPowerKit.Regions) |

- [MPowerKit.Navigation.Core](#MPowerKitNavigationCore)
    - [Awares](#Awares)
        - [IInitializeAware](#IInitializeAware)
        - [IDestructible](#IDestructible)
        - [INavigationAware](#INavigationAware)
        - [IPageLifecycleAware](#IPageLifecycleAware)
        - [IWindowLifecycleAware](#IWindowLifecycleAware)
        - [ISystemBackButtonClickAware](#ISystemBackButtonClickAware)
        - [IActiveTabAware](#IActiveTabAware)
        - [IFlyoutPageFlyoutPresentedAware](#IFlyoutPageFlyoutPresentedAware)
    - [Other useful classes](#Other-useful-classes)
        - [NavigationResult](#NavigationResult)
        - [KnownNavigationParameters](#KnownNavigationParameters)
        - [BehaviorBase](#BehaviorBase)
        - [ServiceLocatorExtensions](#ServiceLocatorExtensions)
        - [BehaviorExtensions](#BehaviorExtensions)
        - [MvvmHelpers](#MvvmHelpers)
- [MPowerKit.Navigation](#MPowerKitNavigation)
    - [Setup](#Setup)
        - [Register pages, services and behaviors](#Register-pages-services-and-behaviors)
            - [Register services](#RegisterServices)
            - [Already register services](#AlreadyRegisterServices)
            - [Register pages](#RegisterPages)
            - [Already register pages](#AlreadyRegisterPages)
            - [Register behaviors](#RegisterBehaviors)
            - [Already register behaviors](#AlreadyRegisterBehaviors)
        - [Configure app start](#Configure-app-start)
- [MPowerKit.Navigation.Popups](#MPowerKitNavigationPopups)
    - [Setup](#Setup-1)
        - [Register popup pages](#Register-popup-pages)
    - [Usage](#Usage)
        - [IPopupDialogAware](#IPopupDialogAware)
            - [Example](#Example)
        - [IPopupNavigationService](#IPopupNavigationService)
- [MPowerKit.Navigation.Regions](#MPowerKitNavigationRegions)
    - [Setup](#Setup-2)
        - [Register your region views](#Register-your-region-views)
    - [Usage](#Usage-1)
        - [IRegionManager](#IRegionManager)
            - [Example](#Example-1)
        - [IRegion](#IRegion)

---

## MPowerKit.Navigation.Core

This is the core library for [MPowerKit.Navigation](#MPowerKitNavigation) and other libraries. It contains core functionality, utility classes and interfaces.

### Awares

Same as in Prism, aware interfaces are here to know when and what page event happens. For your page / viewmodel to be awared for particular event it should implement that aware interface.

<details>
<summary><a name="IInitializeAware">IInitializeAware</a></summary>
<br>

It has only one interface method ```void Initialize(INavigationParameters parameters);```.
This method is executed right after the page and it's viewmodel has been created and not attached to visual tree. Executes for each page in navigation stack continuously in direct order. Accepts ```INavigationParameters``` as input arguments. Executed only once during the lifetime of the page.
If you want your page or it's viewmodel know about this event, it must implement this interface.

It's purpose to get ```INavigationParameters``` and probably to subscribe to some events.

**Note: There is no ```IInitializeAsyncAware``` interface, because in mobile development it is not a good practise calling async methods when your page is not atatched to visual tree**

</details>

<details>
<summary><a name="IDestructible">IDestructible</a></summary>
<br>

It has only one interface method ```void Desctroy();```.
This method is executed right after the page and it's viewmodel has been detached from visual tree and needs to be GCed. Executes for each page in navigation stack continuously in reverse order. Executed only once during the lifetime of the page.
If you want your page or it's viewmodel know about this event, it must implement this interface.

It's purpose to unsubscribe from events and clear resources.

</details>

<details>
<summary><a name="INavigationAware">INavigationAware</a></summary>
<br>

It has two interface methods:
1. ```void OnNavigatedFrom(INavigationParameters parameters);```. This method is executed when the forward or backward navigation from the page happens.
When a new page is opened this event will be called on a previous page with ```NavigationDirection``` parameter included with the ```NavigationDirection.New``` value.
When going back this event will be called on a page is being closed with ```NavigationDirection``` parameter included with the ```NavigationDirection.Back``` value.
2. ```void OnNavigatedTo(INavigationParameters parameters);```. This method is executed when the forward or backward navigation to the page happens.
When a new page is opened this event will be called on a new page with ```NavigationDirection``` parameter included with the ```NavigationDirection.New``` value.
When going back this event will be called on a previous page in the stack with ```NavigationDirection``` parameter included with the ```NavigationDirection.Back``` value.

To get ```NavigationDirection``` value you can call ```GetNavigationDirection()``` extension method on ```INavigationParameters``` variable;

</details>

<details>
<summary><a name="IPageLifecycleAware">IPageLifecycleAware</a></summary>
<br>

This interface is tied to the ```Page``` class lifecycle events
It has two interface methods:
1. ```void OnAppearing();```. This method is executed when the page is appearing.
2. ```void OnDisappearing();```. This method is executed when the page is disappearing.

**Note: This interface should be implemented only by page viewmodels, because ```Page``` class already has these methods under the hood from MAUI**
**Note: You can get page lyfecycle events in the regions if region views / viewmodels implement this interface**

</details>

<details>
<summary><a name="IWindowLifecycleAware">IWindowLifecycleAware</a></summary>
<br>

This interface is tied to the ```Window``` class lifecycle events
It has two interface methods:
1. ```void OnResume();```.
2. ```void OnSleep();```.

**Note: You can get window lyfecycle events in the regions if region views / viewmodels implement this interface**

</details>

<details>
<summary><a name="ISystemBackButtonClickAware">ISystemBackButtonClickAware</a></summary>
<br>

If you want to have the full control over the navigation process you would want your page or it's viewmodel implement this interface.
It has only one interface method ```bool OnSystemBackButtonClick();```. This method is executed when the system back button was clicked. It works on all platfroms, supported by MAUI, even on iOS and MacCatalyst, even when a page is closed by swipe.
It returns boolean value indicating whether this event was handled or not. If handled, then the backward navigation should be handled by developer, eg call ```INavigationService.GoBackAsync()```. If not, the backward navigation will be handled by the MAUI itself. In this case no navigation or destructive events are called by MPowerKit.

</details>

<details>
<summary><a name="IActiveTabAware">IActiveTabAware</a></summary>
<br>

This interface has only one boolean property ```bool IsOnActiveTab { get; set; }``` indicating the tab page is active or not.
This interface can be implemented by the page which is the root of the tab. If the root of the tab is ```NavigationPage``` the ```IsOnActiveTab``` property will be changed on this ```NavigationPage``` and on ```RootPage``` and ```CurrentPage``` of this ```NavigationPage``` if they are implementing this interface.

**Note: It will take no effect if this property is changed manually from the tab page / viewmodel**

</details>

<details>
<summary><a name="IFlyoutPageFlyoutPresentedAware">IFlyoutPageFlyoutPresentedAware</a></summary>
<br>

This interface has only one boolean property ```bool IsFlyoutPresented { get; set; }``` indicating the ```Flyout``` is presented or not.
This interface can be implemented by the ```FlyoutPage``` viewmodel, ```Flyout``` page, ```Detail``` page. If the root of the ```Detail``` page is ```NavigationPage``` the ```IsFlyoutPresented``` property will be changed on this ```NavigationPage``` and on ```RootPage``` and ```CurrentPage``` of this ```NavigationPage``` if they are implementing this interface.

**Note: It will take no effect if this property is changed manually from the FlyOut page / viewmodel**

</details>

### Other useful classes

<details>
<summary><a name="NavigationResult">NavigationResult</a></summary>
<br>

This class is used as a return type of each navigation. It contains information about was navigation successful or not and if not it will contain an exception with error description.

</details>

<details>
<summary><a name="KnownNavigationParameters">KnownNavigationParameters</a></summary>
<br>

This is a static class with constants which are used as navigation parameters.

</details>

<details>
<summary><a name="BehaviorBase">BehaviorBase</a></summary>
<br>

This is a generic base class for all behaviors you may use in your MAUI app. It has the same ```BindingContext``` as the control it attached to, so you can easily bind to your viewmodel from behavior.

</details>

<details>
<summary><a name="ServiceLocatorExtensions">ServiceLocatorExtensions</a></summary>
<br>

This is a static class which contains methods for registering and resolving views for navigation. You may register pages as well as regular views (used by regions) for navigation.

</details>

<details>
<summary><a name="BehaviorExtensions">BehaviorExtensions</a></summary>
<br>

This is a static class which contains methods for registering behaviors and associating them with views. Then registered behaviors will be applied to the associated view, when this view created by the library.

**Note: The associated views should be registered for navigation if you want the behaviors be applied to these views**

</details>

<details>
<summary><a name="MvvmHelpers">MvvmHelpers</a></summary>
<br>

This is a static class which contains helper methods for navigation, resolving and finding parent views, applying awares and etc.

</details>

---

## MPowerKit.Navigation

WIP

This library provides all neccessary infrastructure to build rich applications full of navigation between different pages in completely MVVM manner.

### Setup

Add ```UseMPowerKitNavigation(b => { })``` to ```MauiAppBuilder``` in your MauiProgram.cs file as next

```csharp
builder
    .UseMauiApp<App>()
    .UseMPowerKitNavigation(mpowerBuilder =>
    {
        mpowerBuilder.ConfigureServices(s =>
        {
            s.RegisterForNavigation<MainPage>();
        })
        .OnAppStart("NavigationPage/MainPage");
    });
```

This is enough to start the app with the root page of ```MainPage```. 

If you need to register services or configure app start you may need set up additional settings.

#### Register pages, services and behaviors

For this action there is ```ConfigureServices(s => { })``` extension method.

<details>
<summary><a name="RegisterServices">Register services</a></summary>
<br>

Since under the hood ```ConfigureServices()``` method uses ```IServiceCollection``` object of ```MauiAppBuilder``` you can also register your services outside ```ConfigureServices``` method.
But it is way better to keep all of your registrations in one place. 

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    // Register services
    s.AddSingleton<ISingletonService, SingletonService>();
    s.AddScoped<IScopedService, ScopedService>();
    s.AddTransient<ITransientService, TransientService>();
});
```

</details>

<details>
<summary><a name="AlreadyRegisterServices">Already register services</a></summary>
<br>

There is already registered a bunch of neccessary services used by this library. The entire list you can find [here](/MPowerKit.Navigation/MPowerKitMvvmBuilder.cs#L38).

But there is some services you may need to extend or completely change registration.

1. ```MPowerKitWindow``` is registered as transient service as ```IMPowerKitWindow``` and gives an ability to handle system back button click and window lifecycle.
If you need to change ```MPowerKitWindow``` implementation or change the system back button click behavior you can extend this class and register new implementation as next:

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.AddTransient<IMPowerKitWindow, NewWindowThatExtendsMPowerKitWindow>();
});
```

2. ```NavigationService``` is registered as scoped service as ```INavigationService```. It is used for navigation through the app. The detailed descriptions is [here]().
If you need to override some basic implementations of it, you can register your implementation as next:

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.AddScoped<INavigationService, YourNavigationService>();
});
```

</details>

<details>
<summary><a name="RegisterPages">Register pages</a></summary>
<br>

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<MainPage>();
})
```

- The page will be resolved by it's ```nameof()```
- No view model is specified, which means it has ```BindingContext``` set to ```new object();```

or

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<MainPage, MainPageViewModel>();
})
```

- The page will be resolved by it's ```nameof()```
- The view model is ```MainPageViewModel```

or

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<MainPage, MainPageViewModel>("TheAssociationNameForYourPage");
})
```

- The page will be resolved by association name, which is preferred way
- The view model is ```MainPageViewModel```

</details>

<details>
<summary><a name="AlreadyRegisterPages">Already register pages</a></summary>
<br>

1. ```NavigationPage```. Note: For iOS and MacCatalyst custom navigation page renderer is registered to handle iOS/mac title bar back button click and swipe-to-close events.
2. ```TabNavigationPage```. You may use it as root page of ```TabbedPage``` tab. It has logic to pass the icon and title from it's root page to ```TabbedPage```.
3. ```TabbedPage```
4. ```FlyoutPage```

</details>

<details>
<summary><a name="RegisterBehaviors">Register behaviors</a></summary>
<br>

If you need that your pages be resolved with already attached behaviors you can easily achieve this by next:

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    // Register behaviors
    s.RegisterBehavior<Page, SomeUsefulBehaviorYouWantToAttachToEachPageInYourApp>();
    s.RegisterBehavior<SecondPage, SomeUsefulBehaviorYouWantToAttachOnlyToSecondPage>();
})
```

The behaviors should be ```typeof(Behavior)```

</details>

<details>
<summary><a name="AlreadyRegisterBehaviors">Already register behaviors</a></summary>
<br>

1. ```PageLifecycleAwareBehavior```. Responsible for handling ```OnAppearing()``` and ```OnDisappearing()``` events of the page. Registered for all pages in the app.
2. ```TabbedPageActiveTabAwareBehavior```. Responsible for handling ```CurrentPageChanged``` (active tab) event of ```TabbedPage```.
3. ```FlyoutPageFlyoutPresentedAwareBehavior```. Responsible for handling ```IsPresentedChanged``` event of ```FlyoutPage```.

</details>

#### Configure app start

For most cases using the setup below is enough to start the app on desired page.

```csharp
mpowerBuilder.OnAppStart("NavigationPage/YourPage");
```

But id you need, for example, do some initial setup of the app you may use next methods:

1. ```OnInitialized()``` executes when the MAUI app is initialized. This is not async method

It can be used as next:

```csharp
mpowerBuilder.OnInitialized(() =>
{
    //your initializations
});
```

or it can accept ```IServiceProvider``` object

```csharp
mpowerBuilder.OnInitialized(serviceProvider =>
{
    //your initializations
});
```

2. ```OnAppStart()``` executes when the app is ready to navigate to the first page. This is required method, without it the app will crash on the start. This is async method

It can be used as next:

```csharp
mpowerBuilder.OnAppStart("NavigationPage/YourPage");
```

or if you need execute some async methods before navigation, for example:

```csharp
mpowerBuilder.OnAppStart(async (serviceProvider, navigationService) =>
{
    if (await IsUserLoggedIn())
    {
        await navigationService.NavigateAsync("NaviationPage/MainPage");
    }
    else await navigationService.NavigateAsync("LoginPage");
});
```

---

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

#### Register popup pages

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<TestPopupPage>();
})
```

- The popup will be resolved by it's ```nameof()```
- No view model is specified, which means it has ```BindingContext``` set to ```new object();```

or

```csharp
mpowerBuilder.ConfigureServices(s =>
{
    s.RegisterForNavigation<TestPopupPage, TestPopupViewModel>();
})
```

- The popup will be resolved by it's ```nameof()```
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

<details>
<summary><a name="IPopupDialogAware">IPopupDialogAware</a></summary>
<br>

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

</details>

<details>
<summary><a name="IPopupNavigationService">IPopupNavigationService</a></summary>
<br>

Main unit of work of this library is ```IPopupNavigationService```. Under the hood it is registered as scoped service (NOT SINGLETONE), which means that it knows from which page it was opened to know the parent window it is attached to.
So, in theory you can open different popups in different windows in same time.

Inject ```IPopupNavigationService``` to your page's or viewmodel's contructor.

```IPopupNavigationService``` describes 4 methods:

1. Show 'fire-and-forget' popup:
```csharp
ValueTask<NavigationResult> ShowPopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true, Action<Confirmation>? closeAction = null);
```
When you invoke this method it will show the popup and the main thread will continue doing it's very important work. 
You can provide close callback which accepts ```Confirmation``` object with boolean whether confirmed or not and ```INavigationParameters``` parameters.
it invokes all necessary aware interfaces you specified for your popup or it's viewmodel.
The result of showing popup is ```NavigationResult```

2. Show awaitable popup:
```csharp
ValueTask<PopupResult> ShowAwaitablePopupAsync(string popupName, INavigationParameters? parameters = null, bool animated = true);
```
When you invoke this method it will show the popup and it will await until the popup is closed.
The reslut of this method is ```PopupResult```. ```PopupResult``` is inherited from ```NavigationResult```. It has extra property for ```Confirmation``` object to know how the popup was closed.

3. Hide the last popup from popup stack:
```csharp
ValueTask<NavigationResult> HidePopupAsync(bool animated = true);
```
Hides the last popup available in the popup stack. The stack is controlled by the [MPowerKit.Popups](https://github.com/MPowerKit/Popups) framework.

4. Hide specific popup:
```csharp
ValueTask<NavigationResult> HidePopupAsync(PopupPage page, bool animated = true);
```
Hides the specified popup if it was opened.
The difference with [MPowerKit.Popups](https://github.com/MPowerKit/Popups) that it invokes all necessary aware interfaces you specified for your popup or it's viewmodel.

</details>

---

## MPowerKit.Navigation.Regions

Like [MPowerKit.Navigation](#MPowerKit.Navigation) Regions library is very similar to [Prism's](https://github.com/PrismLibrary/Prism) one. It has same sense, but different implementation.

Shortly what it is:
In MAUI you can navigate only through pages, but what if you need to have big page with few different sections, let's call them, regions. For example: [TabView](https://github.com/MPowerKit/TabView) or some desktop screen with sections. Do we need to keep all logic in one god viewmodel? - With regions no.
It gives you simple and flexible way to navigate to the regions (sections on UI) from your page or viewmodel, or even from another region. Each region can hold as much views as you like, but only one will be visible at the moment. And you can simply put all logic related to the section inside the region viewmodel. Regions can be recursive.

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

- The region view will be resolved by it's ```nameof()```
- No view model is specified, which means it has ```BindingContext``` set to ```new object();```

or

```csharp
builder.Services
    .RegisterForNavigation<RegionView1, Region1ViewModel>();
```

- The region view will be resolved by it's ```nameof()```
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

Firstly add namespace

```xaml
xmlns:regions="clr-namespace:MPowerKit.Regions;assembly=MPowerKit.Regions"
```

and then just simple to use

```xaml
<ContentView regions:RegionManager.RegionName="YourVeryMeaningfulRegionName" />
```

or, unlike [Prism](https://github.com/PrismLibrary/Prism), it can have dynamic name, for example if you need to bind it to some ID.

```xaml
<ContentView regions:RegionManager.RegionName="{Binding DynamicString}" />
```

This is very helpful if you use it, for example, with [TabView](https://github.com/MPowerKit/TabView) and you need to open new tab with tab specific dynamic data which has region(s). With static names you are not able to do such trick.

**!!! Important: the region names MUST be unique throughout the entire app or it will crash!!!**

To remove region holder from region registrations there is hidden method ```RegionManager.RemoveHolder(string? key)```.

**Note: you should not use it, if you use [MPowerKit.Regions](#MPowerKit.Regions) in couple with [MPowerKit.Navigation](#MPowerKit.Navigation)**

<details>
<summary><a name="IRegionManager">IRegionManager</a></summary>
<br>

Inject ```IRegionManager``` to your view's or viewmodel's contructor.

This interface is registered as a singleton and describes 2 methods:

1. New navigation to the region:
```csharp
NavigationResult NavigateTo(string regionName, string viewName, INavigationParameters? parameters = null);
```
Performs navigation within an empty region holder. It creates an `IRegion` object that describes the region with a region stack and then pushes the chosen view into the region. If the region holder already contains child views, it will clear the region stack and push the new view into the region.

2. Get all child regions for chosen view:
```csharp
IEnumerable<IRegion> GetRegions(VisualElement? regionHolder);
```
Retrieves all child regions associated with a chosen region holder. It can be particularly useful when you need to clean up resources and invoke lifecycle events for these regions.

##### Example

```csharp
IRegionManager _regionManager;

_regionManger.NavigateTo("YourRegionName", "RegionViewAssociationName", optionalNavigationParametersObject);
```

</details>

<details>
<summary><a name="IRegion">IRegion</a></summary>
<br>

To use ```IRegion``` object just inject it to your region view ot it's viewmodel and then you will have the control over your region stack.

This interface is registered as scoped service. It means that each region holder contains it's own ```IRegion``` object which can be injected into each region view it holds. This object is responsible for navigation inside the region it describes.

Each region has it's region stack and ```CurrentView```. Region stack is just ```Grid``` with children. So it means that all of region views are currently attached to the visual tree but only one is visible. Visible region view is ```CurrentView```.

This interface describes 7 main methods:
1. Replace all
```csharp
NavigationResult ReplaceAll(string viewName, INavigationParameters? parameters);```
```
Replaces entire region stack, calls all implemented aware interfaces and pushes new region view to the region holder.

2. Push new view
```csharp
NavigationResult Push(string viewName, INavigationParameters? parameters);
```
Detects index of ```CurrentView``` in the stack, clears all view after ```CurrentView``` and pushes new view after ```CurrentView``` and makes it to be ```CurrentView```

3. Push new view backwards 
```csharp
NavigationResult PushBackwards(string viewName, INavigationParameters? parameters);
```
Same as ```Push``` but clears all views before ```CurrentView``` in the stack and pushes new view before ```CurrentView``` and makes it to be ```CurrentView```.

4. Go back through the stack
```csharp
NavigationResult GoBack(INavigationParameters? parameters);
```
Checks whether it can navigate back through the region stack and does backwards navigation invoking ```INavigationAware``` interface.

5. Go forward through the stack
```csharp
NavigationResult GoForward(INavigationParameters? parameters);
```
Same as ```GoBack``` but to the opposite direction.

6. Can go back
```csharp
bool CanGoBack();
```
Checks whether it can navigate back through the region stack.

7. Can go forward
```csharp
bool CanGoForward();
```
Same as ```CanGoBack``` but to the opposite direction.

Also, this interface describes another few utility methods which invoke aware interfaces.

Region views or their viewmodels can implement next aware interfaces: ```IInitializeAware```, ```INavigationAware```, ```IDestructible```, ```IWindowLifecycleAware```, ```IPageLifecycleAware```

</details>
