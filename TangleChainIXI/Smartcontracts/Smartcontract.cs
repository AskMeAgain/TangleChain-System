using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Smartcontracts
{

    [Serializable]
    public class Smartcontract
    {

        public string Name { get; set; }
        public string SendTo { set; get; }
        public string Hash { set; get; }
        public int Balance { set; get; }
        public Code Code { set; get; }

        public int TransactionFee { get; set; }
        public string Signature { get; set; }
        public string From { get; set; }
        public string ReceivingAddress { get; set; }

        public Smartcontract()
        {
            Code = new Code();
        }

        public Smartcontract(string name, string sendto)
        {
            Code = new Code();
            Name = name;
            SendTo = sendto;
        }

        public Smartcontract(SQLiteDataReader reader)
        {

            Name = (string)reader[1];
            Hash = (string)reader[2];
            Balance = (int)reader[3];
            Code = StringToCode((string)reader[4]);
            From = (string)reader[5];
            Signature = (string)reader[6];
            TransactionFee = (int)reader[7];
            SendTo = (string)reader[8];
            ReceivingAddress = (string)reader[9];

        }

        public static Code StringToCode(string s)
        {
            Code code = new Code();

            s = s.Replace("\n", "");
            s = s.Replace("  ", " ");
            s = s.Replace(" ;", ";");
            var expArray = s.Split(';');

            foreach (string exp in expArray)
            {

                var repl = exp.Split(' ');

                //dirty
                if (repl.Count() == 2)
                {
                    code.AddExpression(new Expression(int.Parse(repl[0]), repl[1]));
                }
                else if (repl.Count() == 3)
                {
                    code.AddExpression(new Expression(int.Parse(repl[0]), repl[1], repl[2]));
                }
                else if (repl.Count() == 4)
                {
                    code.AddExpression(new Expression(int.Parse(repl[0]), repl[1], repl[2], repl[3]));
                }
            }

            return code;
        }


        public string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static Smartcontract FromJSON(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Smartcontract>(json);
        }

        public void Final()
        {

            From = IXISettings.PublicKey;
            GenerateHash();

            ReceivingAddress = Cryptography.GetPublicKey(Hash);

            Sign();

        }

        public void AddFee(int fee)
        {
            TransactionFee = fee;
        }

        private void GenerateHash()
        {
            string codeHash = Cryptography.HashCurl(Code.ToFlatString(), 20);
            Hash = Cryptography.HashCurl(SendTo + TransactionFee + Name + From, 20);
        }

        private void Sign()
        {
            Signature = Cryptography.Sign(Hash, IXISettings.PrivateKey);
        }

        public bool VerifySignature()
        {
            return Cryptography.VerifyMessage(Hash, Signature, From);
        }

        public override bool Equals(object obj)
        {       

            Smartcontract smart = obj as Smartcontract;

            if (smart == null)
                return false;

            if(!smart.Code.ToFlatString().Equals(Code.ToFlatString()))
                return false;



            smart.GenerateHash();
            this.GenerateHash();

            if (smart.Hash.Equals(this.Hash))
                return true;
            return false;

        }

        public void Print()
        {
            Console.WriteLine($"Hash: {Hash}\nCode: {Code.ToString()}\nSendto: {SendTo}");
        }




    }
}
