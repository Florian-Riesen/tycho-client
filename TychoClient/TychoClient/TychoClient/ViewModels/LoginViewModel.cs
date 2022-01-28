using Plugin.NFC;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;

using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string UserName
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            Title = "Login";

            LoginCommand = new Command(Login);
        }

        private void Login()
        {
        	// check entered login against saved AES hash
        	// if it matches, unlock admin features
        	// save login and password
        }
    }
}