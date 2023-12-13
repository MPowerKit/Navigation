using Foundation;

using Microsoft.Maui.Controls.Handlers.Compatibility;

using UIKit;

namespace MPowerKit.Navigation.PlatfromSpecific;

public class MPowerKitNavigtaionRenderer : NavigationRenderer
{
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        this.InteractivePopGestureRecognizer.Delegate = new SwipeBackDelegate(this);
    }

    public override void ViewWillDisappear(bool animated)
    {
        this.InteractivePopGestureRecognizer.Delegate = null;

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
        protected MPowerKitNavigtaionRenderer NavRenderer { get; }

        public SwipeBackDelegate(MPowerKitNavigtaionRenderer navRenderer)
        {
            NavRenderer = navRenderer;
        }

        public override bool ShouldBegin(UIGestureRecognizer recognizer)
        {
            return NavRenderer.ShouldPopItem(default, default);
        }
    }
}