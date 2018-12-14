using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class MultiplicationNode : Node
    {

        public Node Left { get; protected set; }
        public Node Right { get; protected set; }

        public MultiplicationNode(Node right, Node left)
        {
            Left = left;
            Right = right;
        }

        public override List<Expression> Compile(Scope scope,ParserContext context = null)
        {
            var list = new List<Expression>();

            list.AddRange(Left.Compile(scope,context.NewContext()));
            string leftResult = list.Last().Args2;
            list.AddRange(Right.Compile(scope,context.NewContext()));
            string rightResult = list.Last().Args2;

            list.Add(new Expression(04, leftResult, rightResult, context + "-2-Result"));
            list.Add(new Expression(00, context + "-2-Result", context + "-3-Result"));
            return list;
        }
    }
}
