namespace MPowerKit.Navigation.Interfaces;

public interface IMPowerKitWindow : IWindow
{
    Page? CurrentPage { get; }
    Page? Page { get; set; }
}