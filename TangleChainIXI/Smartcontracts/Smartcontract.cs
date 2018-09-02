using System;
using System.Collections.Generic;
using System.Linq;

namespace TangleChainIXI.Smartcontracts {

    [Serializable]
    public class Smartcontract {

        public string SendTo { set; get; }
        public string Hash { set; get; }
        public int Balance { set; get; }
        public Code Code { set; get; }

        public Smartcontract() {
            Code = new Code();
        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Smartcontract FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Smartcontract>(json);
        }

        public void Final() {
            Hash = Cryptography.HashCurl(Code.ToString(),20);
            SendTo = Cryptography.HashCurl(Hash + "_SMARTCONTRACT",81);
        }

        public override bool Equals(object obj) {

            Smartcontract smart = obj as Smartcontract;

            if (smart == null)
                return false;

            if (Hash.Equals(smart.Hash))
                return true;
            return false;

        }

        public void Print() {
            Console.WriteLine($"Hash: {Hash}\nCode: {Code.ToString()}\nSendto: {SendTo}");
        }


      

    }
}
