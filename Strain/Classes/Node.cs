using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public class Node
    {

        public Node(params Node[] list)
        {
            Nodes = list.ToList();
        }

        public List<Node> Nodes { get; set; } = new List<Node>();

        public virtual List<Expression> Parse()
        {
            var list = new List<Expression>();

            Nodes.ForEach(x => list.AddRange(x.Parse()));

            return list;
        }

        public virtual string GetValue()
        {
            throw new NotSupportedException();
        }

        public void Add(Node node)
        {
            if (node != null)
                Nodes.Add(node);
        }
    }
}
