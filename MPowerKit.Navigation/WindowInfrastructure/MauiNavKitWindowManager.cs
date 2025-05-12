using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.WindowInfrastructure;

public class MPowerKitWindowManager : IWindowManager
{
    public Window? InitialWindow { get; set; }
    protected readonly IApplication Application;
    protected readonly MPowerKitWindowCreator? WindowCreator;

    public MPowerKitWindowManager(IApplication application)
    {
        Application = application;
    }

    public IReadOnlyList<Window> Windows => Application.Windows.OfType<MPowerKitWindow>().ToList();

    public void OpenWindow(Window window)
    {
        if (InitialWindow is null)
            InitialWindow = window;
        else
            Application.OpenWindow(window);
    }

    public void CloseWindow(Window window)
    {
        if (InitialWindow == window)
        {
            InitialWindow = Windows.FirstOrDefault();
        }

        Application.CloseWindow(window);
    }
}