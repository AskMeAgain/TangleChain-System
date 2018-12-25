using System;
using System.Collections.Generic;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ValueNode : Node
    {
        public string Value { get; protected set; }

        public ValueNode(string value)
        {
            Value = value;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return new List<Expression>() {
                Factory.IntroduceValue(ConvertPrefix(Value), context + "-Value")
            };
        }

        public string ConvertPrefix(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return "Int_" + value;
            }

            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                return "Str_" + value.Trim('"');
            }

            throw new ArgumentException("this should never happen!");
        }
    }
}
