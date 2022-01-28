using Plugin.NFC;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TychoClient.Models;

using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class ReadCardViewModel : BaseViewModel
    {

        private FreeloaderCustomerData _customerData;

        private byte[] _chipUid;
        public byte[] ChipUid
        {
            get => _chipUid;
            private set => SetProperty(ref _chipUid, value);
        }

        private string _name;
        public string CustomerName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private byte? _transactionId;
        public byte? TransactionId
        {
            get => _transactionId;
            set => SetProperty(ref _transactionId, value);
        }

        private byte? _availableDrinks;
        public byte? AvailableDrinks
        {
            get => _availableDrinks;
            set => SetProperty(ref _availableDrinks, value);
        }

        private byte? _spentAlc;
        public byte? SpentAlcoholTokens
        {
            get => _spentAlc;
            set => SetProperty(ref _spentAlc, value);
        }

        private byte[] _checksum;
        public byte[] Checksum
        {
            get => _checksum;
            set => SetProperty(ref _checksum, value);
        }

        private ObservableCollection<Transaction> _transactions;
        public ObservableCollection<Transaction> Transactions
        {
            get => _someData;
            set => SetProperty(ref _transactions, value);
        }

        public int CurrentBalance
        {
            get => CollapsedHistory + Transactions.Sum(t => t.Sum);
        }

        private int _collapsedHistory;
        public int CollapsedHistory
        {
            get => _collapsedHistory;
            set => SetProperty(ref _collapsedHistory, value);
        }


        public ICommand ClearFormCommand { get; }
        public ICommand WriteToTagCommand { get; }

        public ReadCardViewModel()
        {
            Title = "Read Card";
            WriteToTagCommand = new Command(); // declare intent to write
            ClearFormCommand = new Command(ClearForm);
        }

        private void ClearForm()
        {
            ChipUid = null;
            CustomerName = "";
            TransactionId = null;
            AvailableDrinks = null;
            SpentAlcoholTokens = null;
            Checksum = null;

            Transactions.Clear();
        }

        protected override void OnRead(NfcEventArgs e)
        {
            //	base

            if (e.Data is null)
                return;
            ChipUid = e.Data.ChipUid;
            CustomerName = e.Data.CustomerName;
            TransactionId = e.Data.ByteId;
            AvailableDrinks = e.Data.AvailableAlcoholTokens;
            SpentAlcoholTokens = e.Data.SpentAlcoholTokens;
            Checksum = e.Data.Fletcher32Checksum;
        }

        private void Write()
        {
            var data = new FreeloaderCustomerData();
            // populate
            AddToWritingQueue(data);
        }

        // OnPropertyChanged update Checksum
    }
}