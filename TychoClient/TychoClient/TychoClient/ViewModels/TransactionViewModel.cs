using Plugin.NFC;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class TransactionViewModel : NfcAwareViewModel
    {
        private string _someData = "";
        public string SomeData
        {
            get => _someData;
            set => SetProperty(ref _someData, value + Environment.NewLine);
        }

        public ICommand ReadTagCommand { get; }
        public ICommand IncrementValueCommand { get; }
        public ICommand WriteToTagCommand { get; }

        public TransactionViewModel()
        {
            Title = "Charge money to another card";
            
            
        }

        protected override void OnFreeloaderCardScanned(RfidEventArgs e)
        {
            base.OnFreeloaderCardScanned(e);
        }
    }
}