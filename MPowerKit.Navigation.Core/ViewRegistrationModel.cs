namespace MPowerKit.Navigation;

public class ViewRegistrationModel
{
    public Type View { get; set; }
    public Type? ViewModel { get; set; }
    public string Name { get; set; }
}