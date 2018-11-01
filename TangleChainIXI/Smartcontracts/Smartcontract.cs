using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Newtonsoft.Json;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI.Smartcontracts
{

    [Serializable]
    public class Smartcontract : IDownloadable, ISignable
    {

        public string Name { get; set; }

        [JsonIgnore]
        public bool IsFinalized { get; set; } = false;

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
            Code = SmartcontractUtils.StringToCode((string)reader[4]);
            From = (string)reader[5];
            Signature = (string)reader[6];
            TransactionFee = (int)reader[7];
            SendTo = (string)reader[8];
            ReceivingAddress = (string)reader[9];

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

            if (!Code.ToFlatString().Equals(smart.Code.ToFlatString()))
                return false;

            GenerateHash();
            smart.GenerateHash();

            if (Hash.Equals(smart.Hash))
                return true;
            return false;

        }

        public void Print()
        {
            Console.WriteLine($"Hash: {Hash}\nCode: {Code.ToString()}\nSendto: {SendTo}");
        }

        /// <summary>
        /// Generates a unique hash for the smartcontract
        /// </summary>
        public IDownloadable GenerateHash()
        {
            IsFinalized = false;

            string codeHash = Code.ToFlatString().HashCurl(20);
            Hash = (SendTo + TransactionFee + Name + From).HashCurl(20);

            return this;

        }

        /// <summary>
        /// Finalizes the Smartcontract. Adds your specified Public Key, generates a Receiving address and Signs the Contract
        /// </summary>
        public Smartcontract Final()
        {

            From = IXISettings.PublicKey;
            GenerateHash();

            ReceivingAddress = Hash.GetPublicKey();

            this.Sign();

            IsFinalized = true;

            return this;
        }

        /// <summary>
        /// Adds a Fee to the object
        /// </summary>
        /// <param name="fee"></param>
        public Smartcontract AddFee(int fee)
        {
            IsFinalized = false;

            TransactionFee = fee;

            return this;
        }

        /// <summary>
        /// Adds an Expression to the Code.
        /// </summary>
        /// <param name="exp"></param>
        public Smartcontract AddExpression(int bytecode, string args1, string args2 = "", string args3 = "")
        {
            IsFinalized = false;

            Expression exp = new Expression(bytecode, args1, args2, args3);

            Code.Expressions.Add(exp);

            return this;
        }

        /// <summary>
        /// Adds an Expression to the Code.
        /// </summary>
        /// <param name="exp"></param>
        public Smartcontract AddExpression(Expression exp)
        {
            IsFinalized = false;

            Code.Expressions.Add(exp);

            return this;
        }

        /// <summary>
        /// Adds a statevariable to the code. If you want persistent storage, you need to set these vars
        /// </summary>
        /// <param name="name">The name of the State. Internally will always have "S_" prefix</param>
        /// <param name="value">The startvalue</param>
        public Smartcontract AddVariable(string name, string value = "__0")
        {
            IsFinalized = false;

            Code.Variables.Add(new Variable("S_" + name.RemoveType(), value));

            return this;
        }

        /// <summary>
        /// Signs the smartcontract with the private key specified in ixisettings
        /// </summary>
        public void Sign()
        {
            IsFinalized = false;
            Signature = Cryptography.Sign(Hash, IXISettings.PrivateKey);
        }

        public int GetFee()
        {
            return TransactionFee;
        }
    }
}
