using System.Collections.Generic;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class ApplicationNode : Node
    {
        public string AppName { get; protected set; }

        public ApplicationNode(string ApplicationName, List<Node> nodes)
        {
            AppName = ApplicationName;
            Nodes = nodes;
        }

        public override List<Expression> Compile(Scope scope = null, ParserContext context = null)
        {

            scope = new Scope();
            context = new ParserContext(AppName);

            return Nodes.Compile(scope, context);
        }
    }
}
