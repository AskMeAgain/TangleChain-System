using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace TangleChainIXI.Smartcontracts {
    [Serializable]
    public class Code {

        public List<Variable> Variables { set; get; }
        public List<Expression> Expressions { set; get; }

        public Code() {
            Expressions = new List<Expression>();
            Variables = new List<Variable>();
        }

        public void AddExpression(Expression exp) {
            Expressions.Add(exp);
        }

        public void AddVariable(string name) {
            Variables.Add(new Variable(name));
        }

        public void AddVariable(string name, string value) {
            Variables.Add(new Variable(name, value));
        }

        public override string ToString() {

            string s = "";

            Expressions.ForEach(exp => s += exp.ToString());

            s += "\n";

            Variables.ForEach(v => s += v.ToString() + "\n");

            return s;
        }

        public string ToFlatString() {
            string s = ToString();

            return s.Replace("  ", " ");

        }

    }
}
