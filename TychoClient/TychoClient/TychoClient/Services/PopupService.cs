using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TychoClient.ViewModels;
using Xamarin.Forms;

namespace TychoClient.Services
{
    public static class PopupService
    {
        public static IPopupViewModel RandomlyShowAdvertisement()
        {
            if (new Random().Next(0, 100) > 20)
                return GetInvisiblePopup();

            var newPopup = new PlainImagePopupViewModel();
            newPopup.IsVisible = true;
            newPopup.CanClose = true;
            newPopup.CloseCommand = new Command(() => newPopup.IsVisible = false);
            newPopup.Image = GetRandomAd();

            return newPopup;
        }

        public static IPopupViewModel GetInvisiblePopup() => new PlainImagePopupViewModel() { IsVisible = false };


        private static ImageSource GetRandomAd()
        {
            return ImageSource.FromResource("TychoClient.Resources.nauvoo_ad.png");
        }
    }


    public interface IPopupViewModel
    {
        bool IsVisible { get; set; }
        bool CanClose { get; set; }
        ICommand CloseCommand { get; set; }
        ImageSource Image { get; set; }
    }

    public class PlainImagePopupViewModel : BaseViewModel, IPopupViewModel
    {
        private bool _canClose;
        public bool CanClose
        {
            get => _canClose;
            set => SetProperty(ref _canClose, value);
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }


        public ICommand CloseCommand { get; set; }

        public ImageSource Image { get; set; }
    }
}
