using System;
using System.Net.Http;
using System.Net.Http.Headers;
using DryIoc;
using Inspector.Framework.Helpers;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.ViewModels;
using Inspector.ViewModels.Signup;
using Inspector.Views;
using Inspector.Views.Signup;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Logging.AppCenter;
using Prism.Modularity;
using Refit;
using Xamarin.Forms;

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
            Xamarin.Forms.Application.Current.UserAppTheme = OSAppTheme.Light;

            
            if (!Settings.IsLoggedIn)
            {
#if DEBUG
            var result = await NavigationService.NavigateAsync("WelcomePage");
            if (!result.Success)            
                System.Diagnostics.Debugger.Break();            
#elif DEBUG_AGENT
            var result = await NavigationService.NavigateAsync("WelcomePage");
            if (!result.Success)            
                System.Diagnostics.Debugger.Break();
#elif RELEASE_AGENT
            await NavigationService.NavigateAsync("WelcomePage");
#else
            await NavigationService.NavigateAsync("WelcomePage");
#endif
            }
            else
            {
                await NavigationService.NavigateAsync(NavigationKeys.HomePage);
            }
        }


        protected override void OnStart()
        {
            base.OnStart();
#if RELEASE_AGENT || DEBUG_AGENT
            // appcenter keys for inspector agents
            AppCenter.Start("android=1090a168-34ba-434f-ab47-7bef6201cd16;" +
                  "ios=0adb06a3-f245-4250-90c8-1fc488d6ee17",
                  typeof(Analytics), typeof(Crashes), typeof(Distribute));
#else
            AppCenter.Start("android=8b508ed0-50f1-4836-a73d-76a7665351bd;" +
                "ios=4afbe2f3-1f31-4fc7-8d10-21e62cea0fc9;", typeof(Analytics), typeof(Crashes), typeof(Distribute));
#endif
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var container = Container.GetContainer();
            container.RegisterMany<AppCenterLogger>(Reuse.Singleton,
                        ifAlreadyRegistered: IfAlreadyRegistered.Replace,
                        serviceTypeCondition: t => typeof(AppCenterLogger).ImplementsServiceType(t));

            containerRegistry.RegisterForNavigation<NavigationPage>();
            //containerRegistry.RegisterForNavigation<MainTabbedPage, MainTabbedPageViewModel>();
            //containerRegistry.RegisterForNavigation<ProfilePage, ProfilePageViewModel>();
            //containerRegistry.RegisterForNavigation<ReportHistoryPage, ReportHistoryPageViewModel>();
            //containerRegistry.RegisterForNavigation<FeedReportsPage, FeedReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<WelcomePage, WelcomePageViewModel>();
            containerRegistry.RegisterForNavigation<AddReportPage, AddReportPageViewModel>();
            containerRegistry.RegisterForNavigation<EditProfilePage, EditProfilePageViewModel>();
            containerRegistry.RegisterForNavigation<HomePage, HomePageViewModel>();
            containerRegistry.RegisterForNavigation<ReportDetailPage, ReportDetailPageViewModel>();
            containerRegistry.RegisterForNavigation<GalleryPage, GalleryPageViewModel>();
            containerRegistry.RegisterForNavigation<AppInfoPage, AppInfoPageViewModel>();

#if RELEASE_AGENT || DEBUG_AGENT
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<SignupDocumentPage, SignupDocumentPageViewModel>();
            containerRegistry.RegisterForNavigation<SignupLocationPage, SignupLocationPageViewModel>();
            containerRegistry.RegisterForNavigation<SignupSocialMediaPage, SignupSocialMediaPageViewModel>();
#endif

            // Registering types
            //containerRegistry.RegisterSingleton<INetworkRepository, NetworkRepository>();
            containerRegistry.RegisterSingleton<ICacheService, CacheService>();
            containerRegistry.RegisterForNavigation<PreviewGalleryPage, PreviewGalleyPageViewModel>();

            var territorialClient = RestService.For<ITerritorialDivisionAPI>(new HttpClient() { BaseAddress = new Uri(AppKeys.TerritorialDivisionApiBaseUrl) });
            containerRegistry.RegisterInstance(territorialClient);

            var incidentsClient = RestService.For<IIncidentsAPI>(new HttpClient() { BaseAddress = new Uri(AppKeys.IncidentsApiBaseUrl) });
            containerRegistry.RegisterInstance(incidentsClient);

            var keycloakClient = RestService.For<IKeycloakApi>(AppKeys.KeycloakBaseUrl, new RefitSettings(new NewtonsoftJsonContentSerializer(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })));
            containerRegistry.RegisterInstance(keycloakClient);

            var zammadClientLite = RestService.For<IZammadLiteApi>(AppKeys.ZammadApiBaseUrl, new RefitSettings(new NewtonsoftJsonContentSerializer(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })));
            containerRegistry.RegisterInstance(zammadClientLite);

            var iamclient = new HttpClient() { BaseAddress = new Uri(AppKeys.IAmApiBaseUrl) };
            iamclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", AppKeys.IamAuthToken);
            var iamService = RestService.For<IAMAPI>(iamclient);
            containerRegistry.RegisterInstance(iamService);

            var citizenClient = RestService.For<ICitizenAPI>(new HttpClient(new AuthHeaderHandler(iamService)) { BaseAddress = new Uri(AppKeys.DigitalGobApiBaseUrl) });
            containerRegistry.RegisterInstance(citizenClient);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
#if DEBUG || RELEASE
        moduleCatalog.AddModule<LoginModule.LoginModule>(InitializationMode.WhenAvailable);
#endif
        }
    }
}
