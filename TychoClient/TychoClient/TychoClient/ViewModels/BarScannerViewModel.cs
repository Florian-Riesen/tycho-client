using Plugin.NFC;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class BarScannerViewModel : BaseViewModel
    {
        private int _tokensToBeCharged;
        public int TokensToBeCharged
        {
            get => _tokensToBeCharged;
            set
            {
                SetProperty(ref _tokensToBeCharged, value);
                if (value == 0)
                {
                    // remove from write queue
                }
                else
                {
                    // update entry in write queue
                }
            }
        }

        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            private set => SetProperty(ref _customerName, value);
        }

        private bool _rejected;
        public bool PaymentRejected
        {
            get => _rejected;
            private set => SetProperty(ref _rejected, value);
        }

        private string _succeeded;
        public string PaymentSucceeded
        {
            get => _succeeded;
            private set => SetProperty(ref _succeeded, value);
        }

        public ICommand IncrementByOneCommand { get; }
        public ICommand IncrementByFourCommand { get; }
        public ICommand OkCommand { get; }

        public BarScannerViewModel()
        {
            Title = "Bar Scanner";

            IncrementByOneCommand = new Command(() => TokensToBeCharged++);
            IncrementByFourCommand = new Command(() => TokensToBeCharged += 4);
        }

        protected override void OnFreeloaderCardScanned(NfcEventArgs e)
        {
            // base.OnRead(e);

            // call of this method means we attempted to charge tokens

            CustomerName = e.Data.CustomerName;

        }
    }
}