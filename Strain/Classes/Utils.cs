using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StrainLanguage.NodeClasses;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.Classes
{
    public static class Utils
    {
        private static Stack<ParserNode> valueStack = new Stack<ParserNode>();
        private static Stack<string> symbolStack = new Stack<string>();

        public static List<Expression> Compile(this List<ParserNode> nodes, Scope scope, ParserContext context, string contextName = null)
        {
            return nodes.SelectMany(x => x.Compile(scope, context.NewContext(contextName))).ToList();
        }

        public static string ShrinkExpression(string exp)
        {
            return exp.Replace(" [ ", "[").Replace(" ]", "]").Replace(" ( ", "(").Replace(" )", ")");
        }

        public static string StretchExpression(string exp)
        {
            return exp.Replace("[", " [ ").Replace("]", " ]").Replace("(", " ( ").Replace(")", " )");
        }

        public static ParserNode ConvertStringToNode(string exp)
        {

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
            if (long.TryParse(exp, out long result2))
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
                return new FunctionCallNode(helper[0], strings.Select(x => new ExpressionNode(x)).Cast<ParserNode>().ToList());
            }

            if (exp.Contains("["))
            {
                return new ArrayNode(exp.Substring(0, exp.IndexOf("[")), exp.Substring(exp.IndexOf("[") + 1, exp.Length - 2 - exp.IndexOf("[")));
            }

            //its a variable
            return new VariableNode(exp);

        }

        public static ParserNode ExpressionStringToNode(string expression)
        {


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
                    valueStack.Push(Utils.ConvertStringToNode(currentSymbol));
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

        private static void CombineNodesToStack(string symbol)
        {
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

        public static ParserNode CreateNodeFromAssertion(string assertion)
        {
            var helper = new ExpressionHelper(assertion.Trim());
            ;
            if (helper.Length != 3) throw new Exception("Assertion is not correct!");

            var left = ConvertStringToNode(helper[0]);
            var right = ConvertStringToNode(helper[2]);

            if (helper[1].Equals("=="))
            {
                return new EqualNode(left, right);
            }

            if (helper[1].Equals("!="))
            {
                return new NegateNode(new EqualNode(left, right));
            }

            if (helper[1].Equals("<"))
            {
                return new SmallerNode(left, right);
            }

            if (helper[1].Equals(">"))
            {
                //bigger is smaller bug reversed order of parameter
                return new SmallerNode(right, left);
            }

            if (helper[1].Equals("<="))
            {
                //smaller equal is equal to Nage(Bigger()), which is equal to negate(smaller(reverse order));
                return new NegateNode(new SmallerNode(right, left));
            }

            if (helper[1].Equals(">="))
            {
                //we just use the same node but reverse the things
                return new NegateNode(new SmallerNode(left, right));
            }

            throw new NotImplementedException("Other assertions are not implemented");
        }

        public static ParserNode CreateNodeFromQuestion(string question)
        {
            var operationStack = new Stack<string>();
            var assertionStack = new Stack<ParserNode>();

            var array = Regex.Split(question, @"((?<=\|\||&&)|(?=\|\||&&))")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            for (int i = 0; i < array.Count; i++)
            {

                if (array[i].Equals("&&") || array[i].Equals("||"))
                {
                    operationStack.Push(array[i]);
                }
                else
                {
                    assertionStack.Push(Utils.CreateNodeFromAssertion(array[i]));
                }

                if (operationStack.Count == 1 && assertionStack.Count == 2)
                {
                    //we reduce now
                    if (operationStack.Pop().Equals("&&"))
                    {
                        assertionStack.Push(new AndNode(assertionStack.Pop(), assertionStack.Pop()));
                    }
                    else
                    {
                        assertionStack.Push(new OrNode(assertionStack.Pop(), assertionStack.Pop()));
                    }
                }

            }

            return assertionStack.Pop();
        }
    }
}
