using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace TangleChainIXI.Classes {
    [Serializable]
    public class Code {

        public List<Expression> Expressions { set; get; }

        public Code() {
            Expressions = new List<Expression>();
        }

        public void AddExpression(Expression exp) {
            Expressions.Add(exp);
        }

        public override string ToString() {

            string s = "";

            Expressions.ForEach(exp => s += exp.ToString());

            return s;
        }

    }
}
