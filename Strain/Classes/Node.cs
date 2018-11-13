using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public abstract class Node
    {
        public abstract List<Node> Nodes { get; set; }

        public abstract List<Expression> Parse();

        public abstract string GetValue();

    }
}
