using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.NodeClasses;

namespace StrainLanguage.Classes
{
    public class ExpressionHelper
    {
        private List<string> _expression;
        private string _base;

        public ExpressionHelper(string expression)
        {

            _base = expression.Replace(";", "");
            _expression = _base.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public int Length {
            get => _expression.Count;
        }

        public string this[int index] {
            get { return _expression[index]; }
        }

        public List<ParserNode> GetParameterNodeFromString(string name = null)
        {

            var functionName = name ?? this[1];

            //the things in the brackets
            var array = GetStringInBrackets().Split(",", StringSplitOptions.RemoveEmptyEntries);

            if (array.Length == 0) return new List<ParserNode>();

            var list = new List<ParserNode>();

            for (int i = 0; i < array.Length; i++)
            {
                list.Add(new ParameterNode(array[i].Trim(), functionName));
            }

            ;
            return list;
        }

        public string GetStringInBrackets()
        {

            var s = String.Join(" ", _expression);

            var start = s.IndexOf('(');
            var end = s.LastIndexOf(')');

            var question = s.Substring(start + 1, (end - 1) - start);

            return question.Trim();

        }

        public int IndexOf(string key)
        {
            return _expression.IndexOf(key);
        }

        public bool Contains(string key)
        {
            return _expression.Contains(key);
        }

        public string GetSubList(int startIndex)
        {
            return String.Join(" ", _expression.GetRange(startIndex, _expression.Count - startIndex));
        }

        public override string ToString()
        {
            return _base;
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
    }
}
