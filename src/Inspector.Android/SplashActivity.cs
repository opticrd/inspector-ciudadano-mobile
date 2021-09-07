using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Util;
using Android.Content;

namespace Inspector.Droid
{
#if RELEASE || DEBUG
    [Activity(Label="Inspector", Icon = "@mipmap/ic_launcher", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
#else
    [Activity(Label = "Reportero", Icon = "@mipmap/ic_launcher", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
#endif
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Log.Debug(TAG, "Performing some startup work that takes a bit of time.");
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            Log.Debug(TAG, "Startup work is finished - starting MainActivity.");
        }

        public override void OnBackPressed() { }
    }

}
