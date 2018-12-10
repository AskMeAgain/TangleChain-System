using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ValueNode : Node
    {
        public string Value { get; protected set; }
        public string Type { get; protected set; }

        public ValueNode(string value, string type)
        {
            Value = value;
            Type = type;
        }

        public override List<Expression> Compile(string context = null)
        {
            var list = new List<Expression>();
            list.Add(new Expression(01, ConvertPrefix(Type, Value), context + "-Value"));
            return list;
        }

        public string ConvertPrefix(string type, string value)
        {
            if (type.Equals("string")) return "Str_" + value;
            if (type.Equals("int")) return "Int_" + value;
            if (type.Equals("long")) return "Lon_" + value;

            throw new NotImplementedException("this type does not exist yet!");
        }
    }
}
