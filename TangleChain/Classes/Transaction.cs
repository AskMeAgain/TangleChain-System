using LiteDB;
using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;
using System.Data.SQLite;

namespace TangleChain.Classes {

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
            Sign("private key!");
            GenerateHash();
        }

        public void AddOutput(int value, string receiver) {

            if (value < 0)
                return;

            OutputValue.Add(value);
            OutputReceiver.Add(receiver);

        }

        private void GenerateHash() {

            Curl curl = new Curl();

            curl.Absorb(TangleNet::TryteString.FromAsciiString(SendTo).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(From).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Mode + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Data.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Time + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Signature).ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(OutputValue.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(OutputReceiver.GetHashCode() + "").ToTrits());
            curl.Absorb(TangleNet::TryteString.FromAsciiString(Data.GetHashCode() + "").ToTrits());

            var hash = new int[60];
            curl.Squeeze(hash);

            Hash = Converter.TritsToTrytes(hash);
        }

        private void Sign(string privateKey) {
            Signature = privateKey;
            GenerateHash();
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

            sum += int.Parse(Data[0]);

            OutputValue.ForEach(m => sum += m);

            return sum;
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
