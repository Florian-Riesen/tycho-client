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
        #region Properties
        private TransactionUiModel _selectedTransaction;
        public TransactionUiModel SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        private bool _readonly;
        public bool Readonly
        {
            get => _readonly;
            set => SetProperty(ref _readonly, value);
        }

        public bool IsWriting => DataToWrite != null;

        private bool? _checksumMatches;
        public bool? ChecksumMatches
        {
            get => _checksumMatches;
            set => SetProperty(ref _checksumMatches, value);
        }


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

        public ObservableCollection<TransactionUiModel> Transactions
        {
            get;
            set;
        } = new ObservableCollection<TransactionUiModel>();

        public string CurrentBalance
        {
            get => int.TryParse(CollapsedHistory, out int parsedCollapsed) ? (parsedCollapsed + Transactions.Sum(t => t.Sum)).ToString() : "";
        }

        private string _collapsedHistory;
        public string CollapsedHistory
        {
            get => _collapsedHistory;
            set => SetProperty(ref _collapsedHistory, value);
        }
        #endregion Properties

        #region Commands
        public ICommand ClearFormCommand { get; }
        public ICommand WriteToTagCommand { get; }
        public ICommand AddTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        #endregion Commands

        public ReadCardViewModel()
        {
            Title = "Read Card";
            WriteToTagCommand = new Command(Write);
            ClearFormCommand = new Command(ClearForm);
            AddTransactionCommand = new Command(() => Transactions.Add(new TransactionUiModel() { NameDb = DataStore.GetInstance().GetUserList() }));
            DeleteTransactionCommand = new Command(DeleteTransaction);
            UpdateReadonly();
            Transactions.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CurrentBalance));

        }

        protected override void OnFreeloaderCardScanned(RfidEventArgs e)
        {
            UpdateReadonly();
            OnPropertyChanged(nameof(IsWriting));
            this.Log("ReadCardVm: Card scanned!");
            if (e.Data is null)
            {
                this.Log("No data in scan!");
                ClearForm();
                ChipUid = string.Join(":", e.MetaData.Identifier);
                return;
            }

            ChipUid = string.Join(":", e.Data.ChipUid);
            CustomerName = e.Data.CustomerName;
            TransactionId = e.Data.ByteId.ToString();
            AvailableDrinks = e.Data.AvailableAlcoholTokens.ToString();
            SpentAlcoholTokens = e.Data.SpentAlcoholTokens.ToString();
            Checksum = string.Join(":", e.Data.Fletcher32Checksum);
            ChecksumMatches = LoginData.IsAdmin ? (bool?)Enumerable.SequenceEqual(e.Data.Fletcher32Checksum, e.Data.CalculateFletcher32(LoginData.Password)) : null;
            CollapsedHistory = e.Data.CollapsedTransactionHistory.ToString();
            Transactions.Clear();
            var userList = DataStore.GetInstance().GetUserList();
            foreach (var t in e.Data.Transactions)
                Transactions.Add(new TransactionUiModel() { Sum = t.Sum, PartnerByteId = t.PartnerByteId, NameDb = userList});
        }

        protected override void OnUserNavigatedHere()
        {
            DataStore.GetInstance().ForceRefreshCache();
            base.OnUserNavigatedHere();
            ClearForm();
            UpdateReadonly();
        }

        private void UpdateReadonly() => Readonly = !LoginData.IsAdmin;

        #region Command Methods
        private void Write()
        {
            if (IsWriting)
            {
                DataToWrite = null;
                OnPropertyChanged(nameof(IsWriting));
                Readonly = false;
                return;
            }

            Readonly = true;

            var data = new FreeloaderCustomerData()
            {
                ChipUid = ChipUid.Split(':').Select(sb => byte.Parse(sb)).ToArray(),
                CustomerName = CustomerName,
            };

            data.CollapsedTransactionHistory = int.TryParse(CollapsedHistory, out int parsedCollapsedHistory) ? parsedCollapsedHistory : 0;
            data.ByteId = byte.TryParse(TransactionId, out byte result) ? result : (byte)0;
            if (Transactions != null)
                data.Transactions = Transactions.Select(t => new Transaction() { PartnerByteId = t.PartnerByteId, Sum = t.Sum }).ToList();

            data.AvailableAlcoholTokens = byte.TryParse(AvailableDrinks, out byte resDrinks) ? resDrinks : (byte)0;
            data.SpentAlcoholTokens = byte.TryParse(SpentAlcoholTokens, out byte resSpent) ? resSpent : (byte)0;

            data.Fletcher32Checksum = data.CalculateFletcher32(LoginData.Password);
            DataToWrite = data;
            OnPropertyChanged(nameof(IsWriting));
            this.Log("ReadCardVm: Prepared Data for write!");
        }

        private void DeleteTransaction()
        {
            Transactions.Remove(SelectedTransaction);
            SelectedTransaction = null;
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
            ChecksumMatches = null;
            DataToWrite = null;
            OnPropertyChanged(nameof(IsWriting));

            Transactions.Clear();
        }

        #endregion Command Methods
    }

    public class TransactionUiModel : Transaction
    {
        public UserList NameDb { get; set; }
        public string Partner
        {
            get => NameDb.FirstOrDefault(e => e.sid == PartnerByteId)?.name ?? PartnerByteId.ToString();
            set 
            {
                if (byte.TryParse(value, out byte parsedSid))
                    PartnerByteId = parsedSid;
                else
                    PartnerByteId = NameDb.FirstOrDefault(e => e.sid == PartnerByteId)?.sid ?? 0;
            }
        }
    }

    
}