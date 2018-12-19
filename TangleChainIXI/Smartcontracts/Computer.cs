using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXI.Smartcontracts
{
    public class Computer
    {

        public bool compiled = false;

        public int InstructionPointer { get; set; } = 0;

        public Dictionary<string, ISCType> State { get; set; }
        public Dictionary<string, ISCType> Register { get; set; } = new Dictionary<string, ISCType>();
        public List<Expression> ExpressionList { get; set; }
        public Dictionary<string, int> EntryRegister { get; set; } = new Dictionary<string, int>();
        public string SmartcontractAddress { get; set; }

        public Stack<int> JumpStack { get; set; } = new Stack<int>();

        public List<string> Data { get; set; } = new List<string>();

        public Smartcontract NewestSmartcontract { get; set; }

        public Transaction OutTrans { get; set; }
        public Transaction InTrans { get; set; }

        /// <summary>
        /// Object which runs a smartcontract
        /// </summary>
        /// <param name="smart">The smartcontract which should be run</param>
        public Computer(Smartcontract smart)
        {
            SetupComputer(smart);
        }

        public Computer(List<Expression> expList, Dictionary<string, ISCType> varDict = null)
        {
            var smart = new Smartcontract("Foo", Utils.GenerateRandomString(81))
                .AddExpression(expList)
                .AddVariable(varDict)
                .SetFee(0)
                .Final();

            SetupComputer(smart);
        }

        /// <summary>
        /// Compiles the code
        /// </summary>
        public void Compile()
        {

            if (!ExpressionList.Any(exp => exp.ByteCode.Equals(05)))
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

            InstructionPointer = EntryRegister[trans.Data[1]];

            while (InstructionPointer < ExpressionList.Count)
            {

                int flag = Eval(ExpressionList[InstructionPointer]);

                if (flag == 0)
                    break;

                InstructionPointer++;

            }

            if (OutTrans.OutputReceiver.Count > 0)
            {
                //copying time of in trans
                OutTrans.Mode = 100;
                OutTrans.From = SmartcontractAddress;
                OutTrans.Final();
                return OutTrans;
            }

            return null;

        }


        public Transaction Run(string label = "Main")
        {
            var triggerTrans = new Transaction("asd", -2, "lol")
                .AddFee(0)
                .AddData(label)
                .Final();

            return Run(triggerTrans);
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
                IntroduceValue(exp);
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

            if (exp.ByteCode == 07)
            {
                IntroduceTransactionData(exp);
            }

            if (exp.ByteCode == 09)
            {
                SetOutTransaction(exp);
            }

            if (exp.ByteCode == 10)
            {
                IntroduceStateVariable(exp);
            }

            if (exp.ByteCode == 11)
            {
                IntroduceMetaData(exp);
            }

            if (exp.ByteCode == 12)
            {
                Subtract(exp);
            }

            if (exp.ByteCode == 13)
            {
                Goto(exp);
            }

            if (exp.ByteCode == 14)
            {
                BranchIfEqual(exp);
            }

            if (exp.ByteCode == 15)
            {
                IntroduceData(exp);
            }

            if (exp.ByteCode == 16)
            {
                BranchIfNotEqual(exp);
            }

            if (exp.ByteCode == 17)
            {
                BranchIfLower(exp);
            }

            if (exp.ByteCode == 18)
            {
                SwitchRegister(exp);
            }

            if (exp.ByteCode == 19)
            {
                JumpAndLink(exp);
            }

            if (exp.ByteCode == 20)
            {
                PopJump(exp);
            }

            if (exp.ByteCode == 21)
            {
                BranchIfOne(exp);
            }

            if (exp.ByteCode == 22) {
                ;
                IsSmaller(exp);
            }

            if (exp.ByteCode == 23)
            {
                IsBigger(exp);
            }

            if (exp.ByteCode == 24)
            {
                IsEqual(exp);
            }

            if (exp.ByteCode == 25)
            {
                And(exp);
            }

            if (exp.ByteCode == 26)
            {
                Negate(exp);
            }

            if (exp.ByteCode == 99)
            {
                exitFlag = true;
            }

            if (exp.ByteCode == 27)
            {
                Or(exp);
            }

            //exit function
            if (exp.ByteCode == 05 && exp.Args1.ToLower().Equals("exit"))
                return 0;

            //we exit program if exitflag is true
            return exitFlag ? 0 : 1;
        }

        private void Negate(Expression exp)
        {

            var args1Obj = Register.GetFromRegister(exp.Args1);

            if (args1Obj.IsEqual(new SC_Int(0)))
            {
                Register.AddToRegister(exp.Args1, new SC_Int(1));
            }
            else
            {
                Register.AddToRegister(exp.Args1, new SC_Int(0));
            }
        }

        private void And(Expression exp)
        {

            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            if (args1Obj == args2Obj && args1Obj.IsEqual(new SC_Int(1)))
            {
                Register.AddToRegister(exp.Args3, new SC_Int(1));
            }
            else
            {
                Register.AddToRegister(exp.Args3, new SC_Int(0));
            }

        }

        private void Or(Expression exp)
        {

            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            if (args1Obj.IsEqual(new SC_Int(1)) || args2Obj.IsEqual(new SC_Int(1)))
            {
                Register.AddToRegister(exp.Args3, new SC_Int(1));
            }
            else
            {
                Register.AddToRegister(exp.Args3, new SC_Int(0));
            }
        }

        private void IsSmaller(Expression exp)
        {
            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            //we branch
            if (args1Obj.IsLower(args2Obj))
            {
                Register.AddToRegister(exp.Args3, new SC_Int(1));
            }
            else
            {
                Register.AddToRegister(exp.Args3, new SC_Int(0));
            }
        }

        private void IsBigger(Expression exp)
        {
            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            //we branch
            if (args2Obj.IsLower(args1Obj))
            {
                Register.AddToRegister(exp.Args3, new SC_Int(1));
            }
            else
            {
                Register.AddToRegister(exp.Args3, new SC_Int(0));
            }
        }

        private void IsEqual(Expression exp)
        {
            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            //we branch
            if (!args2Obj.IsEqual(args1Obj))
            {
                Register.AddToRegister(exp.Args3, new SC_Int(0));
            }
            else
            {
                Register.AddToRegister(exp.Args3, new SC_Int(1));
            }
        }

        private void BranchIfOne(Expression exp)
        {

            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args2);

            //we branch
            if (args1Obj.GetValueAs<int>() == 1)
            {
                Goto(exp);
            }
        }

        private void PopJump(Expression exp)
        {

            if (JumpStack.Count > 0)
            {
                InstructionPointer = JumpStack.Pop();
            }
            else
            {
                exitFlag = true;
            }
        }

        private bool exitFlag = false;

        private void JumpAndLink(Expression exp)
        {

            //we remember our current instructionpointer +1
            JumpStack.Push(InstructionPointer);

            //we just do goto
            Goto(exp);

        }

        private void SwitchRegister(Expression exp)
        {

            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            Register.AddToRegister(exp.Args2, args1Obj);
            Register.AddToRegister(exp.Args1, args2Obj);
        }

        private void BranchIfLower(Expression exp)
        {
            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args2);
            var args2Obj = Register.GetFromRegister(exp.Args3);

            //we branch
            if (args1Obj.IsLower(args2Obj))
            {
                Goto(exp);
            }
        }

        private void BranchIfNotEqual(Expression exp)
        {
            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args2);
            var args2Obj = Register.GetFromRegister(exp.Args3);

            //we branch
            if (!args1Obj.IsEqual(args2Obj))
            {
                Goto(exp);
            }
        }

        private void IntroduceData(Expression exp)
        {

            //we first get args1 as int
            var index = exp.Args1.ConvertToInternalType().GetValueAs<int>();

            try
            {
                //write data to args2
                Register.AddToRegister(exp.Args2, Data[index].ConvertToInternalType());
            }
            catch (Exception e)
            {
                throw new ArgumentException("Data register probl doesnt exists!");
            }
        }

        private void BranchIfEqual(Expression exp)
        {

            //get both values
            var args1Obj = Register.GetFromRegister(exp.Args2);
            var args2Obj = Register.GetFromRegister(exp.Args3);

            //we branch
            if (args1Obj.IsEqual(args2Obj))
            {
                Goto(exp);
            }
        }

        private void Goto(Expression exp)
        {
            //first we get the label name
            var name = exp.Args1;

            //we set the instruction counter to the label counter
            InstructionPointer = EntryRegister[name];
        }

        private void Subtract(Expression exp)
        {
            //we get args1 and args2 as isctypes
            var args1Obj = Register.GetFromRegister(exp.Args1);
            var args2Obj = Register.GetFromRegister(exp.Args2);

            //we add them together
            var obj = args1Obj.Subtract(args2Obj);

            //we store in args3 now
            Register.AddToRegister(exp.Args3, obj);
        }

        private void IntroduceMetaData(Expression exp)
        {

            //first we need to get the index
            int index = exp.Args1.ConvertToInternalType().GetValueAs<int>();

            //we then get the metadata
            if (exp.Args1.Equals("Int_0"))
            {
                Register.AddToRegister(exp.Args2, ("Str_" + InTrans.Hash).ConvertToInternalType());
            }
            else if (exp.Args1.Equals("Int_1"))
            {
                Register.AddToRegister(exp.Args2, ("Str_" + InTrans.SendTo).ConvertToInternalType());
            }
            else if (exp.Args1.Equals("Int_2"))
            {
                Register.AddToRegister(exp.Args2, ("Lon_" + InTrans.Time).ConvertToInternalType());
            }
            else if (exp.Args1.Equals("Int_3"))
            {
                Register.AddToRegister(exp.Args2, ("Str_" + InTrans.From).ConvertToInternalType());
            }
            else
            {
                throw new ArgumentException("Wrong Index");
            }
        }

        private void IntroduceStateVariable(Expression exp)
        {

            //first we get the state variable from args1
            var variable = State.GetFromRegister(exp.Args1);

            //we then copy to args2
            Register.AddToRegister(exp.Args2, variable);

        }

        private void SetOutTransaction(Expression exp)
        {

            //we get the value from args1 WITHOUT TYPE PREFIX!!
            string receiver = Register.GetFromRegister(exp.Args1).GetValueAs<string>();

            //we get the value from args1 WITHOUT TYPE PrEFIX!
            int cash = Register.GetFromRegister(exp.Args2).GetValueAs<int>();

            OutTrans.AddOutput(cash, receiver);
        }

        /// <summary>
        /// Introduces a value (like 1, "100") into a register
        /// </summary>
        /// <param name="exp"></param>
        private void IntroduceValue(Expression exp)
        {

            //first we convert args1 to an isctype
            ISCType obj = exp.Args1.ConvertToInternalType();

            //we then store it in args2
            Register.AddToRegister(exp.Args2, obj);

        }

        private void IntroduceTransactionData(Expression exp)
        {
            //first we need to get the index
            int index = exp.Args1.ConvertToInternalType().GetValueAs<int>();

            //we then get the data of the data entry
            ISCType data = Data[index].ConvertToInternalType();

            //we then write data into args2
            Register.AddToRegister(exp.Args2, data);

        }

        /// <summary>
        /// Gets the state of the smartcontract. Should be called after you ran a transaction to get the newest state
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ISCType> GetCompleteState()
        {
            return State;
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
            var obj = args1Obj.Multiply(args2Obj);

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
            var obj = args1Obj.Add(args2Obj);

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

        private void SetupComputer(Smartcontract smart)
        {
            NewestSmartcontract = smart;

            SmartcontractAddress = smart.ReceivingAddress;

            //prebuild transaction:
            OutTrans = new Transaction(smart.SendTo, 0, "");
            OutTrans.AddFee(0);

            ExpressionList = smart.Code.Expressions;

            //create the state variables
            State = new Dictionary<string, ISCType>(smart.Code.Variables);

            //we get all entrys
            for (int i = 0; i < ExpressionList.Count; i++)
            {
                if (ExpressionList[i].ByteCode == 05) EntryRegister.Add(ExpressionList[i].Args1, i);
            }
        }
    }
}
