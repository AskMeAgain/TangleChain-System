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
            set { _expression[index] = value; }
        }

        public string Last()
        {
            return _expression.Last();
        }

        public string Type(int index)
        {

            if (_expression[index].Contains("\"")) return "Str";

            return "Int";

        }

        public List<Node> GetParameterNodesFromFunctionCreation() {
            
            //the things in the brackets
            var array = GetStringInBrackets().Split(",", StringSplitOptions.RemoveEmptyEntries);

            if (array.Length == 0) return new List<Node>();

            var list = new List<Node>();

            for (int i = 0; i < array.Length; i++)
            {
                list.Add(new ParameterNode(array[i].Trim(), this[1]));
            }

            ;
            return list;

        }

        public string GetStringInBrackets()
        {

            var s = String.Join(" ", _expression);

            var start = s.IndexOf('(');
            var end = s.IndexOf(')');

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

        public List<Node> GetParameterNodesFromFunctionCall() {
            
            //the things in the brackets
            var array = GetStringInBrackets().Split(",", StringSplitOptions.RemoveEmptyEntries);

            if (array.Length == 0) return new List<Node>();

            var list = new List<Node>();

            for (int i = 0; i < array.Length; i++)
            {

                list.Add(new ParameterNode(array[i].Trim(), this[0]));
            }

            ;
            return list;


        }
    }
}
