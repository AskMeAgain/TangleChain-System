using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class ParameterNode : Node
    {
        public string ParameterName{ get; protected set; }
        public string ParameterType{ get; protected set; }

        public ParameterNode(string parameterName, string parameterType)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
        }
    }
}
