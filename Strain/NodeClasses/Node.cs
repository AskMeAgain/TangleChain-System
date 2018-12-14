using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
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
