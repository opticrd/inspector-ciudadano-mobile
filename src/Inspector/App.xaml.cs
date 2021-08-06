using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Inspector.Framework.Helpers;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.ViewModels;
using Inspector.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Refit;
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

            if (!Settings.IsLoggedIn)
            {
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
            containerRegistry.RegisterForNavigation<ReportDetailPage, ReportDetailPageViewModel>();
            containerRegistry.RegisterForNavigation<GalleryPage, GalleryPageViewModel>();

#if RELEASE_AGENT || DEBUG_AGENT
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<SignupDocumentPage, SignupDocumentPageViewModel> ();
#endif

            // Registering types
            //containerRegistry.RegisterSingleton<INetworkRepository, NetworkRepository>();
            containerRegistry.RegisterSingleton<ICacheService, CacheService>();
            containerRegistry.RegisterForNavigation<PreviewGalleryPage, PreviewGalleyPageViewModel>();

            var territorialClient = RestService.For<ITerritorialDivisionAPI>(new HttpClient() { BaseAddress = new Uri(AppKeys.TerritorialDivisionApiBaseUrl) });
            containerRegistry.RegisterInstance(territorialClient);

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
