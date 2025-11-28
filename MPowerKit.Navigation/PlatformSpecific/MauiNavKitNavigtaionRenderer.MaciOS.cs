using Foundation;

using Microsoft.Maui.Controls.Handlers.Compatibility;

using UIKit;

namespace MPowerKit.Navigation.PlatformSpecific;

public class MPowerKitNavigationRenderer : NavigationRenderer
{
    public override void ViewWillAppear(bool animated)
    {
        InteractivePopGestureRecognizer.Delegate = new SwipeBackDelegate(this);

        base.ViewWillAppear(animated);
    }

    public override void ViewWillDisappear(bool animated)
    {
        InteractivePopGestureRecognizer.Delegate = null;

        base.ViewWillDisappear(animated);
    }

    [Export("navigationBar:shouldPopItem:")]
    public virtual bool ShouldPopItem(UINavigationBar navBar, UINavigationItem navItem)
    {
        var window = ((this as IViewHandler)?.VirtualView as NavigationPage)?.Window as IWindow;
        var should = !(window?.BackButtonClicked() ?? false);

        var field = typeof(NavigationRenderer).GetField("_uiRequestedPop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(this, should);

        return should;
    }

    public class SwipeBackDelegate : UIGestureRecognizerDelegate
    {
        protected MPowerKitNavigationRenderer NavRenderer { get; }

        public SwipeBackDelegate(MPowerKitNavigationRenderer navRenderer)
        {
            NavRenderer = navRenderer;
        }

        public override bool ShouldBegin(UIGestureRecognizer recognizer)
        {
            return NavRenderer.ShouldPopItem(default, default);
        }
    }
}