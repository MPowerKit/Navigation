namespace MPowerKit.Navigation.Interfaces;

public interface IWindowManager
{
    Window? InitialWindow { get; set; }
    IReadOnlyList<Window> Windows { get; }
    void CloseWindow(Window window);
    void OpenWindow(Window window);
}