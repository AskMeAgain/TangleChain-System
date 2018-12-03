using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
