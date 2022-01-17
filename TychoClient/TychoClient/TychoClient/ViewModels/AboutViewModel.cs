using Plugin.NFC;
using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";

            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            CrossNFC.Current.StartListening();

            OpenWebCommand = new Command(() => Device.OpenUri(new Uri("https://xamarin.com/platform")));
        }

        private void Current_OnMessageReceived(ITagInfo tagInfo)
        {
            Title = "SCANNED A TAG!";
            
        }

        public ICommand OpenWebCommand { get; }
    }
}