using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Smartcontracts
{
    public static class SmartcontractExtensions
    {
        /// <summary>
        /// Finalizes the Smartcontract. Adds your specified Public Key, generates a Receiving address and Signs the Contract
        /// </summary>
        public static Smartcontract Final(this Smartcontract smart)
        {

            smart.From = IXISettings.PublicKey;
            smart.GenerateHash();

            smart.ReceivingAddress = smart.Hash.GetPublicKey();

            smart.Sign();

            smart.IsFinalized = true;

            return smart;
        }

        /// <summary>
        /// Adds a Fee to the object
        /// </summary>
        /// <param name="fee"></param>
        public static Smartcontract AddFee(this Smartcontract smart, int fee)
        {
            smart.TransactionFee = fee;

            return smart;
        }

        /// <summary>
        /// Generates a unique hash for the smartcontract
        /// </summary>
        public static Smartcontract GenerateHash(this Smartcontract smart)
        {
            string codeHash = smart.Code.ToFlatString().HashCurl(20);
            smart.Hash = (smart.SendTo + smart.TransactionFee + smart.Name + smart.From).HashCurl(20);

            return smart;

        }

        /// <summary>
        /// Adds an Expression to the Code.
        /// </summary>
        /// <param name="exp"></param>
        public static Smartcontract AddExpression(this Smartcontract smart, int bytecode, string args1, string args2 = "", string args3 = "")
        {
            Expression exp = new Expression(bytecode, args1, args2, args3);

            smart.Code.Expressions.Add(exp);

            return smart;
        }

        /// <summary>
        /// Adds an Expression to the Code.
        /// </summary>
        /// <param name="exp"></param>
        public static Smartcontract AddExpression(this Smartcontract smart, Expression exp)
        {
            smart.Code.Expressions.Add(exp);

            return smart;
        }

        /// <summary>
        /// Adds a statevariable to the code. If you want persistent storage, you need to set these vars
        /// </summary>
        /// <param name="name">The name of the State. Internally will always have "S_" prefix</param>
        /// <param name="value">The startvalue</param>
        public static Smartcontract AddVariable(this Smartcontract smart, string name, string value = "__0") {

            smart.Code.Variables.Add(new Variable("S_" + name.RemoveType(), value));

            return smart;
        }
    }
}
