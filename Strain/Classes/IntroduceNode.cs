using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public class IntroduceNode : Node
    {

        public IntroduceNode(Node assignee, Node value)
        {
            Nodes = new List<Node>();

            Nodes.Add(assignee);
            Nodes.Add(value);
        }

        public override List<Node> Nodes { get; set; }

        public override string GetValue()
        {
            throw new NotImplementedException();
        }

        public override List<Expression> Parse()
        {

            if (Nodes.Count != 2) throw new ArgumentException("Error node! you have more then 2 assign in the node list");

            return new List<Expression>()
            {
                new Expression(01, Nodes[0].GetValue(), Nodes[1].GetValue())
            };

        }
    }
}
