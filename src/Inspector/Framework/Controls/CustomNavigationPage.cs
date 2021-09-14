using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Inspector.Framework.Controls
{
    public class CustomNavigationPage : Xamarin.Forms.NavigationPage
    {
        public CustomNavigationPage()
        {
            On<iOS>().SetHideNavigationBarSeparator(true);
            //var x = On<iOS>().HideNavigationBarSeparator();
        }
    }
}
