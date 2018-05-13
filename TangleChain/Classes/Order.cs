using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChain.Classes {

    [Serializable]
    public class Order {

        [BsonId]
        public string Hash { get; set; }

        [BsonId]
        public string SendTo { get; set; }

        public Order() {

        }

        public string From { get; set; }
        public string Signature { get; set; }

        public List<(int In, string Receiver)> Outputs;
        public int Mode { get; set; }
        public List<string> Data { get; set; }

        public Order(string fro, int mod, string sendt) {
            From = fro;
            Mode = mod;
            SendTo = sendt;

            Data = new List<string>();
            Outputs = new List<(int In, string Receiver)>();
        }

        public override bool Equals(object obj) {

            Order newOrder = obj as Order;

            if (newOrder == null)
                return false;

            return (newOrder.SendTo.Equals(this.SendTo) && newOrder.Hash.Equals(this.Hash)) ? true : false;
        }

        public void Sign(string privateKey) {
            Signature = privateKey;
        }

        public void SetID() {
            Hash = "LOL";
        }

        public void SetTransactionFees(int fees) {
            Data.Add(fees + "");
        }

        public void AddOutput(int _in, string _out) {

            if (_in > 0)
                return;

            Outputs.Add((_in, _out));
        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Order FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(json);
        }

        public void Print() {
            Console.WriteLine("Hash " + Hash);
            Console.WriteLine("FROM" + From);
            Console.WriteLine("Signature" + Signature);
            Console.WriteLine("Sendto" + SendTo);
            Console.WriteLine("Outputs count" + Outputs.Count);
            Console.WriteLine("Mode" + Mode);
            Console.WriteLine("Data count " + Data.Count);
            Console.WriteLine("===========================================");

        }

    }
}
