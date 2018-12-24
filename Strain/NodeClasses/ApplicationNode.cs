using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ApplicationNode : ParserNode
    {
        public string AppName { get; protected set; }

        public ApplicationNode(string ApplicationName, List<ParserNode> nodes)
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
