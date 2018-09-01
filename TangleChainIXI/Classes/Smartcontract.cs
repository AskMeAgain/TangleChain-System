using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Classes {

    [Serializable]
    public class Smartcontract {

        public string SendTo { set; get; }
        public string Hash { set; get; }
        public Code Code { set; get; }

        public Smartcontract() { }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Smartcontract FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Smartcontract>(json);
        }

        public void AddCode(Code code) {
            Code = code;
        }

        public void Final() {
            SendTo = Utils.GenerateRandomString(81);
            Hash = GetHashCode() + "";
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
            Console.WriteLine($"Hash: {Hash}\nCode: {Code}\nSendto: {SendTo}");
        }

    }
}
