using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using Tangle.Net.Cryptography.Curl;
using Tangle.Net.Entity;
using Tangle.Net.Cryptography;
using Tangle.Net.Utils;

namespace TangleChain.Classes {

    [Serializable]
    public class Order {

        [BsonId]
        public ID Identity { get; set; }

        public long Time { get; set; }
        public string From { get; set; }
        public string Signature { get; set; }
        public int Mode { get; set; }

        public List<int> Trans_In { get; set; }
        public List<string> Trans_Receiver { get; set; }

        public List<string> Data { get; set; }

        public Order() {}

        public Order(string fro, int mod, string sendt) {

            Identity = new ID(sendt);

            From = fro;
            Mode = mod;
            Time = Timestamp.UnixSecondsTimestamp;

            Data = new List<string>();

            Trans_In = new List<int>();
            Trans_Receiver = new List<string>();
        }

        public void AddFees(int fee) {
            if (Data == null)
                Data = new List<string>();

            Data.Add(fee + "");
        }

        public override bool Equals(object obj) {

            Order newOrder = obj as Order;

            if (newOrder == null)
                return false;

            return (newOrder.Identity.SendTo.Equals(this.Identity.SendTo) && newOrder.Identity.Hash.Equals(this.Identity.Hash)) ? true : false;
        }

        public void Sign(string privateKey) {
            Signature = privateKey;
            GenerateHash();
        }

        void GenerateHash() {

            Curl curl = new Curl();

            curl.Absorb(TryteString.FromAsciiString(Identity.SendTo).ToTrits());
            curl.Absorb(TryteString.FromAsciiString(From).ToTrits());
            curl.Absorb(TryteString.FromAsciiString(Mode+"").ToTrits());
            curl.Absorb(TryteString.FromAsciiString(Data.GetHashCode()+"").ToTrits());
            curl.Absorb(TryteString.FromAsciiString(Time+"").ToTrits());

            var hash = new int[60];
            curl.Squeeze(hash);

            Identity.Hash = Converter.TritsToTrytes(hash).ToString();
        }

        public void AddOutput(int _in, string _out) {

            if (_in > 0)
                return;

            Console.WriteLine("added");

            Trans_In.Add(_in);
            Trans_Receiver.Add(_out);

        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Order FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(json);
        }

        public void Print() {
            Console.WriteLine("Hash " + Identity.Hash);
            Console.WriteLine("FROM" + From);
            Console.WriteLine("Signature" + Signature);
            Console.WriteLine("Sendto" + Identity.SendTo);
            Console.WriteLine("Mode" + Mode);
            Console.WriteLine("Data count " + Data.Count);
            Console.WriteLine("===========================================");

        }

        public class ID {
            public string Hash { get; set; }
            public string SendTo { get; set; }

            public ID(string sendt) {
                SendTo = sendt;
            }

            public ID() { }
        }

        public static Order CreateOrder(string from, string sendTo, int mode, int fees) {

            //create order
            Order order = new Order(from, mode, sendTo);

            //fill object with stuff
            order.AddFees(fees);
            order.AddOutput(30, "ASD");

            //set ID and sign order
            order.Sign("private key!");

            return order;
        }

    }
}
