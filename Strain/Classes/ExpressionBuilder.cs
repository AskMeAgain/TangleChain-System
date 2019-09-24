using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Strain.NodeClasses;

namespace Strain.Classes
{
    public class ExpressionBuilder
    {

        private Stack<Node> _valueStack;
        private Stack<string> _symbolStack;
        private string _expression;

        public ExpressionBuilder(string expression)
        {
            _expression = expression;
            _valueStack = new Stack<Node>();
            _symbolStack = new Stack<string>();
        }

        public Node BuildExpression()
        {

            //first push everything on stack
            var helper = new ExpressionHelper(_expression);

            var symbolDictionary = new Dictionary<string, int>() {
                {"+", 0}, {"-", 0}, {"*", 1}
            };

            for (int i = 0; i < helper.Length; i++)
            {

                //first we compare the stack with our current value.
                //If true we pop 2 of valuestack and put together into the normal value stack!
                var currentSymbol = helper[i];
                var containsKey = symbolDictionary.ContainsKey(currentSymbol);

                while (containsKey && _symbolStack.Count > 0 && symbolDictionary[currentSymbol] <= symbolDictionary[_symbolStack.Peek()])
                {
                    CombineNodesToStack(_symbolStack.Pop());
                }

                //after we did this we just push everything to the correct stack:
                if (containsKey)
                {
                    _symbolStack.Push(currentSymbol);
                }
                else
                {
                    _valueStack.Push(ConvertStringToNode(currentSymbol));
                }
            }

            //we now empty the stack
            while (_symbolStack.Count > 0)
            {
                //we combine until empty
                CombineNodesToStack(_symbolStack.Pop());
            }

            return _valueStack.Pop();
        }

        private void CombineNodesToStack(string symbol)
        {
            if (symbol.Equals("+"))
            {
                _valueStack.Push(new AdditionNode(_valueStack.Pop(), _valueStack.Pop()));
            }

            if (symbol.Equals("-"))
            {
                _valueStack.Push(new SubtractionNode(_valueStack.Pop(), _valueStack.Pop()));
            }

            if (symbol.Equals("*"))
            {
                _valueStack.Push(new MultiplicationNode(_valueStack.Pop(), _valueStack.Pop()));
            }
        }

        public Node BuildQuestion()
        {

            var operationStack = new Stack<string>();
            var assertionStack = new Stack<Node>();

            var array = Regex.Split(_expression, @"((?<=\|\||&&)|(?=\|\||&&))")
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

        public Node BuildAssertion()
        {

            var helper = new ExpressionHelper(_expression.Trim());

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

        public Node ConvertStringToNode(string exp)
        {
            var helper = new ExpressionHelper(Utils.StretchExpression(exp));
            var stringInBrackets = helper.GetStringInBrackets();

            //its an string or int
            if (exp.StartsWith("\"") || int.TryParse(exp, out int result))
            {
                return new ValueNode(exp);
            }

            //special node!
            if (helper[0].Equals("_LENGTH"))
            {
                return new LengthNode(stringInBrackets);
            }

            if (helper[0].Equals("_META"))
            {
                return new MetaNode(helper[helper.Length - 2]);
            }

            if (helper[0].Equals("_DATA"))
            {
                return new DataNode(helper[helper.Length - 2]);
            }

            //functioncall
            if (exp.Contains("("))
            {
                //we need to get the values from the functioncall
                var strings = stringInBrackets.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                return new FunctionCallNode(helper[0], strings.Select(x => new ExpressionNode(x)).Cast<Node>().ToList());
            }

            if (exp.Contains("["))
            {
                return new ArrayNode(exp.Substring(0, exp.IndexOf("[")), exp.Substring(exp.IndexOf("[") + 1, exp.Length - 2 - exp.IndexOf("[")));
            }

            //its a variable
            return new VariableNode(helper[0]);

        }
    }
}
