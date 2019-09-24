using System;
using System.Collections.Generic;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public abstract class Node
    {
        public List<Node> Nodes { get; protected set; } = new List<Node>();

        public virtual List<Expression> Compile(Scope scope = null, ParserContext context = null)
        {
            throw new NotImplementedException("not implemented!");
        }
    }
}
