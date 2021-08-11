using Xamarin.Essentials;
using Xamarin.Forms;

namespace Inspector.Views
{
    public partial class AppInfoPage : ContentPage
    {
        public AppInfoPage()
        {
            InitializeComponent();
            appName.Text = AppInfo.Name;
            appVersion.Text = AppInfo.VersionString;
        }
    }
}
