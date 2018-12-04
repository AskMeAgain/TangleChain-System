using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ApplicationNode : Node
    {
        public string AppName { get; protected set; }

        public ApplicationNode(string ApplicationName, params Node[] nodes)
        {
            AppName = ApplicationName;
            Nodes = nodes.ToList();
        }

        public override List<Expression> Compile(string context = null)
        {
            var list = new List<Expression>();

            int i = 0;
            list.AddRange(Nodes.SelectMany(x => x.Compile(context + "-" + i++)));
            list.Add(new Expression(05, "Exit"));

            return list;
        }
    }
}
