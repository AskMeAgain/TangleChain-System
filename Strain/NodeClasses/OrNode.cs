using System.Collections.Generic;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class OrNode : OperationNode
    {

        public OrNode(Node right, Node left) : base(right, left, 27)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return base.Compile(scope, context);
        }
    }
}
