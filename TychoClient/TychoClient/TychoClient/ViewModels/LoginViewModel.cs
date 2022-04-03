using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private IPopupViewModel _currentPopup;
        public IPopupViewModel CurrentPopup
        {
            get => _currentPopup;
            set => SetProperty(ref _currentPopup, value);
        }

        private string _username = "";
        public string UserName
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }


        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            Title = "Login";
            CurrentPopup = PopupService.GetInvisiblePopup();
            LoginCommand = new Command(Login);
        }

        private void Login()
        {
            if(IsLoggedIn)
            {
                UserName = "";
                Password = "";
            }
            IsLoggedIn = !IsLoggedIn;
            LoginData.Username = UserName;
            LoginData.Password = Password;
            LoginData.IsAdmin = HashStuff.IsAdminPassword(Password);
        }
    }

    class HashStuff
    {
        public static bool IsAdminPassword(string enteredPassword)
        {
            // DEBUG
            return ! (enteredPassword == "user");

            byte[] adminPwHash = LoadAdminPwHash();
            byte[] userPwBytes = Encoding.ASCII.GetBytes(enteredPassword);
            byte[] salt = LoadAdminPwSalt();

            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(userPwBytes, salt, 100000);
            byte[] userPwHash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (adminPwHash[i] != userPwHash[i])
                    return false;
            return true;
        }

        private static byte[] LoadAdminPwHash()
        {
            var str = "77:254:233:146:163:60:99:35:138:17:0:19:118:5:17:200:157:179:15:43";
            return str.Split(':').Select(sb => byte.Parse(sb)).ToArray();
        }
        private static byte[] LoadAdminPwSalt()
        {
            var str = "211:66:61:117:219:134:27:40:188:134:186:255:105:11:227:118";
            return str.Split(':').Select(sb => byte.Parse(sb)).ToArray();
        }
    }

    public static class LoginData
    {
        private static string _username = "";
        private static bool s_isAdmin;

        public static string Username
        {
            get => _username;
            set
            {
                _username = value;
                UsernameChanged?.Invoke(Username, new EventArgs());
            }
        }
        public static string Password { get; set; } = "";
        public static bool IsAdmin
        {
            get => s_isAdmin;
            set
            {
                s_isAdmin = value;
                AdminStatusChanged?.Invoke(null, new EventArgs());
            }
        }

        public static event EventHandler UsernameChanged;
        public static event EventHandler AdminStatusChanged;
    }
}