using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.WindowInfrastructure;

public class MPowerKitWindowCreator : IWindowCreator
{
    public MPowerKitWindowCreator(IWindowManager windowManager)
    {
        WindowManager = windowManager;
    }

    protected IWindowManager WindowManager { get; }

    public virtual Window CreateWindow(Application app, IActivationState? activationState)
    {
        if (WindowManager.InitialWindow is not null)
            return WindowManager.InitialWindow;
        else if (app.Windows.OfType<MPowerKitWindow>().Any())
            return WindowManager.InitialWindow = app.Windows.OfType<MPowerKitWindow>().First();

        activationState!.Context.Services.GetRequiredService<MPowerKitMvvmBuilder>().OnAppStarted(activationState.Context.Services);

        return WindowManager.InitialWindow ?? throw new InvalidNavigationException("Expected navigation Failed. No Root Window has been created. Try to look at navigation result.");
    }
}