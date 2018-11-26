using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ApplicationNode : Node
    {
        private string _appName;

        public ApplicationNode(string ApplicationName, params Node[] nodes)
        {
            _appName = ApplicationName;
            Nodes = nodes.ToList();
        }
    }
}
