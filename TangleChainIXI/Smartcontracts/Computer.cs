using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using System.Linq;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Smartcontracts
{
    public class Computer
    {

        public bool compiled = false;

        public Dictionary<string, string> State { get; set; }
        public Dictionary<string, string> Register { get; set; }
        public List<Expression> Code { get; set; }
        public Dictionary<string, int> EntryRegister { get; set; }
        public string SmartcontractAddress { get; set; }

        public List<string> Data;

        public Smartcontract NewestSmartcontract { get; set; }

        public Transaction OutTrans { get; set; }
        public Transaction InTrans { get; set; }

        /// <summary>
        /// Object which runs a smartcontract
        /// </summary>
        /// <param name="smart">The smartcontract which should be run</param>
        public Computer(Smartcontract smart)
        {

            NewestSmartcontract = smart;

            State = new Dictionary<string, string>();
            Register = new Dictionary<string, string>();
            EntryRegister = new Dictionary<string, int>();
            Data = new List<string>();

            SmartcontractAddress = smart.ReceivingAddress;

            //prebuild transaction:
            OutTrans = new Transaction(smart.SendTo, 0, "");
            OutTrans.AddFee(0);

            Code = smart.Code.Expressions;

            //create the state variables
            smart.Code.Variables.ForEach(var => State.Add(var.Name, var.Value ?? "__0"));

            //we get all entrys
            for (int i = 0; i < Code.Count; i++)
            {
                if (Code[i].ByteCode == 05)
                    EntryRegister.Add(Code[i].Args1, i);
            }

        }

        /// <summary>
        /// Compiles the code
        /// </summary>
        public void Compile()
        {

            if (!Code.Any(exp => exp.ByteCode.Equals(05)))
            {
                throw new ArgumentException("Your code doesnt have any entry points!");
            }

            compiled = true;
        }

        /// <summary>
        /// Runs the specified smartcontract from the constructor with a given transaction
        /// </summary>
        /// <param name="trans">Data[1] inside transaction specifies where you enter the program</param>
        /// <returns>Returns an out transaction which should be added to your DB</returns>
        public Transaction Run(Transaction trans)
        {

            Data = trans.Data;
            InTrans = trans;

            int instructionPointer = EntryRegister[trans.Data[1]];

            while (instructionPointer < Code.Count)
            {

                int flag = Eval(Code[instructionPointer]);

                instructionPointer++;

                if (flag == 0)
                    break;
            }

            //copying time of in trans
            OutTrans.Mode = 100;
            OutTrans.From = SmartcontractAddress;
            OutTrans.Final();

            //maybe later
            //NewestSmartcontract.Balance -= OutTrans.ComputeOutgoingValues();

            return OutTrans;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private int Eval(Expression exp)
        {

            if (exp.ByteCode == 00)
            {
                Copy(exp);
            }

            if (exp.ByteCode == 01)
            {
                Add(exp);
            }

            if (exp.ByteCode == 03)
            {
                Multiply(exp);
            }

            if (exp.ByteCode == 06)
            {
                SetState(exp);
            }

            if (exp.ByteCode == 09) {
                string receiver = GetValue(exp.Args1).RemoveType();
                int cash = exp.Args2._Int();
                OutTrans.AddOutput(cash,receiver);
            }

            //exit function
            if (exp.ByteCode == 05 && exp.Args1.ToLower().Equals("exit"))
                return 0;

            return 1;

        }

        /// <summary>
        /// Gets the state of the smartcontract. Should be called after you ran a transaction to get the newest state
        /// </summary>
        /// <returns></returns>
        public Smartcontract GetCompleteState()
        {
            State.Keys.ToList().ForEach(x => NewestSmartcontract.Code.AddVariable(x, State[x]));

            return NewestSmartcontract;
        }

        /// <summary>
        /// Sets the state variables
        /// </summary>
        /// <param name="exp"></param>
        private void SetState(Expression exp)
        {
            State[exp.Args2] = GetValue(exp.Args1);
        }

        /// <summary>
        /// Multiplies two registers together and stores them inside another register
        /// </summary>
        /// <param name="exp"></param>
        private void Multiply(Expression exp)
        {
            int value = GetValue(exp.Args1)._Int() * GetValue(exp.Args2)._Int();
            ChangeRegister(exp.Args3, value._String());
        }

        /// <summary>
        /// adds two registers together and stores them inside another register
        /// </summary>
        /// <param name="exp"></param>
        private void Add(Expression exp)
        {
            int value = GetValue(exp.Args1)._Int() + GetValue(exp.Args2)._Int();
            ChangeRegister(exp.Args3, value._String());
        }

        /// <summary>
        /// Copys a value from a register into another register
        /// </summary>
        /// <param name="exp"></param>
        private void Copy(Expression exp)
        {
            string value = GetValue(exp.Args1);
            ChangeRegister(exp.Args2, value);
        }

        /// <summary>
        /// Gets a Register and a value and sets the given register to the value
        /// </summary>
        /// <param name="register">The Register you want to change</param>
        /// <param name="value">The value</param>
        public void ChangeRegister(string register, string value)
        {
            if (!Register.ContainsKey(register))
                Register.Add(register, value);
            else
                Register[register] = value;
        }

        /// <summary>
        /// Gets the value of the given string. ALWAYS RETURNS PREFLAG
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetValue(string name)
        {

            if (!name[1].Equals('_'))
                throw new ArgumentException("sorry but your input is wrong formated");

            char pre = name[0];

            if (pre.Equals('R'))
                return GetRegisterValue(name);

            if (pre.Equals('D'))
                return GetDataValue(name);

            if (pre.Equals('S'))
                return GetStateValue(name);

            if (pre.Equals('_'))
                return name;

            if (pre.Equals('T'))
            {
                return GetTransactionDetails(name);
            }

            throw new ArgumentException("sorry but your pre flag doesnt exist!");

        }

        /// <summary>
        /// Returns the transaction details. Helper function
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string GetTransactionDetails(string s)
        {

            if (s._Int() == 0)
                return "__" + InTrans.Hash;

            if (s._Int() == 1)
                return "__" + InTrans.SendTo;

            if (s._Int() == 2)
                return "__" + InTrans.Time + "";

            return "__" + InTrans.From;

        }

        /// <summary>
        /// Returns the value from a register
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetRegisterValue(string name)
        {
            if (Register.ContainsKey(name))
                return Register[name];
            throw new ArgumentException("Register doesnt exist!");
        }

        /// <summary>
        /// returns the value from a state
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetStateValue(string name)
        {
            if (State.ContainsKey(name))
                return State[name];
            throw new ArgumentException("State doesnt exist!");
        }

        /// <summary>
        /// returns the value from the datafield of a transaction
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetDataValue(string name)
        {

            int test2 = name._Int();

            return "__" + Data[test2];
        }
    }
}
