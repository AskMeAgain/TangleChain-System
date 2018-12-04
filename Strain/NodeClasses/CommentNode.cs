using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class CommentNode : Node
    {
        public string Comment { get; protected set; }

        public CommentNode(string comment)
        {
            Comment = comment;
        }

        public override List<Expression> Compile(string context = null)
        {
            return new List<Expression>();
        }
    }
}
