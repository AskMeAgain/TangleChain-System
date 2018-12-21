using System;
using System.Collections.Generic;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ValueNode : ParserNode
    {
        public string Value { get; protected set; }
        public string Type { get; protected set; }

        public ValueNode(string value)
        {
            Value = value;
        }

        public override List<Expression> Compile(Scope scope,ParserContext context)
        {
            var list = new List<Expression>();
            list.Add(new Expression(01, ConvertPrefix(Type, Value), context + "-Value"));
            return list;
        }

        public string ConvertPrefix(string type, string value)
        {
            var flag = int.TryParse(value, out int result);
            if (flag)
            {
                return "Int_" + value;
            }

            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                return "Str_" + value.Trim('"');
            }

            return "Lon_" + value;
        }
    }
}
