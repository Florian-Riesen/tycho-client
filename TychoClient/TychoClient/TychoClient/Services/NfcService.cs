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
    public class NfcService
    {
        private static NfcService _instance;
        private ITagInfo _lastReadKeyContent;

        protected INFC Nfc => CrossNFC.Current;

        public FreeloaderCustomerData DataToWrite { get; set; }

        public static NfcService GetInstance()
        {
            if (_instance is null)
                _instance = new NfcService();
            return _instance;
        }

        public event EventHandler<RfidEventArgs> FreeloaderCardScanned;
        public event EventHandler<RfidEventArgs> FreeloaderCardWritten;

        private NfcService()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Nfc.OnTagDiscovered += Current_OnTagDiscovered;
                //Nfc.OnTagConnected += Current_OnTagConnected;
                Nfc.OnNfcStatusChanged += _nfc_OnNfcStatusChanged;
                Nfc.OnTagListeningStatusChanged += _nfc_OnTagListeningStatusChanged;
                Nfc.OnMessageReceived += _nfc_OnMessageReceived;
            });

            Nfc.StartListening();
            Nfc.StartPublishing();
        }

        private void _nfc_OnMessageReceived(ITagInfo tagInfo)
        {
            Log.Line($"NFCService: MESSAGE RECEIVED!");
            Log.Line($"NFCService: Tag id: {String.Join(":", tagInfo.Identifier.Select(b => b.ToString("x")))}");
            Log.Line("NFCService: " + Newtonsoft.Json.JsonConvert.SerializeObject(tagInfo));
            Log.Line($"NFCService: TagInfo type: {tagInfo.GetType().Name}");
        }

        private void _nfc_OnTagListeningStatusChanged(bool isListening)
        {
            Log.Line($"NFCService: LISTENING STATUS CHANGED: {(isListening ? "started" : "stopped")} listening");
            if (!isListening)
                Nfc.StartListening();
        }

        private void _nfc_OnNfcStatusChanged(bool isEnabled)
        {
            Log.Line($"NFCService: NFC status changed: {(isEnabled ? "NOT" : "")} enabled.");
        }

        //private void Current_OnTagConnected(object sender, EventArgs e)
        //{
        //    Log.Line("TAG CONNECTED!");
        //    Nfc.StopListening();
        //}

        private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
        {
            Log.Line("NFCService: TAG DISCOVERED!");

            if(tagInfo.Records.Count() == 1)
            {
                var message = tagInfo.Records[0].Payload;

                try
                {
                    var data = FreeloaderCustomerData.FromBytes(tagInfo.Identifier, message);
                    Log.Line("NFCService: Freeloader block deserialized!");
                    FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { Data = data, MetaData = tagInfo });

                    if(Equals(DataToWrite.ChipUid, tagInfo.Identifier))
                    {
                        Log.Line("NFCService: There is some data to write.");
                        tagInfo.Records[0].Payload = DataToWrite.ToBytes();

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Nfc.PublishMessage(tagInfo, false);
                        });
                        Log.Line("NFCService: Written successfully!");
                        FreeloaderCardWritten?.Invoke(this, new RfidEventArgs() { Data = DataToWrite, MetaData = tagInfo });

                    }
                }
                catch(Exception e)
                {
                    Log.Line("NFCService: Error while NFC i/o!" + e.ToString());
                }

            }
            else
                Log.Line($"NFCService: Error: Chip contains {tagInfo.Records.Length} records instead of exactly 1.");

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
            Log.Line($"Message: {message}");
            Log.Line($"As Bytes: {String.Join(":", asBytes.Select(b => b.ToString("X2")))}");
            asBytes[0]++;
            Log.Line($"New Bytes: {String.Join(":", asBytes.Select(b => b.ToString("X2")))}");
            var newMessage = new String(Encoding.ASCII.GetChars(asBytes));
            Log.Line($"New Message: {newMessage}");

            Log.Line($"Payload as string: {new string(Encoding.ASCII.GetChars(_lastReadKeyContent.Records[0].Payload))}");
        }
    }

    public class RfidEventArgs : EventArgs
    {
        public FreeloaderCustomerData Data { get; set; }
        public ITagInfo MetaData { get; set; }
    }
}
