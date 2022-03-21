using System;
using TychoClient.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TychoClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPageWithPopup
    {
        public LoginPage()
        {
            InitializeComponent();
        }
    }
}