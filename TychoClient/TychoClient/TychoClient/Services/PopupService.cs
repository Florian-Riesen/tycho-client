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
        private static Random _random = new Random();

        public static IPopupViewModel RandomlyShowAdvertisement()
        {
            if (_random.Next(0, 100) > 50)
                return GetInvisiblePopup();

            var newPopup = new PlainImagePopupViewModel();
            newPopup.IsVisible = true;
            newPopup.CanClose = true;
            newPopup.CloseCommand = new Command(() => newPopup.IsVisible = false);
            newPopup.Image = GetRandomAd();

            return newPopup;
        }

        public static IPopupViewModel GetInvisiblePopup() => new PlainImagePopupViewModel() { IsVisible = false };


        private static string GetRandomAd()
        {
            var possibleAdResources = new List<string>();
            possibleAdResources.Add("nauvoo_ad.png");
            possibleAdResources.Add("protector.gif");
            possibleAdResources.Add("hand_model_s.gif");
            possibleAdResources.Add("human_plus_s.gif");
            possibleAdResources.Add("the_cube_s.gif");
            possibleAdResources.Add("vj.gif");

            return possibleAdResources[_random.Next(0, possibleAdResources.Count)];
        }
    }


    public interface IPopupViewModel
    {
        bool IsVisible { get; set; }
        bool CanClose { get; set; }
        ICommand CloseCommand { get; set; }
        string Image { get; set; }
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

        public string Image { get; set; }
    }
}
