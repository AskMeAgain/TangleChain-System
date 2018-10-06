using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI.Smartcontracts
{

    [Serializable]
    public class Smartcontract : IDownloadable
    {

        public string Name { get; set; }
        public bool IsFinalized { get; private set; }
        public string SendTo { set; get; }
        public string Hash { set; get; }
        public int Balance { set; get; }
        public Code Code { set; get; }

        public int TransactionFee { get; set; }
        public string Signature { get; set; }
        public string From { get; set; }
        public string ReceivingAddress { get; set; }

        /// <summary>
        /// General constructor for JSON convertion
        /// </summary>
        public Smartcontract()
        {
            Code = new Code();
        }

        /// <summary>
        /// Basic Smartcontract constructor
        /// </summary>
        /// <param name="name">The name for the smartcontract</param>
        /// <param name="sendto">The address where to send the smartcontract</param>
        public Smartcontract(string name, string sendto)
        {
            Code = new Code();
            Name = name;
            SendTo = sendto;
        }

        /// <summary>
        /// Creates a smartcontract from the result of an SQLiteDataReader
        /// </summary>
        /// <param name="reader"></param>
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

        /// <summary>
        /// A function which allows to convert a flattened code to back to code again.
        /// Is the Reverse of Code.ToFlatString();
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Code Object</returns>
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
                if (repl[0].Contains("Name:")) {
                    code.AddVariable(repl[1], repl[3].Substring(2));

                }else if (repl.Count() == 2)
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

        /// <summary>
        /// Finalizes the Smartcontract. Adds your specified Public Key, generates a Receiving address and Signs the Contract
        /// </summary>
        public void Final()
        {

            From = IXISettings.PublicKey;
            GenerateHash();

            ReceivingAddress = Cryptography.GetPublicKey(Hash);

            Sign();

            IsFinalized = true;

        }

        /// <summary>
        /// Adds a Fee to the object
        /// </summary>
        /// <param name="fee"></param>
        public void AddFee(int fee)
        {
            TransactionFee = fee;
        }

        /// <summary>
        /// Generates a unique hash for the smartcontract
        /// </summary>
        public void GenerateHash() {
            string codeHash = Code.ToFlatString().HashCurl(20);
            Hash = (SendTo + TransactionFee + Name + From).HashCurl(20);

        }

        /// <summary>
        /// Signs the smartcontract with the private key specified in ixisettings
        /// </summary>
        public void Sign()
        {
            Signature = Cryptography.Sign(Hash, IXISettings.PrivateKey);
        }

        /// <summary>
        /// Returns a boolean which indicates wheather the smartcontract was correctly signed or not
        /// </summary>
        /// <returns>If true the smartcontract is correctly signed</returns>
        public bool Verify()
        {
            return Hash.VerifyMessage(Signature, From);
        }

        /// <summary>
        /// Equality comparer
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
