using System;
using System.Collections.Generic;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class SmallerNode : OperationNode
    {

        public SmallerNode(Node left, Node right) : base(right, left, 22)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return base.Compile(scope, context);
        }
    }
}
