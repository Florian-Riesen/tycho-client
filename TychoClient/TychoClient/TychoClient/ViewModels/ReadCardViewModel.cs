using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TychoClient.Models;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class ReadCardViewModel : NfcAwareViewModel
    {
        private string _chipUid;
        public string ChipUid
        {
            get => _chipUid;
            set => SetProperty(ref _chipUid, value);
        }
		
        private string _name;
        public string CustomerName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _transactionId;
        public string TransactionId
        {
            get => _transactionId;
            set => SetProperty(ref _transactionId, value);
        }

        private string _availableDrinks;
        public string AvailableDrinks
        {
            get => _availableDrinks;
            set => SetProperty(ref _availableDrinks, value);
        }

        private string _spentAlc;
        public string SpentAlcoholTokens
        {
            get => _spentAlc;
            set => SetProperty(ref _spentAlc, value);
        }

        private string _checksum;
        public string Checksum
        {
            get => _checksum;
            set => SetProperty(ref _checksum, value);
        }

        private ObservableCollection<Transaction> _transactions;
        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }
        
        //public int CurrentBalance
        //{
        //    get => CollapsedHistory + Transactions.Sum(t => t.Sum);
        //}

        private string _collapsedHistory;
        public string CollapsedHistory
        {
            get => _collapsedHistory;
            set => SetProperty(ref _collapsedHistory, value);
        }


        public ICommand ClearFormCommand { get; }
        public ICommand WriteToTagCommand { get; }

        public ReadCardViewModel()
        {
            Title = "Read Card";
            WriteToTagCommand = new Command(Write);
            ClearFormCommand = new Command(ClearForm);
        }

        private void ClearForm()
        {
            ChipUid = "";
            CustomerName = "";
            TransactionId = "";
            AvailableDrinks = "";
            SpentAlcoholTokens = "";
            Checksum = "";
            CollapsedHistory = "";

            Transactions?.Clear();
        }

        protected override void OnFreeloaderCardScanned(RfidEventArgs e)
        {
            Log.Line("ReadCardVm: Card scanned!");
            if (e.Data is null)
            {
                Log.Line("ReadCardVm: No data in scan!");
                ChipUid = string.Join(":", e.MetaData.Identifier);
                Log.Line($"ReadCardVm: Read UID {ChipUid} from metadata.");
                return;
            }

            ChipUid = string.Join(":", e.Data.ChipUid);
            CustomerName = e.Data.CustomerName;
            TransactionId = e.Data.ByteId.ToString();
            AvailableDrinks = e.Data.AvailableAlcoholTokens.ToString();
            SpentAlcoholTokens = e.Data.SpentAlcoholTokens.ToString();
            Checksum = string.Join(":", e.Data.Fletcher32Checksum);
            CollapsedHistory = e.Data.CollapsedTransactionHistory.ToString();
        }

        private void Write()
        {
            var data = new FreeloaderCustomerData()
            {
                ChipUid = ChipUid.Split(':').Select(sb => Byte.Parse(sb)).ToArray(),
                CustomerName = CustomerName,
            };

            data.CollapsedTransactionHistory = int.TryParse(CollapsedHistory, out int parsedCollapsedHistory) ? parsedCollapsedHistory : 0;
            data.ByteId = byte.TryParse(TransactionId, out byte result) ? result : (byte)0;
            if (Transactions != null)
                data.Transactions = Transactions.ToList();

            data.AvailableAlcoholTokens = byte.TryParse(AvailableDrinks, out byte resDrinks) ? resDrinks : (byte)0;
            data.SpentAlcoholTokens = byte.TryParse(SpentAlcoholTokens, out byte resSpent) ? resSpent : (byte)0;

            data.Fletcher32Checksum = data.CalculateFletcher32("ThereBeDragons");
            DataToWrite = data;
            Log.Line("ReadCardVm: Prepared Data for write!");
        }
    }
}