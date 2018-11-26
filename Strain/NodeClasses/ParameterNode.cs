using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class ParameterNode : Node
    {
        private string _name;
        private string _type;

        public ParameterNode(string name, string type)
        {
            _name = name;
            _type = type;
        }
    }
}
