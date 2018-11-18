using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class OperationNode : Node
    {

        private string _operation;

        public OperationNode(string operation, Node left, Node right)
        {
            _operation = operation;

            Nodes = new List<Node>() {
                left, right
            };
        }

        public override List<Expression> Compile(string context)
        {

            int byteCode = -1;

            switch (_operation)
            {
                case "+":
                    byteCode = 03;
                    break;
                case "-":
                    byteCode = 12;
                    break;
                case "*":
                    byteCode = 04;
                    break;
                default:
                    throw new ArgumentException("not supported!");

            }

            var left = Nodes[0].Compile($"{context}-0");
            var right = Nodes[1].Compile($"{context}-1");

            var destinationOfLeft = left.Last().Args2;
            var destinationOfRight = right.Last().Args2;

            var list = new List<Expression>(left);
            list.AddRange(right);

            list.Add(new Expression(byteCode, destinationOfLeft, destinationOfRight, context + "-result"));

            return list;
        }
    }
}
