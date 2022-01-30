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
        private List<Byte> _lastPayload;

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

        private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
        {
            Log.Line("NFCService: TAG DISCOVERED!");

            if(tagInfo.Records != null && tagInfo.Records.Length == 1)
            {
                var message = tagInfo.Records[0].Payload;

                if (_lastPayload != null)
                    Log.Line("NfcService: Previously Written Bytes: \r\n" + string.Join(":", _lastPayload));
                Log.Line("NfcService: Read Bytes: \r\n" + string.Join(":", message));


                try
                {
                    var data = FreeloaderCustomerData.FromBytes(tagInfo.Identifier, message);
                    Log.Line("NFCService: Freeloader block deserialized!");
                    FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { Data = data, MetaData = tagInfo });
                }
                catch(Exception e)
                {
                    Log.Line("NFCService: Error while reading NFC!" + e.ToString());
                    FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { MetaData = tagInfo });
                }

            }
            else
            {
                Log.Line($"NFCService: Error: Chip contains {tagInfo.Records?.Length} records instead of exactly 1.");
                FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { MetaData = tagInfo });
            }

            if (DataToWrite != null)
            {
                if (Enumerable.SequenceEqual(DataToWrite.ChipUid, tagInfo.Identifier))
                {
                    Log.Line("NFCService: There is some data to write.");

                    //tagInfo.Records[0].TypeFormat = NFCNdefTypeFormat.Unknown;
                    //tagInfo.Records[0].LanguageCode = "";
                    //tagInfo.Records[0].Payload = DataToWrite.ToBytes();
                    //tagInfo.Records[0].TypeFormat = NFCNdefTypeFormat.Unknown;
                    //tagInfo.Records[0].LanguageCode = "";

                    var bytes = DataToWrite.ToBytes();

                    Log.Line("NFCService: Data as bytes: " + string.Join(":", bytes));
                    tagInfo.Records = new[] {
                    new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.Mime,
                        MimeType = "application/com.companyname.nfcsample",
                        Payload = bytes
                    }};

                    Log.Line("NFCService: Data as bytes in Record: " + string.Join(":", tagInfo.Records[0].Payload));
                    _lastPayload = tagInfo.Records[0].Payload.ToList();

                    try
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Nfc.PublishMessage(tagInfo, false);
                        });
                        Log.Line("NFCService: Written successfully!");
                        DataToWrite = null;
                        FreeloaderCardWritten?.Invoke(this, new RfidEventArgs() { Data = DataToWrite, MetaData = tagInfo });
                    }
                    catch (Exception e)
                    {
                        Log.Line("NFCService: Error while writing NFC!" + e.ToString());
                    }

                }
                else
                    Log.Line($"NFCService: The Data to write does not target the presented tag. ID of presented tag: {string.Join(":",tagInfo.Identifier)}, ID of waiting data: {string.Join(":", DataToWrite.ChipUid)}");
            }
            else
                Log.Line("NFCService: There is no data to write.");

            


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
    }

    public class RfidEventArgs : EventArgs
    {
        public FreeloaderCustomerData Data { get; set; }
        public ITagInfo MetaData { get; set; }
    }
}
