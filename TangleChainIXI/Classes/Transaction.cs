using LiteDB;
using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;
using System.Data.SQLite;
using Nethereum.Hex.HexConvertors;

namespace TangleChainIXI.Classes {

    [Serializable]
    public class Transaction {

        public string Hash { get; set; }
        public string SendTo { get; set; }

        public long Time { get; set; }
        public string From { get; set; }
        public string Signature { get; set; }
        public int Mode { get; set; }
        public List<int> OutputValue { get; set; }
        public List<string> OutputReceiver { get; set; }
        public List<string> Data { get; set; }


        public Transaction(string fro, int mod, string sendTo) {

            SendTo = sendTo;
            From = fro;
            Mode = mod;
            Data = new List<string>();
            OutputValue = new List<int>();
            OutputReceiver = new List<string>();
        }

        public void AddFee(int fee) {
            Data.Add(fee + "");
        }

        public void Final() {
            Time = Timestamp.UnixSecondsTimestamp;
            GenerateHash();
            Sign();
        }

        public void AddOutput(int value, string receiver) {

            if (value < 0)
                return;

            OutputValue.Add(value);
            OutputReceiver.Add(receiver);

        }

        public void SetGenesisInformation(int BlockReward, int RewardReduction, int ReductionFactor, int TransactionsPerBlock, int BlockTime, int TransInterval, int diffi) {

            Data = new List<string>();

            AddFee(0);

            Data.Add(BlockReward + "");
            Data.Add(RewardReduction + "");
            Data.Add(ReductionFactor + "");
            Data.Add(TransactionsPerBlock + "");
            Data.Add(BlockTime + "");
            Data.Add(TransInterval + "");
            Data.Add(diffi + "");
            Mode = -1;
        }

        public void SetGenesisInformation(ChainSettings set) {

            Data = new List<string>();

            AddFee(0);

            Data.Add(set.BlockReward + "");
            Data.Add(set.RewardReduction + "");
            Data.Add(set.ReductionFactor + "");
            Data.Add(set.TransactionsPerBlock + "");
            Data.Add(set.BlockTime + "");
            Data.Add(set.TransactionPoolInterval + "");
            Data.Add(set.DifficultyAdjustment + "");
            Mode = -1;

        }

        public void GenerateHash() {

            Curl curl = new Curl();

            curl.Absorb(TangleNet::TryteString.FromAsciiString(SendTo).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(From).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Mode + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Data.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Time + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(OutputValue.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(OutputReceiver.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Data.GetHashCode() + "").ToTrits());

            var hash = new int[60];
            curl.Squeeze(hash);

            Hash = Converter.TritsToTrytes(hash);
        }

        private void Sign() {
            Signature = (Mode == -1) ? "GENESIS" : Cryptography.Sign(Hash, IXISettings.PrivateKey);
        }

        public bool VerifySignature() {

            return Cryptography.VerifyMessage(Hash, Signature, From);
        }

        #region Utility

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Transaction FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Transaction>(json);
        }

        public Transaction() { }

        public Transaction(SQLiteDataReader reader, List<int> value, List<string> receiver, List<string> data) {

            Hash = (string)reader[1];
            Time = (long)reader[2];
            From = (string)reader[3];
            Signature = (string)reader[4];
            Mode = (int)reader[5];

            OutputValue = value;
            OutputReceiver = receiver;
            Data = data;

        }

        public int ComputeOutgoingValues() {

            int sum = 0;

            //we get the trans fees
            sum += int.Parse(Data[0]);

            //and then each outgoing transaction value
            OutputValue.ForEach(m => sum += m);

            return sum;
        }

        public int ComputeMinerReward() {
            return int.Parse(Data[0]);
        }

        public void Print() {
            Console.WriteLine("Hash " + Hash);
            Console.WriteLine("FROM " + From);
            Console.WriteLine("Signature " + Signature);
            Console.WriteLine("Sendto " + SendTo);
            Console.WriteLine("Mode" + Mode);
            Console.WriteLine("data count " + Data.Count);
            Console.WriteLine("===========================================");

        }

        public override bool Equals(object obj) {

            Transaction newTrans = obj as Transaction;

            if (newTrans == null)
                return false;

            return (newTrans.Hash.Equals(Hash) && newTrans.SendTo.Equals(SendTo));
        }


        #endregion


    }
}
