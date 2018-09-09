using System;
using System.Collections.Generic;
using System.Linq;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Smartcontracts {

    [Serializable]
    public class Smartcontract {

        public string Name { get; set; }
        public string SendTo { set; get; }
        public string Hash { set; get; }
        public int Balance { set; get; }
        public Code Code { set; get; }

        public int TransactionFee { get; set; }
        public string Signature { get; set; }
        public string From { get; set; }

        public Smartcontract() {
            Code = new Code();
        }

        public Smartcontract(string name, string sendto) {
            Code = new Code();
            Name = name;
            SendTo = sendto;
        }


        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Smartcontract FromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Smartcontract>(json);
        }

        public void Final() {

            From = IXISettings.PublicKey;

            GenerateHash();
            Sign();

        }

        public void AddFee(int fee) {
            TransactionFee = fee;
        }

        private void GenerateHash() {
            string codeHash = Cryptography.HashCurl(Code.ToString(), 20);
            Hash = Cryptography.HashCurl(SendTo + TransactionFee + TransactionFee + SendTo, 20);
        }

        private void Sign() {
            Signature = Cryptography.Sign(Hash, IXISettings.PrivateKey);
        }

        public bool VerifySignature() {
            return Cryptography.VerifyMessage(Hash, Signature, From);
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
