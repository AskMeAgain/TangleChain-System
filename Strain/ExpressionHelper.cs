using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strain
{
    public class ExpressionHelper
    {
        private List<string> _expression;

        public ExpressionHelper(string expression)
        {
            _expression = expression.Replace(";","").Split(" ").ToList();

        }

        public int Length {
            get => _expression.Count;
        }

        public string this[int index] {
            get {
                return _expression[index];
            }
            set {
                _expression[index] = value;
            }
        }

        public string Last() {
            return _expression.Last();
        }
    }
}
