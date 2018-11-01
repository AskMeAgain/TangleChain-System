using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts.Classes;
using TangleChainIXI.Smartcontracts.Interfaces;

namespace TangleChainIXI.Smartcontracts
{
    public class Computer
    {

        public bool compiled = false;

        public Dictionary<string, ISCType> State { get; set; }
        public Dictionary<string, ISCType> Register { get; set; }
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

            State = new Dictionary<string, ISCType>();
            Register = new Dictionary<string, ISCType>();

            EntryRegister = new Dictionary<string, int>();
            Data = new List<string>();

            SmartcontractAddress = smart.ReceivingAddress;

            //prebuild transaction:
            OutTrans = new Transaction(smart.SendTo, 0, "");
            OutTrans.AddFee(0);

            Code = smart.Code.Expressions;

            //create the state variables
            smart.Code.Variables.ForEach(var => State.Add(var.Name, var.Value.ConvertToInternalType() ?? new SC_Int()));

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
                Introduce(exp);
            }

            if (exp.ByteCode == 03)
            {
                Add(exp);
            }

            if (exp.ByteCode == 04)
            {
                Multiply(exp);
            }


            if (exp.ByteCode == 06)
            {
                SetState(exp);
            }

            if (exp.ByteCode == 09)
            {
                SetOutTransaction(exp);
            }

            //exit function
            if (exp.ByteCode == 05 && exp.Args1.ToLower().Equals("exit"))
                return 0;

            return 1;

        }

        private void SetOutTransaction(Expression exp)
        {

            //we get the value from args1 WITHOUT TYPE PREFIX!!
            string receiver = Register.GetFromRegister(exp.Args1).GetValueAsString();

            //we get the value from args1 WITHOUT TYPE PrEFIX!
            int cash = Register.GetFromRegister(exp.Args2).GetValueAsInt();

            OutTrans.AddOutput(cash, receiver);
        }

        /// <summary>
        /// Introduces a value (like 1, "100") into a register
        /// </summary>
        /// <param name="exp"></param>
        private void Introduce(Expression exp)
        {

            //first we convert args1 to an isctype
            ISCType obj = exp.Args1.ConvertToInternalType();

            //we then store it in args2
            Register.AddToRegister(exp.Args2, obj);

        }

        /// <summary>
        /// Gets the state of the smartcontract. Should be called after you ran a transaction to get the newest state
        /// </summary>
        /// <returns></returns>
        public Smartcontract GetCompleteState()
        {
            NewestSmartcontract.Code.Variables.RemoveAll(x => true);
            State.Keys.ToList().ForEach(x => NewestSmartcontract.AddVariable(x, State[x].GetValueAsString()));

            return NewestSmartcontract;
        }

        /// <summary>
        /// Sets the state variables
        /// </summary>
        /// <param name="exp"></param>
        private void SetState(Expression exp)
        {

            //first we get args1
            var obj = Register.GetFromRegister(exp.Args1);

            //we add to register!
            State.AddToRegister(exp.Args2, obj);
        }

        /// <summary>
        /// Multiplies two registers together and stores them inside another register
        /// </summary>
        /// <param name="exp"></param>
        private void Multiply(Expression exp)
        {
            //first get args 1 and args2
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            //we multiply them together
            var obj = OperatorUtils.Add(args1Obj, args2Obj);

            //we store in args3 now
            Register.AddToRegister(exp.Args3, obj);
        }

        /// <summary>
        /// adds two registers together and stores them inside another register
        /// </summary>
        /// <param name="exp"></param>
        private void Add(Expression exp)
        {
            //we get args1 and args2 as isctypes
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            //we add them together
            var obj = OperatorUtils.Add(args1Obj, args2Obj);

            //we store in args3 now
            Register.AddToRegister(exp.Args3, obj);

        }

        /// <summary>
        /// Copys a register into another register
        /// </summary>
        /// <param name="exp"></param>
        private void Copy(Expression exp)
        {
            //first we get the register value:
            ISCType obj = Register.GetFromRegister(exp.Args1);

            //we then move the obj to args2
            Register.AddToRegister(exp.Args2, obj);
        }
    }
}
