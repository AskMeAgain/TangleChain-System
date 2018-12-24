using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ExpressionNode : Node
    {
        public string _expression { get; protected set; }

        public ExpressionNode(string exp)
        {
            _expression = Utils.ShrinkExpression(exp);

            Nodes.Add(Utils.ExpressionStringToNode(_expression));
        }

        public override List<Expression> Compile(Scope scope, ParserContext context = null)
        {
            return Nodes.Compile(scope, context,"Expression");
        }

    }
}
