using System.Windows.Input;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class BarScannerViewModel : NfcAwareViewModel
    {
        private int _tokensToBeCharged;
        public int TokensToBeCharged
        {
            get => _tokensToBeCharged;
            set
            {
                SetProperty(ref _tokensToBeCharged, value);
                CustomerName = "";
                PaymentRejected = false;
                PaymentSucceeded = false;
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

        private bool _succeeded;
        public bool PaymentSucceeded
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

        protected override void OnFreeloaderCardScanned(RfidEventArgs e)
        {
            if (!LoginData.IsAdmin)
            {
                TokensToBeCharged = 0;
                CustomerName = "ERROR: NO ADMIN PERMISSIONS ON SCANNER. Login as admin!";
                PaymentRejected = true;
                PaymentSucceeded = false;
                return;
            }
            if (e.Data is null)
            {
                TokensToBeCharged = 0;
                CustomerName = "ERROR: CARD EMPTY";
                PaymentRejected = true;
                PaymentSucceeded = false;
                return;
            }

            var returnCode = e.Data.PurchaseAlcohol(TokensToBeCharged, LoginData.Password);

            if (returnCode != 0)
            {
                TokensToBeCharged = 0;
                CustomerName = e.Data.CustomerName;
                PaymentRejected = true;
                PaymentSucceeded = false;
                return;
            }

            e.Data.AvailableAlcoholTokens = (byte)(e.Data.AvailableAlcoholTokens - TokensToBeCharged);
            e.Data.SpentAlcoholTokens = (byte)(e.Data.SpentAlcoholTokens + TokensToBeCharged);
            TokensToBeCharged = 0;
            CustomerName = e.Data.CustomerName;
            DataToWrite = e.Data;
            PaymentSucceeded = true;
            PaymentRejected = false;
        }
    }
}