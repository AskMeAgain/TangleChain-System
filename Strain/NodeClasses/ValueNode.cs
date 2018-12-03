using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class ValueNode : Node
    {
        public string Value { get; protected set; }
        public string Type { get; protected set; }

        public ValueNode(string value, string type)
        {
            Value = value;
            Type = type;
        }
    }
}
