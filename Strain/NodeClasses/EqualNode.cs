using System.Collections.Generic;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class EqualNode : OperationNode
    {
        public EqualNode(Node right, Node left) : base(right, left, 24)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return base.Compile(scope, context);
        }
    }
}
