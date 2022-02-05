using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.NFC;
using TychoClient.Services;
using Android.Content;

namespace TychoClient.Droid
{
    [Activity(Label = "TychoClient", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.Log("OnCreate called. Loading application.");

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            // Plugin NFC: Initialization
            CrossNFC.Init(this);

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Plugin NFC: Restart NFC listening on resume (needed for Android 10+) 
            CrossNFC.OnResume();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // Plugin NFC: Tag Discovery Interception
            CrossNFC.OnNewIntent(intent);
        }
    }
}