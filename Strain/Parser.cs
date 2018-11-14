using System;
using System.Collections.Generic;
using System.Text;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain
{
    public class Parser
    {
        public Node Node { get; set; }

        public Parser(Node node)
        {
            Node = node;
        }

        public List<Expression> Parse()
        {
            return Node.Parse();
        }
    }
}
