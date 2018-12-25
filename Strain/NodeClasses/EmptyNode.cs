using System;
using System.Collections.Generic;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class EmptyNode : Node
    {
        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return new List<Expression>();
        }
    }
}
