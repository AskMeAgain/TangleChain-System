using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class Node
    {
        public List<Node> Nodes { get; protected set; } = new List<Node>();

        public Node(params Node[] list)
        {
            Nodes = list.ToList();
        }

        public virtual List<Expression> Compile(string context = null)
        {
            throw new NotImplementedException("not implemented!");
        }
    }
}
