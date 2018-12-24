using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            
            return list;
        }

        public string GetStringInBrackets()
        {

            try {
                var s = String.Join(" ", _expression);

                var start = s.IndexOf('(');
                var end = s.LastIndexOf(')');

                var question = s.Substring(start + 1, (end - 1) - start);

                return question.Trim();
            }
            catch (Exception ) {
                return "";
            }

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
    }
}
