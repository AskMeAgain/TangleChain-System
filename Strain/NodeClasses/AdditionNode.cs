using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class AdditionNode : Node
    {

        public Node Left { get; protected set; }
        public Node Right { get; protected set; }

        public AdditionNode(Node right, Node left)
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

            list.Add(new Expression(03, leftResult, rightResult, context + "-Temp"));
            list.Add(new Expression(00, context + "-Temp", context + "-Result"));
            return list;
        }
    }
}
