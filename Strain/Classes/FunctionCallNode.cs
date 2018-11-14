using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public class FunctionCallNode : Node
    {
        private string _label;

        public FunctionCallNode(string label)
        {
            _label = label;
        }

        public new List<Node> Nodes { get; set; }

        public override List<Expression> Parse()
        {
            var list = new List<Expression>();

            list.Add(new Expression(19, _label));

            return list;
        }
    }
}
