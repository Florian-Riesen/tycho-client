using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TychoClient.Views;
using TychoClient.Services;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TychoClient
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            this.Log("Creating MainPage.");
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            DataStore.GetInstance().ForceRefreshCache();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
