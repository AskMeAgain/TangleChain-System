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

        public ExpressionHelper(string expression)
        {
            _expression = expression.Replace(";", "").Split(" ").ToList();
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

        public List<ParameterNode> GetParameters()
        {

            var s = String.Join(" ", _expression);

            var array = s.Split(new string[] { "(", ",", ")", "{" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (array.Count() < 3) return new List<ParameterNode>();

            array.RemoveAt(0);

            var list = new List<ParameterNode>();

            for (int i = 0; i < array.Count; i++)
            {

                var result = array[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                list.Add(new ParameterNode(result[1], result[0]));
            }

            return list;

        }

        public QuestionNode GetQuestion()
        {

            var s = String.Join(" ", _expression);

            var start = s.IndexOf('(');
            var end = s.IndexOf(')');

            var question = s.Substring(start + 1, (end - 1) - start);

            return new QuestionNode(question);

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
            return String.Join("", _expression.GetRange(startIndex, _expression.Count - startIndex));
        }
    }
}
