using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class SubtractionNode : Node
    {

        public Node Left { get; protected set; }
        public Node Right { get; protected set; }

        public SubtractionNode(Node right, Node left)
        {
            Left = left;
            Right = right;
        }

        public override List<Expression> Compile(string context)
        {
            var list = new List<Expression>();

            list.AddRange(Left.Compile(context + "-0"));
            string leftResult = list.Last().Args2;
            list.AddRange(Right.Compile(context + "-1"));
            string rightResult = list.Last().Args2;

            list.Add(new Expression(12, leftResult, rightResult, context + "-2-Result"));
            list.Add(new Expression(00, context + "-2-Result", context + "-3-Result"));
            return list;
        }
    }
}
