using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class FunctionCallNode : Node
    {
        private string _label;

        public FunctionCallNode(string label)
        {
            _label = label;
        }

        public override List<Expression> Compile(string context)
        {
            return new List<Expression>() {
                new Expression(19, _label)
            };
        }
    }
}
