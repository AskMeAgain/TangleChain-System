using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class VariableNode : Node
    {

        public string Name { get; protected set; }
        public string Type { get; protected set; }


        public VariableNode(string name, string type, ExpressionNode expNode)
        {
            Name = name;
            Type = type;
            Nodes.Add(expNode);
        }

        public VariableNode(string name)
        {
            Name = name;
        }

        public override List<Expression> Compile(string context = null)
        {
            return new List<Expression>() {
                new Expression(00, Name, context + "-Variable")
            };
        }
    }
}
