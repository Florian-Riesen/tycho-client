using Plugin.NFC;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;

using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class NewsViewModel : BaseViewModel
    {
        private string _someData = "";
        private INFC _nfc => CrossNFC.Current;
        private ITagInfo _lastReadKeyContent;


        public string SomeData
        {
            get => _someData;
            set => SetProperty(ref _someData, value + Environment.NewLine);
        }

        public ICommand ReadTagCommand { get; }
        public ICommand IncrementValueCommand { get; }
        public ICommand WriteToTagCommand { get; }

        public NewsViewModel()
        {
            Title = "Tycho Station News";

            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    _nfc.OnTagDiscovered += Current_OnTagDiscovered;
            //    _nfc.OnTagConnected += Current_OnTagConnected;
            //    _nfc.OnNfcStatusChanged += _nfc_OnNfcStatusChanged;
            //    _nfc.OnTagListeningStatusChanged += _nfc_OnTagListeningStatusChanged;
            //    _nfc.OnMessageReceived += _nfc_OnMessageReceived;
            //});
            

            ReadTagCommand = new Command(ReadAndWriteTag);
            IncrementValueCommand = new Command(LogSomeData);
        }

        private void _nfc_OnMessageReceived(ITagInfo tagInfo)
        {
            SomeData += $"MESSAGE RECEIVED!";
            SomeData += $"Tag id: {String.Join(":", tagInfo.Identifier.Select(b => b.ToString("x")))}";
            SomeData += Newtonsoft.Json.JsonConvert.SerializeObject(tagInfo);
            _lastReadKeyContent = tagInfo;
            SomeData += $"TagInfo type: {tagInfo.GetType().Name}";
            
        }

        private void _nfc_OnTagListeningStatusChanged(bool isListening)
        {
            SomeData += $"LISTENING STATUS CHANGED: {(isListening ? "started" : "stopped")} listening";
        }


        private void _nfc_OnNfcStatusChanged(bool isEnabled)
        {
            SomeData += "NFC status changed!";
        }

        private void Current_OnTagConnected(object sender, EventArgs e)
        {
            SomeData += "TAG CONNECTED!";
            _nfc.StopListening();
        }

        private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
        {
            SomeData += "TAG DISCOVERED!";

            _lastReadKeyContent = tagInfo;
            LogSomeData();

            tagInfo.Records = new[] { new NFCNdefRecord
                        {
                            TypeFormat = NFCNdefTypeFormat.WellKnown,
                            MimeType = "application/com.companyname.nfcsample",
                            Payload = _lastReadKeyContent.Records[0].Payload.Skip(4).ToArray(),// NFCUtils.EncodeToByteArray("Plugin.NFC is awesome!"),
                            LanguageCode = "en"
                        }};

            Device.BeginInvokeOnMainThread(() =>
            {
                _nfc.PublishMessage(tagInfo, false);
            });
        }


        private void ReadAndWriteTag()
        {
            _nfc.StartListening(); // makes android use this App preferably the next time a tag is presented
            _nfc.StartPublishing(); // prepares for writing
            SomeData += "Waiting for tag...";
        }

        private void LogSomeData()
        {
            if (_lastReadKeyContent is null || _lastReadKeyContent.Records.Length == 0
                || _lastReadKeyContent.Records[0].Payload.Length < 8)
                return;

            var message = _lastReadKeyContent.Records[0].Message;
            var asBytes = Encoding.ASCII.GetBytes(_lastReadKeyContent.Records[0].Message);

            byte firstByte = asBytes[0];
            SomeData += $"Message: {message}";
            SomeData += $"As Bytes: {String.Join(":", asBytes.Select(b => b.ToString("X2")))}";
            asBytes[0]++;
            SomeData += $"New Bytes: {String.Join(":", asBytes.Select(b => b.ToString("X2")))}";
            var newMessage = new String(Encoding.ASCII.GetChars(asBytes));
            SomeData += $"New Message: {newMessage}";

            SomeData += $"Payload as string: {new string(Encoding.ASCII.GetChars(_lastReadKeyContent.Records[0].Payload))}";
        }
        
    }
}