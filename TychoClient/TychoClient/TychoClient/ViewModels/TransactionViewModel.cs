using System.Linq;
using System.Windows.Input;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class TransactionViewModel : NfcAwareViewModel
    {
        private byte[] losingCreditsUid;
        private byte[] receivingCreditsUid;
        private byte losingCreditsTransactionId;
        private byte receivingCreditsTransactionId;

        private TransactionState _state;
        public TransactionState State
        {
            get => _state;
            set { SetProperty(ref _state, value);
                switch (value)
                {
                    case TransactionState.WaitingForInput:
                        Prompt = "Enter the transaction amount. You will then need to present: \r\n(1) The charged chip; \r\n(2) The receiving chip; \r\n(3) The charged chip.";
                        break;
                    case TransactionState.WaitingForChargee:
                        Prompt = "Present the chip that will be CHARGED.";
                        break;
                    case TransactionState.WaitingForCharger:
                        Prompt = "Present the chip that will RECEIVE the money.";
                        break;
                    case TransactionState.WaitingForFinalization:
                        Prompt = "Present the chip that will be CHARGED once more.";
                        break;
                    case TransactionState.Finished:
                        Prompt = "The transaction was successful!";
                        break;
                }
            }
        }


        private string _prompt = "";
        public string Prompt
        {
            get => _prompt;
            set => SetProperty(ref _prompt, value);
        }

        private short _amount;
        public short Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }


        public ICommand ButtonCommand { get; }

        public TransactionViewModel()
        {
            Title = "Charge money to another card";
            ButtonCommand = new Command(ButtonPressed);
            State = TransactionState.WaitingForInput;
        }

        private void ButtonPressed()
        {
            if (State == TransactionState.WaitingForInput)
                State = TransactionState.WaitingForChargee;
            else if (State == TransactionState.Finished)
                State = TransactionState.WaitingForInput;
            else
            {
                State = TransactionState.WaitingForInput;
                Amount = 0;
            }
                
        }

        protected override void OnFreeloaderCardScanned(RfidEventArgs e)
        {
            base.OnFreeloaderCardScanned(e);

            if(_state == TransactionState.Finished || _state == TransactionState.WaitingForInput || _state == TransactionState.Error || e.Data is null)
            {
                return;
            }

            if(_state == TransactionState.WaitingForChargee)
            {
                losingCreditsUid = e.Data.ChipUid;
                losingCreditsTransactionId = e.Data.ByteId;
                e.Data.ChargeFromCard(Amount);
                State = TransactionState.WaitingForCharger;
                DataToWrite = e.Data;
                return;
            }

            if(_state == TransactionState.WaitingForCharger && !Enumerable.SequenceEqual(e.Data.ChipUid, losingCreditsUid))
            {
                receivingCreditsUid = e.Data.ChipUid;
                receivingCreditsTransactionId = e.Data.ByteId;
                e.Data.AddToCard(Amount, losingCreditsTransactionId);
                State = TransactionState.WaitingForFinalization;
                DataToWrite = e.Data;
                return;
            }


            if (_state == TransactionState.WaitingForFinalization && Enumerable.SequenceEqual(e.Data.ChipUid, losingCreditsUid))
            {
                e.Data.FinalizeTransaction(receivingCreditsTransactionId);
                State = TransactionState.Finished;
                Amount = 0;
                DataToWrite = e.Data;
                return;
            }

        }
    }

    public enum TransactionState
    {
        WaitingForInput,
        WaitingForChargee,
        WaitingForCharger,
        WaitingForFinalization,
        Finished,
        Error
    }
}