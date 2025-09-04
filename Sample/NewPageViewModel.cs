using MPowerKit.Navigation.Awares;

namespace Sample
{
    public class NewPageViewModel : ISystemBackButtonClickAware
    {
        public bool OnSystemBackButtonClick()
        {
            return false;
        }
    }
}
