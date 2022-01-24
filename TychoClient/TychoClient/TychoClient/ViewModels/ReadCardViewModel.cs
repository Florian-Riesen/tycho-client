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
            set => SetProperty(ref _chipUid, value);
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


        public ICommand ReadTagCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand WriteToTagCommand { get; }

        public ReadCardViewModel()
        {
            Title = "Read Card";

            

            ReadTagCommand = new Command(); // start listening
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

        

        
        
    }
}