using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TychoClient.Models
{
    class FreeloaderCustomerData
    {
        private int _maxTransactions = 30;


        #region Properties
        private string _customerName;

        public string CustomerName
        {
            get => _customerName;
            set => _customerName = value.Length > 20 ? value.Substring(0, 20) : value;
        }

        public byte ByteId
        {
            get; set;
        }

        public byte[] Fletcher32Checksum
        {
            get;
            set;
        }

        public byte[] ChipUid
        {
            get;
            set;
        }

        public int CollapsedTransactionHistory
        {
            get;
            set;
        }

        public int CurrentBalance
        {
            get => CollapsedTransactionHistory + Transactions.Sum(t => t.Sum);
        }

        public byte AvailableAlcoholTokens
        {
            get; set;
        }

        public byte SpentAlcoholTokens
        {
            get; set;
        }

        public List<Transaction> Transactions
        {
            get; set;
        }
        #endregion Properties

        public FreeloaderCustomerData()
        {
            Transactions = new List<Transaction>();
        }

        #region Alcohol tokens
        // 0: ok, 1: checksum invalid, 2: insufficient alcohol tokens
        public int PurchaseAlcohol(int amount, string password)
        {
            if (!IsChecksumValid(password))
                return 1;
            if (AvailableAlcoholTokens < amount)
                return 2;
            AvailableAlcoholTokens -= (byte)amount;
            SpentAlcoholTokens += (byte)amount;
            Fletcher32Checksum = CalculateFletcher32(password);
            return 0;
        }

        public bool IsChecksumValid(string password)
        {
            return Enumerable.SequenceEqual(CalculateFletcher32(password), Fletcher32Checksum);
        }

        public byte[] CalculateFletcher32(string password)
        {
            //from uid, spent, available, and password
            var bytes = new List<byte>();
            bytes.AddRange(ChipUid);
            bytes.Add(ByteId);
            bytes.Add(SpentAlcoholTokens);
            bytes.Add(AvailableAlcoholTokens);
            bytes.AddRange(Encoding.ASCII.GetBytes(password));
            return GetFletcher(bytes);
        }

        public bool RechargeAlcoholTokens(int amount, string password)
        {
            if (!IsChecksumValid(password))
                return false;
            AvailableAlcoholTokens += (byte)amount;
            Fletcher32Checksum = CalculateFletcher32(password);
            return true;
        }
#endregion Alcohol tokens

        #region serialization
        // Assumes a raw payload without a preceding language tag.
        public static FreeloaderCustomerData FromBytes(byte[] uid, byte[] payload)
        {
            var ret = new FreeloaderCustomerData();
            ret.ChipUid = new List<byte>(uid).ToArray();
            ret.CustomerName = Encoding.ASCII.GetString(payload.Take(20).ToArray()).TrimEnd();
            ret.ByteId = payload.Skip(20).First();
            ret.CollapsedTransactionHistory = BitConverter.ToInt32(payload.Skip(21).Take(4).ToArray(), 0);
            ret.AvailableAlcoholTokens = payload[25];
            ret.SpentAlcoholTokens = payload[26];
            ret.Fletcher32Checksum = payload.Skip(27).Take(4).ToArray();
            // header finished, now parse the history
            var payloadCopy = payload.Skip(31).ToList();
            while (payloadCopy.Count >= 3)
            {
                var t = new Transaction();
                t.Sum = BitConverter.ToInt16(payloadCopy.Take(2).ToArray(), 0);
                t.PartnerByteId = payloadCopy.Skip(2).First();
                payloadCopy = payloadCopy.Skip(3).ToList();
                ret.Transactions.Add(t);
            }
            return ret;
        }


        // Serializes the object for use in an NDEF message record payload.
        public byte[] ToBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(CustomerName));
            while (bytes.Count < 20)
                bytes.Add((byte)32); // Pad with 32 (Space in ASCII). Trim on deserialization.
            bytes.Add(ByteId);
            bytes.AddRange(BitConverter.GetBytes(CollapsedTransactionHistory));
            bytes.Add(AvailableAlcoholTokens);
            bytes.Add(SpentAlcoholTokens);
            bytes.AddRange(Fletcher32Checksum);
            foreach (var t in Transactions)
            {
                bytes.AddRange(BitConverter.GetBytes(t.Sum));
                bytes.Add(t.PartnerByteId);
            }

            return bytes.ToArray();
        }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        #endregion serialization

        #region player transaction
        public void ChargeFromCard(Int16 amount)
        {
            if (Transactions.Count == _maxTransactions)
            {
                CollapsedTransactionHistory += Transactions.First().Sum;
                Transactions = Transactions.Skip(1).ToList();
            }
            Transactions.Add(new Transaction() { Sum = (short)(-1 * amount) });
        }

        // security checks (is the right card presented, is the amount valid, etc.) must be made by the caller!
        public void AddToCard(Int16 amount, byte partnerByteId)
        {
            if (Transactions.Count == _maxTransactions)
            {
                CollapsedTransactionHistory += Transactions.First().Sum;
                Transactions = Transactions.Skip(1).ToList();
            }
            Transactions.Add(new Transaction() { Sum = amount, PartnerByteId = partnerByteId });
        }


        public void FinalizeTransaction(byte partnerByteId)
        {
            if (Transactions.LastOrDefault() is Transaction t)
                t.PartnerByteId = partnerByteId;
        }
        #endregion player transaction

        #region Fletcher32 calculation
        private IEnumerable<ulong> Blockify(IReadOnlyList<byte> inputAsBytes, int blockSize)
        {
            var i = 0;
            ulong block = 0;

            while (i < inputAsBytes.Count)
            {
                block = (block << 8) | inputAsBytes[i];
                i++;

                if (i % blockSize != 0 && i != inputAsBytes.Count) continue;

                yield return block;
                block = 0;
            }
        }

        private byte[] GetFletcher(IReadOnlyList<byte> input) // Fletcher 32
        {
            var n = 32;
            var bytesPerCycle = 2;
            var modValue = (ulong)(Math.Pow(2, 8 * bytesPerCycle) - 1);

            ulong sum1 = 0;
            ulong sum2 = 0;

            foreach (var block in Blockify(input, bytesPerCycle))
            {
                sum1 = (sum1 + block) % modValue;
                sum2 = (sum2 + sum1) % modValue;
            }

            return BitConverter.GetBytes(sum1 + sum2 * (modValue + 1)).Take(4).ToArray();
        }
        #endregion Fletcher32 calculation
    }

    public class Transaction
    {
        // positive value means money got added to this chip
        public Int16 Sum
        {
            get; set;
        }

        public byte PartnerByteId
        {
            get; set;
        }

        public bool Eq(Transaction t)
        {
            return Sum == t.Sum && PartnerByteId == t.PartnerByteId;
        }
    }
}
