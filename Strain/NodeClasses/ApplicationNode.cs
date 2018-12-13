using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ApplicationNode : Node
    {
        public string AppName { get; protected set; }

        public ApplicationNode(string ApplicationName, List<Node> nodes)
        {
            AppName = ApplicationName;
            Nodes = nodes;
        }

        public override List<Expression> Compile(Scope scope = null, string context = null)
        {

            var list = new List<Expression>();
            scope = new Scope();

            int i = 0;
            list.AddRange(Nodes.SelectMany(x => x.Compile(scope, AppName + "-" + i++)));
            list.Add(new Expression(99));

            return list;
        }
    }
}
