using Plugin.NFC;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class NewsViewModel : BaseViewModel
    {
        private IPopupViewModel _currentPopup;
        public IPopupViewModel CurrentPopup
        {
            get => _currentPopup;
            set => SetProperty(ref _currentPopup, value);
        }

        public NewsViewModel()
        {
            Title = "Tycho Station News";
            CurrentPopup = PopupService.GetInvisiblePopup();
        }
    }
}