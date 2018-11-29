using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ApplicationNode : Node
    {
        public string appName { get; protected set; }

        public ApplicationNode(string ApplicationName, params Node[] nodes)
        {
            appName = ApplicationName;
            Nodes = nodes.ToList();
        }
    }
}
