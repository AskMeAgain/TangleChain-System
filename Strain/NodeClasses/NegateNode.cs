using System.Collections.Generic;
using System.Linq;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class NegateNode : Node
    {
        public NegateNode(Node node)
        {
            Nodes.Add(node);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = Nodes.Compile(scope, context, "Negate");

            var lastResult = list.Last().Args3;

            list.Add(Factory.Negate(lastResult));
            return list;
        }
    }
}
