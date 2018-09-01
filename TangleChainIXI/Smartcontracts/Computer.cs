using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI;
using TangleChainIXI.Classes;

namespace TangleChainIXI.Smartcontracts {
    public class Computer {

        public Dictionary<string, string> State { get; set; }
        public Dictionary<string, string> Register { get; set; }
        public List<Expression> Code { get; set; }

        public Computer(Smartcontract smart) {

            State = new Dictionary<string, string>();
            Register = new Dictionary<string, string>();
            Code = smart.Code.Expressions;

        }

        public void Run() {

            int counter = 0;

            while (counter < Code.Count) {
                Eval(Code[counter]);
                counter++;
            }
        }

        public void Eval(Expression exp) {

            if (exp.ByteCode == 00) {
                Copy(exp);
            }

            if (exp.ByteCode == 01) {
                Add(exp);
            }

            if (exp.ByteCode == 03) {
                Multiply(exp);
            }

        }

        public void Multiply(Expression exp) {
            int value = GetValue(exp.Args1).ToInt() * GetValue(exp.Args2).ToInt();
            ChangeRegister(exp.Args3, value.ToString());
        }

        private void Add(Expression exp) {
            int value = GetValue(exp.Args1).ToInt() + GetValue(exp.Args2).ToInt();
            ChangeRegister(exp.Args3, value.ToString());
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

            if (name[0].Equals('_'))
                return Register[name];
            else return name;

        }
    }
}
