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
    }
}
