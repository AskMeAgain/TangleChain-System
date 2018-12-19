using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class BiggerEqualNode: OperationNode
    {
        public BiggerEqualNode(Node left, Node right) : base(left, right, 22)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = base.Compile(scope, context);
            var lastResult = list.Last().Args2;
            list.Add(new Expression(26, lastResult));
            list.Add(new Expression(00, lastResult, context + "-Result"));

            return list;
        }
    }
}
