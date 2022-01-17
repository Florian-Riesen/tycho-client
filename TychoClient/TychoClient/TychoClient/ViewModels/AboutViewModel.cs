using Plugin.NFC;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;

using Xamarin.Forms;

namespace TychoClient.ViewModels
{
    public class AboutViewModel : BaseViewModel
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

        public AboutViewModel()
        {
            Title = "Scanner";

            Device.BeginInvokeOnMainThread(() =>
            {
                _nfc.OnTagDiscovered += Current_OnTagDiscovered;
                _nfc.OnTagConnected += Current_OnTagConnected;
                _nfc.OnNfcStatusChanged += _nfc_OnNfcStatusChanged;
                _nfc.OnTagListeningStatusChanged += _nfc_OnTagListeningStatusChanged;
                _nfc.OnMessageReceived += _nfc_OnMessageReceived;
            });
            

            ReadTagCommand = new Command(ReadTag);
            IncrementValueCommand = new Command(LogSomeData);
            WriteToTagCommand = new Command(WriteToTag);
        }

        private void _nfc_OnMessageReceived(ITagInfo tagInfo)
        {
            SomeData += $"MESSAGE RECEIVED!";
            SomeData += $"Tag id: {String.Join(":", tagInfo.Identifier.Select(b => b.ToString("x")))}";
            SomeData += Newtonsoft.Json.JsonConvert.SerializeObject(tagInfo);
            _lastReadKeyContent = tagInfo;
            SomeData += $"TagInfo type: {tagInfo.GetType().Name}";

            WriteToTag();

            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    _nfc.OnMessageReceived -= _nfc_OnMessageReceived;
            //});
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


        private void ReadTag()
        {
            _nfc.StartListening();
            _nfc.StartPublishing();
            SomeData += "Started listening...! ";
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    _nfc.OnMessageReceived += _nfc_OnMessageReceived;
            //});
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

            //var newPayload = _lastReadKeyContent.Records[0].Payload.Skip(4).ToArray();

            //newPayload[0] = newPayload[0]++;

            //_lastReadKeyContent = new TagInfo(_lastReadKeyContent.Identifier)
            //{
            //    Records = new[] { new NFCNdefRecord()
            //    {
            //        TypeFormat = _lastReadKeyContent.Records[0].TypeFormat,
            //        Payload = newPayload,
            //        MimeType = _lastReadKeyContent.Records[0].MimeType,
            //        ExternalType = _lastReadKeyContent.Records[0].ExternalType,
            //        LanguageCode = _lastReadKeyContent.Records[0].LanguageCode,
            //    }},

            //};

            SomeData += $"Payload as string: {new string(Encoding.ASCII.GetChars(_lastReadKeyContent.Records[0].Payload))}";
            //_lastReadKeyContent.Records[0].Payload
            // actually alter a byte here
        }

        private void WriteToTag()
        {

            _nfc.StartPublishing();
            
        }
    }
}