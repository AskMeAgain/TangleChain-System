using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public class AddNode : Node
    {

        public AddNode(Node left, Node right)
        {
            Nodes = new List<Node>();
            Nodes.Add(left);
            Nodes.Add(right);
        }

        public override List<Node> Nodes { get; set; }

        public override List<Expression> Parse()
        {
            if (Nodes.Count != 2) throw new ArgumentException("Error node! you have more then 2 assign in the node list");

            return new List<Expression>() {
                new Expression(03, Nodes[0].GetValue(), Nodes[1].GetValue())
            };
        }

        public override string GetValue()
        {
            throw new NotImplementedException();
        }
    }
}
