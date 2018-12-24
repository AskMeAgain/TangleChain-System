using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public abstract class OperationNode : ParserNode
    {
        public ParserNode Left { get; protected set; }
        public ParserNode Right { get; protected set; }
        public int AssemblyOperation { get; protected set; }

        public OperationNode(ParserNode right, ParserNode left, int assemblyOperation)
        {
            Left = left;
            Right = right;
            AssemblyOperation = assemblyOperation;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();

            list.AddRange(Left.Compile(scope, context.NewContext()));
            string leftResult = list.Last().Args2;

            list.AddRange(Right.Compile(scope, context.NewContext()));
            string rightResult = list.Last().Args2;

            list.Add(new Expression(AssemblyOperation, leftResult, rightResult, context + "-Temp"));
            list.Add(new Expression(00, context + "-Temp", context + "-Result"));
            return list;
        }
    }
}
