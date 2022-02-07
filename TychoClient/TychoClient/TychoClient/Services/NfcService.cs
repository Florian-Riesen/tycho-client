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
        private Dictionary<byte[], FreeloaderCustomerData> _emergencyCache = new Dictionary<byte[], FreeloaderCustomerData>(new ByteArrayComparer());

        private static NfcService _instance;
        private List<Byte> _lastPayload;
        private FreeloaderCustomerData _dataToWrite;

        protected INFC Nfc => CrossNFC.Current;

        public FreeloaderCustomerData DataToWrite
        {
            get => _dataToWrite;
            set
            {
                _dataToWrite = value;
                if(value != null)
                    UpdateEmergencyCache(value);
            }
        }

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
            this.Log("TAG DISCOVERED!");
            var parsedData = ParseDataFromTag(tagInfo);

            if (parsedData is null && _emergencyCache.ContainsKey(tagInfo.Identifier))
            {
                this.Log("Cache hit - ALERT! TAG WAS MISTAKENLY DELETED! Trying to restore it.");
                var dataToWriteTemp = DataToWrite;
                DataToWrite = parsedData = _emergencyCache[tagInfo.Identifier];
                WriteToTag(tagInfo);
                DataToWrite = dataToWriteTemp;
            }

            if (DataToWrite == null)
            {
                this.Log("NFCService: There is no data to write. Reading first and giving the VM a change to react.");
                FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { Data = parsedData, MetaData = tagInfo });
                WriteToTag(tagInfo);
            }
            else
            {
                this.Log("NFCService: There is some data to write! Writing first and then sending out the resulting Data.");
                var oldDataToWrite = DataToWrite;
                FreeloaderCardScanned?.Invoke(this, new RfidEventArgs() { Data = WriteToTag(tagInfo) ? oldDataToWrite : parsedData, MetaData = tagInfo });
            }

            Nfc.StartPublishing();
        }

        private bool WriteToTag(ITagInfo tagInfo)
        {
            if (DataToWrite == null)
            {
                this.Log("There is no data to write.");
                return false;
            }

            if (!Enumerable.SequenceEqual(DataToWrite.ChipUid, tagInfo.Identifier))
            {
                this.Log($"NFCService: The Data to write does not target the presented tag. ID of presented tag: {string.Join(":", tagInfo.Identifier)}, ID of waiting data: {string.Join(":", DataToWrite.ChipUid)}");
                return false;
            }

            this.Log("There is some data to write.");

            var bytes = DataToWrite.ToBytes();

            this.Log("Data as bytes: " + string.Join(":", bytes));
            tagInfo.Records = new[] {
                    new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.Mime,
                        MimeType = "a/c",
                        Payload = bytes
                    }};
            
            _lastPayload = tagInfo.Records[0].Payload.ToList();

            try
            {
                if (DataToWrite != null) // sometimes this part gets called in an invalid state
                {
                    this.Log("NFCService: Publishing RFID message now.");
                    Nfc.PublishMessage(tagInfo, false);
                }
                else
                {
                    this.Log("Invalid state. Not publishing any RFID message.");
                    return false;
                }

                this.Log("NFCService: Written successfully!");
                DataToWrite = null;
                FreeloaderCardWritten?.Invoke(this, new RfidEventArgs() { Data = DataToWrite, MetaData = tagInfo });
            }
            catch (Exception ex)
            {
                Debugger.Break();
                this.Log("NFCService: Tag IO Error: " + ex.ToString());
                this.Log("Possibly the tag just got deleted! Content still in memory.");
                return false;
            }
            return true;
        }

        private FreeloaderCustomerData ParseDataFromTag(ITagInfo tagInfo)
        {
            if (tagInfo.Records == null)
            {
                this.Log("Tag contains no records.");
                return null;
            }

            if (tagInfo.Records.Length != 1)
            {
                this.Log($"NFCService: Error: Chip contains {tagInfo.Records?.Length} records instead of exactly 1.");
                return null;
            }
            var message = tagInfo.Records[0].Payload;

            try
            {
                var data = FreeloaderCustomerData.FromBytes(tagInfo.Identifier, message);
                UpdateEmergencyCache(data);
                this.Log("Freeloader data deserialized.");
                return data;
            }
            catch (Exception e)
            {
                this.Log("Error while parsing tag content!" + e.ToString());
                return null;
            }

        }

        private void UpdateEmergencyCache(FreeloaderCustomerData data)
        {
            if (data is null)
                return;
            
            if (_emergencyCache.ContainsKey(data.ChipUid))
                _emergencyCache[data.ChipUid] = data;
            else
                _emergencyCache.Add(data.ChipUid, data);
        }
    }

    public class RfidEventArgs : EventArgs
    {
        public FreeloaderCustomerData Data { get; set; }
        public ITagInfo MetaData { get; set; }
    }

    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }
            return left.SequenceEqual(right);
        }
        public int GetHashCode(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            return key.Sum(b => b);
        }
    }
}
