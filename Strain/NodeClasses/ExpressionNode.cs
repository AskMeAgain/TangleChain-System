using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ExpressionNode : ParserNode
    {
        public string _expression { get; protected set; }

        public ExpressionNode(string exp)
        {
            _expression = exp.Replace(" [ ", "[")
                .Replace(" ]", "]").Replace(" ( ", "(").Replace(" )", ")");

            Nodes.Add(ExpressionToNode(_expression));

        }

        public override List<Expression> Compile(Scope scope, ParserContext context = null)
        {
            return Nodes.SelectMany(x => x.Compile(scope, context.NewContext("Expression"))).ToList();
        }

        Stack<ParserNode> valueStack = new Stack<ParserNode>();
        Stack<string> symbolStack = new Stack<string>();

        public ParserNode ExpressionToNode(string expression)
        {
            ;
            //first push everything on stack
            var helper = new ExpressionHelper(expression);

            var symbolDictionary = new Dictionary<string, int>() {
                {"+", 0}, {"-", 0}, {"*", 1}
            };

            for (int i = 0; i < helper.Length; i++)
            {
                ;
                //first we compare the stack with our current value.
                //If true we pop 2 of valuestack and put together into the normal value stack!
                var currentSymbol = helper[i];

                var containsKey = symbolDictionary.ContainsKey(currentSymbol);

                while (containsKey && symbolStack.Count > 0 && symbolDictionary[currentSymbol] <= symbolDictionary[symbolStack.Peek()])
                {
                    CombineNodesToStack(symbolStack.Pop());
                }

                //after we did this we just push everything to the correct stack:
                if (containsKey)
                {
                    symbolStack.Push(currentSymbol);
                }
                else
                {
                    valueStack.Push(ExpressionHelper.ConvertStringToNode(currentSymbol));
                }
            }

            //we now empty the stack
            while (valueStack.Count > 0 || symbolStack.Count > 0)
            {

                if (valueStack.Count == 1 && symbolStack.Count == 0)
                {
                    return valueStack.Pop();
                }

                //we combine until empty
                CombineNodesToStack(symbolStack.Pop());
            }

            throw new Exception("This should really not happen!");

        }

        private void CombineNodesToStack(string symbol)
        {
            ;
            if (symbol.Equals("+"))
            {
                var addNode = new AdditionNode(valueStack.Pop(), valueStack.Pop());
                valueStack.Push(addNode);
            }

            if (symbol.Equals("-"))
            {
                var subNode = new SubtractionNode(valueStack.Pop(), valueStack.Pop());
                valueStack.Push(subNode);
            }

            if (symbol.Equals("*"))
            {
                var mulNode = new MultiplicationNode(valueStack.Pop(), valueStack.Pop());
                valueStack.Push(mulNode);
            }
        }
    }
}
