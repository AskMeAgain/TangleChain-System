using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public class AssignNode : Node
    {
        private string _name;
        private string _type;

        public AssignNode(string type, string name, Node operationNode)
        {

            _name = name;
            _type = type;

            Nodes = new List<Node>() { operationNode };

        }
    }
}
