using System;
using System.Collections.Generic;
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
            Nodes = new List<Node>() { expNode };
        }

        public VariableNode(string name)
        {
            Name = name;
        }

        public override List<Expression> Compile(string context = null)
        {
            var list = new List<Expression>();
            list.Add(new Expression(00, Name, context + "-Variable"));
            return list;
        }
    }
}
