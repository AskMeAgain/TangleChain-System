using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class AssignNode : Node
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }

        public AssignNode(string name, string type, Node node)
        {
            Name = name;
            Type = type;
            Nodes = new List<Node>() { node };
        }

    }
}
