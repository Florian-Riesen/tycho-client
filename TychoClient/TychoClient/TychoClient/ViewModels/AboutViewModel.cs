using Plugin.NFC;
using System;
using System.Linq;
using System.Windows.Input;

using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private string _someData = "";
        private INFC _nfc => CrossNFC.Current;

        public AboutViewModel()
        {
            Title = "Scanner";

            _nfc.OnTagDiscovered += Current_OnTagDiscovered;
            _nfc.OnTagConnected += Current_OnTagConnected;
            _nfc.OnNfcStatusChanged += _nfc_OnNfcStatusChanged;
            _nfc.OnTagListeningStatusChanged += _nfc_OnTagListeningStatusChanged;
            _nfc.OnMessageReceived += _nfc_OnMessageReceived;


            ReadTagCommand = new Command(ReadTag);
        }

        private void _nfc_OnMessageReceived(ITagInfo tagInfo)
        {
            SomeData += $"MESSAGE RECEIVED!";
            SomeData += $"Tag id: {String.Join(":", tagInfo.Identifier.Select(b => b.ToString("x")))}";
            SomeData += Newtonsoft.Json.JsonConvert.SerializeObject(tagInfo);
        }

        private void _nfc_OnTagListeningStatusChanged(bool isListening)
        {
            SomeData += $"LISTENING STATUS CHANGED: {(isListening ? "started" : "stopped")} listening";
        }


        private void _nfc_OnNfcStatusChanged(bool isEnabled)
        {
            SomeData += "NFC STATUS CHANGED!";
            _nfc.StopListening();
        }

        private void Current_OnTagConnected(object sender, EventArgs e)
        {
            SomeData += "TAG CONNECTED!";
            _nfc.StopListening();
        }

        private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
        {
            SomeData += "TAG DISCOVERED!";
            _nfc.StopListening();
        }

        public string SomeData
        {
            get => _someData;
            set => SetProperty(ref _someData, value + Environment.NewLine);
        }
        public ICommand ReadTagCommand { get; }

        private void ReadTag()
        {
            _nfc.StartListening();
            SomeData += "Started listening...! ";
        }
    }
}