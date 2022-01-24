using Plugin.NFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TychoClient.Models;
using Xamarin.Forms;

namespace TychoClient.Services
{
    public class NfcService : INfcService
    {
        private static NfcService _instance;
        private ITagInfo _lastReadKeyContent;

        protected INFC Nfc => CrossNFC.Current;

        public Task<FreeloaderCustomerData> ReadFromChip()
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteToChip(FreeloaderCustomerData data)
        {
            throw new NotImplementedException();
        }

        public static NfcService GetInstance()
        {
            if (_instance is null)
                _instance = new NfcService();
            return _instance;
        }

        public event EventHandler<NfcEventArgs> FreeloaderCardScanned;

        protected virtual void OnFreeloaderCardScanned(NfcEventArgs e)
        {
            FreeloaderCardScanned?.Invoke(this, e);
        }

        private NfcService()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Nfc.OnTagDiscovered += Current_OnTagDiscovered;
                Nfc.OnTagConnected += Current_OnTagConnected;
                Nfc.OnNfcStatusChanged += _nfc_OnNfcStatusChanged;
                Nfc.OnTagListeningStatusChanged += _nfc_OnTagListeningStatusChanged;
                Nfc.OnMessageReceived += _nfc_OnMessageReceived;
            });
        }
        

        private string _someData = "";


        public string SomeData
        {
            get => _someData;
            set => _someData += value + Environment.NewLine;
        }

        private void _nfc_OnMessageReceived(ITagInfo tagInfo)
        {
            SomeData += $"MESSAGE RECEIVED!";
            SomeData += $"Tag id: {String.Join(":", tagInfo.Identifier.Select(b => b.ToString("x")))}";
            SomeData += Newtonsoft.Json.JsonConvert.SerializeObject(tagInfo);
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
            Nfc.StopListening();
        }

        private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
        {
            SomeData += "TAG DISCOVERED!";

            if(tagInfo.Records.Count() == 1)
            {
                var message = tagInfo.Records[0].Payload;

                try
                {
                    var data = FreeloaderCustomerData.FromBytes(tagInfo.Identifier, message);
                    OnFreeloaderCardScanned(new NfcEventArgs() { Data = data, MetaData = tagInfo });
                }
                catch(Exception e)
                {

                }

            }
            else
            {
                OnFreeloaderCardScanned(new NfcEventArgs() { MetaData = tagInfo });
            }

            //_lastReadKeyContent = tagInfo;
            //LogSomeData();

            //tagInfo.Records = new[] { new NFCNdefRecord
            //            {
            //                TypeFormat = NFCNdefTypeFormat.WellKnown,
            //                MimeType = "application/com.companyname.nfcsample",
            //                Payload = _lastReadKeyContent.Records[0].Payload.Skip(4).ToArray(),// NFCUtils.EncodeToByteArray("Plugin.NFC is awesome!"),
            //                LanguageCode = "en"
            //            }};

            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    Nfc.PublishMessage(tagInfo, false);
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

            SomeData += $"Payload as string: {new string(Encoding.ASCII.GetChars(_lastReadKeyContent.Records[0].Payload))}";
        }
    }

    public class NfcEventArgs : EventArgs
    {
        public FreeloaderCustomerData Data { get; set; }
        public ITagInfo MetaData { get; set; }
    }
}
