using Plugin.NFC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Nfc.OnNfcStatusChanged += _nfc_OnNfcStatusChanged;
                Nfc.OnTagListeningStatusChanged += _nfc_OnTagListeningStatusChanged;
                Nfc.OnMessageReceived += _nfc_OnMessageReceived;
            });

            Nfc.StartListening();
            Nfc.StartPublishing();
        }

        private void _nfc_OnMessageReceived(ITagInfo tagInfo)
        {
            this.Log($"NFCService: MESSAGE RECEIVED! Tag id: {String.Join(":", tagInfo.Identifier.Select(b => b.ToString("x")))}", false);
            this.Log("NFCService: " + Newtonsoft.Json.JsonConvert.SerializeObject(tagInfo));
            this.Log($"NFCService: TagInfo type: {tagInfo.GetType().Name}");
            Nfc.StartPublishing();
        }

        private void _nfc_OnTagListeningStatusChanged(bool isListening)
        {
            this.Log($"NFCService: LISTENING STATUS CHANGED: {(isListening ? "started" : "stopped")} listening");
            if (!isListening)
                Nfc.StartListening();
        }

        private void _nfc_OnNfcStatusChanged(bool isEnabled) => this.Log($"NFCService: NFC status changed: {(isEnabled ? "NOT" : "")} enabled.");

        private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
        {
            this.Log("NFCService: TAG DISCOVERED!");

            if (DataToWrite == null)
            {
                this.Log("NFCService: There is no data to write. Reading first and giving the VM a change to react.");
                ReadTag(tagInfo);
                WriteTag(tagInfo);
            }
            else
            {
                this.Log("NFCService: There is some data to write! Writing first and then sending out the resulting Data.");
                this.Log("NFCService: Data to write:" + DataToWrite.ToJson());
                WriteTag(tagInfo);
                ReadTag(tagInfo);
            }

            Nfc.StartPublishing();
        }

        private void WriteTag(ITagInfo tagInfo)
        {
            bool recover = false;

            if (DataToWrite == null)
            {
                this.Log("NFCService: There is no data to write.");
                return;
            }

            if (!Enumerable.SequenceEqual(DataToWrite.ChipUid, tagInfo.Identifier))
            {
                this.Log($"NFCService: The Data to write does not target the presented tag. ID of presented tag: {string.Join(":", tagInfo.Identifier)}, ID of waiting data: {string.Join(":", DataToWrite.ChipUid)}");
                return;
            }

            this.Log("NFCService: There is some data to write.");

            var bytes = DataToWrite.ToBytes();

            this.Log("NFCService: Data as bytes: " + string.Join(":", bytes));
            tagInfo.Records = new[] {
                    new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.Mime,
                        MimeType = "a/c",
                        Payload = bytes
                    }};

            this.Log("NFCService: Data as bytes in Record: " + string.Join(":", tagInfo.Records[0].Payload));
            this.Log("NFCService: Data as JSON: " + DataToWrite.ToJson());
            _lastPayload = tagInfo.Records[0].Payload.ToList();

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (DataToWrite != null) // sometimes this part gets called in an invalid state
                    {
                        this.Log("NFCService: Publishing RFID message now.");
                        Nfc.PublishMessage(tagInfo, false);
                    }
                    else
                        this.Log("NFCService: Invalid state. Not publishing any RFID message.");

                    this.Log("NFCService: Written successfully!");
                    DataToWrite = null;
                    FreeloaderCardWritten?.Invoke(this, new RfidEventArgs() { Data = DataToWrite, MetaData = tagInfo });
                }
                catch (Exception ex)
                {

                    this.Log("NFCService: Tag IO Error: " + ex.ToString());

                    this.Log("Possibly the tag just got deleted! Content still in memory: " + FreeloaderCustomerData.FromBytes(tagInfo.Identifier, bytes).ToJson());
                    //Debugger.Break();
                    recover = true;
                    this.Log("Trying to recover.");
                }
            });

            if (recover)
                WriteTag(tagInfo);
        }

        private void ReadTag(ITagInfo tagInfo)
        {
            if (tagInfo.Records == null)
            {
                this.Log("NfcService: Tag contains no records. No data to parse.");
                FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { MetaData = tagInfo });
                return;
            }

            if (tagInfo.Records.Length != 1)
            {
                this.Log($"NFCService: Error: Chip contains {tagInfo.Records?.Length} records instead of exactly 1.");
                FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { MetaData = tagInfo });
                return;
            }
            var message = tagInfo.Records[0].Payload;

            if (_lastPayload != null)
                this.Log("NfcService: Previously Written Bytes: \r\n" + string.Join(":", _lastPayload));
            this.Log("NfcService: Read Bytes: \r\n" + string.Join(":", message));

            try
            {
                var data = FreeloaderCustomerData.FromBytes(tagInfo.Identifier, message);
                this.Log("NFCService: Freeloader block deserialized!");
                FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { Data = data, MetaData = tagInfo });
            }
            catch (Exception e)
            {
                this.Log("NFCService: Error while reading NFC!" + e.ToString());
                FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { MetaData = tagInfo });
            }
        }
    }

    public class RfidEventArgs : EventArgs
    {
        public FreeloaderCustomerData Data { get; set; }
        public ITagInfo MetaData { get; set; }
    }
}
