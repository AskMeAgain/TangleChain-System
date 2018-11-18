using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ValueNode : Node
    {
        private string _value;
        private string _type;

        public ValueNode(string type, string value)
        {
            _value = value;
            _type = type;
        }

        public string GetValue()
        {
            return _value;
        }

        public override List<Expression> Compile(string context)
        {
            return new List<Expression>() {
                new Expression(01,_type+"_"+_value,context)
            };
        }
    }
}
