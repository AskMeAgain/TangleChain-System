using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class MultiplicationNode : OperationNode
    {

        public MultiplicationNode(Node right, Node left) : base(right, left, 4)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return base.Compile(scope, context);
        }
    }
}
