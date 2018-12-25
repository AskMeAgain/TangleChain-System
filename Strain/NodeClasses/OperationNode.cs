using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public abstract class OperationNode : Node
    {
        public Node Left { get; protected set; }
        public Node Right { get; protected set; }
        public int AssemblyOperation { get; protected set; }

        public OperationNode(Node right, Node left, int assemblyOperation)
        {
            Left = left;
            Right = right;
            AssemblyOperation = assemblyOperation;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            list.AddRange(Left.Compile(scope, context.NewContext()));
            string leftResult = list.Last().Args3;

            list.AddRange(Right.Compile(scope, context.NewContext()));
            string rightResult = list.Last().Args3;

            list.Add(new Expression(AssemblyOperation, leftResult, rightResult, context + "-Result"));

            return list;
        }
    }
}
