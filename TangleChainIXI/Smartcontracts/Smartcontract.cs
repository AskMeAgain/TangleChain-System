using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Newtonsoft.Json;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXI.Smartcontracts
{

    [Serializable]
    public class Smartcontract : IDownloadable, ISignable
    {

        public string Name { get; set; }

        [JsonIgnore]
        public bool IsFinalized { get; set; } = false;

        [JsonIgnore]
        public string NodeAddress { get; set; }


        public string SendTo { set; get; }
        public string Hash { set; get; }
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
        public Smartcontract Final(IXISettings settings)
        {

            From = settings.PublicKey;
            GenerateHash();

            ReceivingAddress = Hash.GetPublicKey();

            Sign(settings);

            NodeAddress = settings.NodeAddress;

            IsFinalized = true;

            return this;
        }


        /// <summary>
        /// Adds a Fee to the object
        /// </summary>
        /// <param name="fee"></param>
        public Smartcontract SetFee(int fee)
        {
            IsFinalized = false;

            TransactionFee = fee;

            return this;
        }

        public Smartcontract SetReceivingAddress(string addr)
        {

            ReceivingAddress = addr;

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

        public Smartcontract AddExpression(List<Expression> list)
        {
            list.ForEach(x => AddExpression(x));

            return this;
        }

        /// <summary>
        /// Adds a statevariable to the code. If you want persistent storage, you need to set these vars
        /// </summary>
        /// <param name="name">The name of the State. Internally will always have "S_" prefix</param>
        /// <param name="value">The startvalue</param>
        public Smartcontract AddVariable(string name, ISCType value)
        {
            IsFinalized = false;

            if (Code.Variables.ContainsKey(name))
            {
                Code.Variables[name] = value;
            }
            else
            {
                Code.Variables.Add(name, value);
            }
            return this;
        }

        public Smartcontract AddVariable(Dictionary<string, ISCType> dict)
        {
            if (dict != null)
                dict.Keys.ToList().ForEach(x => Code.Variables.Add(x, dict[x]));
            ;
            return this;
        }

        /// <summary>
        /// Signs the smartcontract with the private key specified in ixisettings
        /// </summary>
        public void Sign(IXISettings settings)
        {
            IsFinalized = false;
            Signature = Cryptography.Sign(Hash, settings.PrivateKey);
        }

        public int GetFee()
        {
            return TransactionFee;
        }

        public Smartcontract ApplyState(Dictionary<string, ISCType> state)
        {

            state.ToList().ForEach(x => AddVariable(x.Key, x.Value));

            return this;
        }


        public bool VerifySmartcontract()
        {

            //we need to check if smartcontract has correct hash
            if (!this.VerifyHash())
                return false;

            //we need to check if the receiving address is correct
            if (!this.VerifyReceivingAddress())
                return false;

            //we need to check if the signature is correct
            if (!this.VerifySignature())
                return false;

            return true;
        }
    }
}
