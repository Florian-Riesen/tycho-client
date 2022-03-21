using System;
using System.Linq;
using TychoClient.Models;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class NfcAwareViewModel : BaseViewModel
    {
        private SelectionWatcher _watcher;
        protected FreeloaderCustomerData DataToWrite
        {
            get => Nfc.DataToWrite;
            set => Nfc.DataToWrite = value;
        }

        public NfcService Nfc => NfcService.GetInstance();

        public SelectionWatcher Watcher
        {
            get => _watcher;
            set
            {
                bool wasNull = _watcher is null;
                if (_watcher != null)
                {
                    _watcher.GotSelected -= _watcher_GotSelected;
                    _watcher.GotDeselected -= _watcher_GotDeselected;
                }
                _watcher = value;
                {
                    _watcher.GotSelected -= _watcher_GotSelected;
                    _watcher.GotSelected += _watcher_GotSelected;
                    _watcher.GotDeselected -= _watcher_GotDeselected;
                    _watcher.GotDeselected += _watcher_GotDeselected;
                }
                if (wasNull)
                    _watcher_GotSelected(this, new EventArgs());
            }
        }

        private IPopupViewModel _currentPopup;
        public IPopupViewModel CurrentPopup
        {
            get => _currentPopup;
            set => SetProperty(ref _currentPopup, value);
        }


        public NfcAwareViewModel()
        {
            Nfc.FreeloaderCardScanned += Nfc_FreeloaderCardScanned;
            Nfc.FreeloaderCardWritten += Nfc_FreeloaderCardWritten;
            //CurrentPopup = PopupService.GetInitialPopup();
        }

        private void Nfc_FreeloaderCardWritten(object sender, RfidEventArgs e)
        {
            if (Watcher.IsSelected)
                OnFreeloaderCardWritten(e);
        }

        protected virtual void OnFreeloaderCardWritten(RfidEventArgs e)
        {
        }

        private void _watcher_GotDeselected(object sender, EventArgs e)
        {

        }

        private void _watcher_GotSelected(object sender, EventArgs e)
        {
            OnUserNavigatedHere();
            CurrentPopup = PopupService.RandomlyShowAdvertisement();
        }

        protected virtual void OnUserNavigatedHere()
        { }

        private void Nfc_FreeloaderCardScanned(object sender, RfidEventArgs e)
        {
            if (Watcher?.IsSelected ?? false)
                OnFreeloaderCardScanned(e);
        }

        protected virtual void OnFreeloaderCardScanned(RfidEventArgs e)
        {
        }
    }
}
