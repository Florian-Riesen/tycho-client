using System.Linq;
using System.Security.Cryptography;
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
            LoginData.Username = UserName;
            LoginData.Password = Password;
            LoginData.IsAdmin = HashStuff.IsAdminPassword(Password);
        }
    }

    class HashStuff
    {
        public static bool IsAdminPassword(string enteredPassword)
        {
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
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static bool IsAdmin { get; set; }
    }
}