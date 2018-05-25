using LiteDB;
using System;
using System.Collections.Generic;
using Tangle.Net.Cryptography.Curl;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;

namespace TangleChain.Classes {

    [Serializable]
    public class Transaction {

        public class ID {
            public string Hash { get; set; }
            public string SendTo { get; set; }

            public ID(string sendt) {
                SendTo = sendt;
            }

            public ID() { }

            public override bool Equals(object obj) {

                ID id = obj as ID;

                return (id.Hash.Equals(Hash) && id.SendTo.Equals(SendTo)) ? true : false;
            }
        }

        [BsonId]
        public ID Identity { get; set; }

        public long Time { get; set; }
        public string From { get; set; }
        public string Signature { get; set; }
        public int Mode { get; set; }

        public List<int> Output_Value { get; set; }
        public List<string> Output_Receiver { get; set; }

        public List<string> Data { get; set; }


        public Transaction(string fro, int mod, string sendt) {

            Identity = new ID(sendt);

            From = fro;
            Mode = mod;
            Time = Timestamp.UnixSecondsTimestamp;

            Data = new List<string>();

            Output_Value = new List<int>();
            Output_Receiver = new List<string>();
        }

        public void AddFees(int fee) {
            if (Data == null)
                Data = new List<string>();

            Data.Add(fee + "");
        }

        public void Sign(string privateKey) {
            Signature = privateKey;
            GenerateHash();
        }

        public void GenerateHash() {

            Curl curl = new Curl();

            curl.Absorb(TangleNet.TryteString.FromAsciiString(Identity.SendTo).ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(From).ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(Mode+"").ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(Data.GetHashCode()+"").ToTrits());
            curl.Absorb(TangleNet.TryteString.FromAsciiString(Time+"").ToTrits());

            var hash = new int[60];
            curl.Squeeze(hash);

            Identity.Hash = Converter.TritsToTrytes(hash).ToString();
        }

        public void AddOutput(int value, string receiver) {

            if (value < 0)
                throw new ArgumentException("Wrong Transaction Input");

            Output_Value.Add(value);
            Output_Receiver.Add(receiver);

        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Transaction FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Transaction>(json);
        }

        public static Transaction CreateTransaction(string from, string sendTo, int mode, int fees) {

            //create trans
            Transaction trans = new Transaction(from, mode, sendTo);

            //fill object with stuff
            trans.AddFees(fees);
            trans.AddOutput(30, sendTo);

            //set ID and sign trans
            trans.Sign("private key!");

            return trans;
        }

        #region Utility

        public Transaction() { }

        public void Print() {
            Console.WriteLine("Hash " + Identity.Hash);
            Console.WriteLine("FROM" + From);
            Console.WriteLine("Signature" + Signature);
            Console.WriteLine("Sendto" + Identity.SendTo);
            Console.WriteLine("Mode" + Mode);
            Console.WriteLine("Data count " + Data.Count);
            Console.WriteLine("===========================================");

        }

        public override bool Equals(object obj) {

            Transaction newTrans = obj as Transaction;

            if (newTrans == null)
                return false;

            return (newTrans.Identity.Equals(Identity)) ? true : false;
        }


       


        #endregion


    }
}
