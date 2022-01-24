using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using TychoClient.Models;
using TychoClient.Services;
using Plugin.NFC;

namespace TychoClient.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>() ?? new MockDataStore();

        public NfcService Nfc => NfcService.GetInstance();

        public SelectionWatcher Watcher
        {
            get => _watcher;
            set
            {
                if(_watcher != null)
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
            }
        }

        private void _watcher_GotDeselected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _watcher_GotSelected(object sender, EventArgs e)
        {
            //Nfc.
        }

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        private SelectionWatcher _watcher;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public BaseViewModel()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Nfc.FreeloaderCardScanned += Nfc_FreeloaderCardScanned;

                //Nfc.OnTagDiscovered += Current_OnTagDiscovered;
                //Nfc.OnTagConnected += Current_OnTagConnected;
                //Nfc.OnNfcStatusChanged += _nfc_OnNfcStatusChanged;
                //Nfc.OnTagListeningStatusChanged += _nfc_OnTagListeningStatusChanged;
                //Nfc.OnMessageReceived += _nfc_OnMessageReceived;
            });
        }

        private void Nfc_FreeloaderCardScanned(object sender, NfcEventArgs e)
        {
            if (Watcher.IsSelected)
                OnFreeloaderCardScanned(e);
        }

        protected virtual void OnFreeloaderCardScanned(NfcEventArgs e)
        {

        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
