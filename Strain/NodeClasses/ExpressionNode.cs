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
            _expression = exp.Replace(" [ ", "[")
                .Replace(" ]", "]").Replace(" ( ", "(").Replace(" )", ")");

            Nodes.Add(ExpressionToNode(_expression));

        }

        public override List<Expression> Compile(Scope scope, ParserContext context = null)
        {
            return Nodes.SelectMany(x => x.Compile(scope, context.NewContext("Expression"))).ToList();
        }

        Stack<Node> valueStack = new Stack<Node>();
        Stack<string> symbolStack = new Stack<string>();

        public Node ExpressionToNode(string expression)
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
                    valueStack.Push(ConvertStringToNode(currentSymbol));
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

        public Node ConvertStringToNode(string exp)
        {
            ;
            //its a string
            if (exp.StartsWith('"') && exp.EndsWith('"'))
            {
                return new ValueNode(exp);
            }

            //its an int
            var isInteger = int.TryParse(exp, out int result);
            if (isInteger)
            {
                ;
                return new ValueNode(exp);
            }

            //its a long
            var isLong = long.TryParse(exp, out long result2);
            if (isLong)
            {
                return new ValueNode(exp);
            }

            //functioncall
            if (exp.Contains("("))
            {
                var helper = new ExpressionHelper(exp.Replace("(", " ( ").Replace(")", " ) "));

                //special call
                var stringInBrackets = helper.GetStringInBrackets();

                if (exp.StartsWith("_LENGTH"))
                {
                    ;
                    return new LengthNode(stringInBrackets);
                }

                //we need to get the values from the functioncall

                var strings = stringInBrackets.Split(",", StringSplitOptions.RemoveEmptyEntries);
                return new FunctionCallNode(helper[0], strings.Select(x => new ExpressionNode(x)).Cast<Node>().ToList());
            }

            if (exp.Contains("["))
            {
                return new ArrayNode(exp.Substring(0, exp.IndexOf("[")), exp.Substring(exp.IndexOf("[") + 1, exp.Length - 2 - exp.IndexOf("[")));
            }

            //its a variable
            return new VariableNode(exp);


        }
    }
}
