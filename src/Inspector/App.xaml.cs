using System;
using Inspector.ViewModels;
using Inspector.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Inspector
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer initializer) : base(initializer) { }


        protected override async void OnInitialized()
        {
            XF.Material.Forms.Material.Init(this);
            InitializeComponent();
            XF.Material.Forms.Material.Use("Material.Configuration");
            Application.Current.UserAppTheme = OSAppTheme.Light;

            var result = await NavigationService.NavigateAsync("NavigationPage/HomePage");
#if DEBUG
            if (!result.Success)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif
        }

        protected override void OnStart()
        {
            base.OnStart();
            AppCenter.Start("android=8b508ed0-50f1-4836-a73d-76a7665351bd;" +
                            "ios=4afbe2f3-1f31-4fc7-8d10-21e62cea0fc9;",
                            typeof(Analytics), typeof(Crashes));
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainTabbedPage, MainTabbedPageViewModel>();
            containerRegistry.RegisterForNavigation<ProfilePage, ProfilePageViewModel>();
            containerRegistry.RegisterForNavigation<AddReportPage, AddReportPageViewModel>();
            containerRegistry.RegisterForNavigation<ReportHistoryPage, ReportHistoryPageViewModel>();
            containerRegistry.RegisterForNavigation<FeedReportsPage, FeedReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<EditProfilePage, EditProfilePageViewModel>();
            containerRegistry.RegisterForNavigation<HomePage, HomePageViewModel>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<LoginModule.LoginModule>(InitializationMode.WhenAvailable);
        }
    }
}
