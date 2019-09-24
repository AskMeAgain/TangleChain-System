using System.Collections.Generic;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class EmptyNode : Node
    {
        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return new List<Expression>();
        }
    }
}
