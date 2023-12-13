namespace MPowerKit.Navigation.Interfaces;

public interface IMPowerKitWindow
{
    Page? CurrentPage { get; }
    Page? Page { get; set; }
}