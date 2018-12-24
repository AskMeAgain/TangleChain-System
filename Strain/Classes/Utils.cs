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
        private static Stack<Node> valueStack = new Stack<Node>();
        private static Stack<string> symbolStack = new Stack<string>();

        public static List<Expression> Compile(this List<Node> nodes, Scope scope, ParserContext context, string contextName = null)
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

        public static Node ConvertStringToNode(string exp)
        {
            var helper = new ExpressionHelper(StretchExpression(exp));
            var stringInBrackets = helper.GetStringInBrackets();

            //its an string or int
            if (exp.StartsWith('"') || int.TryParse(exp, out int result))
            {
                return new ValueNode(exp);
            }

            //special node!
            if (helper[0].Equals("_LENGTH"))
            {
                return new LengthNode(stringInBrackets);
            }

            //functioncall
            if (exp.Contains("("))
            {
                //we need to get the values from the functioncall
                var strings = stringInBrackets.Split(",", StringSplitOptions.RemoveEmptyEntries);
                return new FunctionCallNode(helper[0], strings.Select(x => new ExpressionNode(x)).Cast<Node>().ToList());
            }

            if (exp.Contains("["))
            {
                return new ArrayNode(exp.Substring(0, exp.IndexOf("[")), exp.Substring(exp.IndexOf("[") + 1, exp.Length - 2 - exp.IndexOf("[")));
            }

            //its a variable
            return new VariableNode(helper[0]);

        }

        public static Node ExpressionStringToNode(string expression)
        {

            //first push everything on stack
            var helper = new ExpressionHelper(expression);

            var symbolDictionary = new Dictionary<string, int>() {
                {"+", 0}, {"-", 0}, {"*", 1}
            };

            for (int i = 0; i < helper.Length; i++)
            {

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
            while (symbolStack.Count > 0)
            {
                //we combine until empty
                CombineNodesToStack(symbolStack.Pop());
            }

            return valueStack.Pop();

        }

        private static void CombineNodesToStack(string symbol)
        {
            if (symbol.Equals("+"))
            {
                valueStack.Push(new AdditionNode(valueStack.Pop(), valueStack.Pop()));
            }

            if (symbol.Equals("-"))
            {
                valueStack.Push(new SubtractionNode(valueStack.Pop(), valueStack.Pop()));
            }

            if (symbol.Equals("*"))
            {
                valueStack.Push(new MultiplicationNode(valueStack.Pop(), valueStack.Pop()));
            }
        }

        public static Node CreateNodeFromAssertion(string assertion)
        {
            var helper = new ExpressionHelper(assertion.Trim());

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
                //bigger is smaller but reversed order of parameter
                return new SmallerNode(right, left);
            }

            if (helper[1].Equals("<="))
            {
                //smaller equal is equal to Negate(Bigger()), which is equal to negate(smaller(reverse order));
                return new NegateNode(new SmallerNode(right, left));
            }

            if (helper[1].Equals(">="))
            {
                //we just use the same node but reverse the things
                return new NegateNode(new SmallerNode(left, right));
            }

            throw new NotImplementedException("Other assertions are not implemented");
        }

        public static Node CreateNodeFromQuestion(string question)
        {
            var operationStack = new Stack<string>();
            var assertionStack = new Stack<Node>();

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
                    assertionStack.Push(CreateNodeFromAssertion(array[i]));
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
