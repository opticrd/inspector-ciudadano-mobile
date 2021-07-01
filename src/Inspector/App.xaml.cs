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
            XF.Material.Forms.Material.Init(this); // WORKAROUND: Solving some reference issues
            XF.Material.Forms.Material.Use("Material.Configuration");
            Application.Current.UserAppTheme = OSAppTheme.Light;

#if DEBUG
            var result = await NavigationService.NavigateAsync("WelcomePage");
            if (!result.Success)            
                System.Diagnostics.Debugger.Break();            
#elif DEBUG_AGENT
            var result = await NavigationService.NavigateAsync("LoginPage");
            if (!result.Success)            
                System.Diagnostics.Debugger.Break();
#elif RELEASE_AGENT
            await NavigationService.NavigateAsync("LoginPage");
#else
            await NavigationService.NavigateAsync("WelcomePage");
#endif
        }

        protected override void OnStart()
        {
            base.OnStart();
#if AGENT
            // appcenter keys for inspector agents
#else
            AppCenter.Start("android=8b508ed0-50f1-4836-a73d-76a7665351bd;" +
                "ios=4afbe2f3-1f31-4fc7-8d10-21e62cea0fc9;", typeof(Analytics), typeof(Crashes));
#endif
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            //containerRegistry.RegisterForNavigation<MainTabbedPage, MainTabbedPageViewModel>();
            //containerRegistry.RegisterForNavigation<ProfilePage, ProfilePageViewModel>();
            //containerRegistry.RegisterForNavigation<ReportHistoryPage, ReportHistoryPageViewModel>();
            //containerRegistry.RegisterForNavigation<FeedReportsPage, FeedReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<AddReportPage, AddReportPageViewModel>();
            containerRegistry.RegisterForNavigation<EditProfilePage, EditProfilePageViewModel>();
            containerRegistry.RegisterForNavigation<HomePage, HomePageViewModel>();
            containerRegistry.RegisterForNavigation<ReportDetailPage>();

#if AGENT
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
#endif
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
#if !AGENT
        moduleCatalog.AddModule<LoginModule.LoginModule>(InitializationMode.WhenAvailable);
#endif
        }
    }
}
