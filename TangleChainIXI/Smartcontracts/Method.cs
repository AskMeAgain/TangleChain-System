using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts
{
    [Serializable]
    public class Method {

        public string Method_ { get; set; }
        public List<string> Parameter { get; set; }
        public List<Expression> Expressions { get; set; }

        public Method(string name) {

            Method_ = name;
            Parameter = new List<string>();
            Parameter.Add("lol");
            Parameter.Add("lol2");

        }

        public void AddExpression(Expression exp) {

            if (Expressions == null)
                Expressions = new List<Expression>();

            Expressions.Add(exp);
        }
    }
}
