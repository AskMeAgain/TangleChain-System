using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using System.Linq;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Smartcontracts {
    public class Computer {

        public bool compiled = false;

        public Dictionary<string, string> State { get; set; }
        public Dictionary<string, string> Register {
            get {
                //do error handling here!!
            }
            set}
        public List<Expression> Code { get; set; }
        public Dictionary<string, int> EntryRegister { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public Computer(Smartcontract smart) {

            State = new Dictionary<string, string>();
            Register = new Dictionary<string, string>();
            EntryRegister = new Dictionary<string, int>();
            Data = new Dictionary<string, string>();

            Code = smart.Code.Expressions;

            //create the state variables
            smart.Code.Variables.ForEach(var => State.Add("S_" + var.Name, "__0"));

            //we get all entrys
            for (int i = 0; i < Code.Count; i++) {
                if (Code[i].ByteCode == 05)
                    EntryRegister.Add(Code[i].Args1, i);
            }

        }

        public void Compile() {

            if (!Code.Any(exp => exp.ByteCode.Equals(05))) {
                throw new ArgumentException("You code doesnt have any entry points!");
            }

            compiled = true;
        }

        public void Run(string Entry) {

            int instructionPointer = EntryRegister[Entry];

            while (instructionPointer < Code.Count) {

                int flag = Eval(Code[instructionPointer]);

                instructionPointer++;

                if(flag == 0)
                    break;
            }


        }

        public int Eval(Expression exp) {

            //if (exp.Args1.Equals("S_State"))
            //    Console.WriteLine("");

            if (exp.ByteCode == 00) {
                Copy(exp);
            }

            if (exp.ByteCode == 01) {
                Add(exp);
            }

            if (exp.ByteCode == 03) {
                Multiply(exp);
            }

            if (exp.ByteCode == 06) {
                SetState(exp);
            }

            //exit function
            if (exp.ByteCode == 05 && exp.Args1.Equals("exit"))
                return 0;

            return 1;

        }

        private void SetState(Expression exp) {

            int value = GetValue(exp.Args1)._Int();
            State[exp.Args2] = value._String();
        }

        private void Multiply(Expression exp) {
            int value = GetValue(exp.Args1)._Int() * GetValue(exp.Args2)._Int();
            ChangeRegister(exp.Args3, value._String());
        }

        private void Add(Expression exp) {
            int value = GetValue(exp.Args1)._Int() + GetValue(exp.Args2)._Int();
            ChangeRegister(exp.Args3, value._String());
        }

        private void Copy(Expression exp) {
            string value = GetValue(exp.Args1);
            ChangeRegister(exp.Args2, value);
        }

        public void ChangeRegister(string name, string value) {
            if (!Register.ContainsKey(name))
                Register.Add(name, value);
            else
                Register[name] = value;
        }



        public string GetValue(string name) {

            if (!name[1].Equals('_'))
                throw new ArgumentException("sorry but your input is wrong formated");

            char pre = name[0];

            if (pre.Equals('R'))
                return Register[name];

            if (pre.Equals('D'))
                return Data[name];

            if (pre.Equals('S'))
                return State[name];

            if (pre.Equals('_'))
                return name;

            throw new ArgumentException("sorry but your pre flag doesnt exist!");

        }
    }
}
