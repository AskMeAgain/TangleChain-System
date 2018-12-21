using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class LengthNode : ParserNode
    {

        public string Name { get; set; }

        public LengthNode(string name)
        {
            Name = name;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context) {
            ;
            return new ValueNode(scope.ArrayIndex[Name].ToString()).Compile(scope, context.NewContext("ArrayLength"));
        }
    }
}
