using System.Collections.Generic;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class AdditionNode : OperationNode
    {

        public AdditionNode(Node right, Node left) : base(right, left, 03)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return base.Compile(scope, context);
        }
    }
}
