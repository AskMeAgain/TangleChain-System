using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class EqualNode : OperationNode
    {
        public EqualNode(ParserNode right, ParserNode left) : base(right, left, 24)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return base.Compile(scope, context);
        }
    }
}
