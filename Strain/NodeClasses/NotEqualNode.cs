using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class NotEqualNode : Node
    {
        public Node Right { get; set; }
        public Node Left { get; set; }

        public NotEqualNode(Node left, Node right)
        {
            Left = left;
            Right = right;
        }

        public override List<Expression> Compile(Scope scope,ParserContext context) {
            
            var list = new List<Expression>();
            list.AddRange(Left.Compile(scope,context.NewContext()));
            var leftRegister = list.Last().Args2;

            list.AddRange(Right.Compile(scope,context.NewContext()));
            var rightRegister = list.Last().Args2;

            list.Add(new Expression(24, leftRegister, rightRegister, context + "-Temp"));
            list.Add(new Expression(26, context + "-Temp"));
            list.Add(new Expression(00, context + "-Temp", context + "-Result"));

            return list;
        }
    }
}
