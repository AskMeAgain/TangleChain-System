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

        public string this[int index] {
            get {
                return _expression[index];
            }
            set {
                _expression[index] = value;
            }
        }
    }
}
